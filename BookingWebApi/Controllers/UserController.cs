using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;

namespace BookingWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserService _service;

        public UserController(IConfiguration config, IUserService service)
        {
            _config = config;
            _service = service;
        }

        [SwaggerOperation(Summary = "User: Login", Description = "User login with email and password.")]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _service.LoginUser(request.Email, request.Password);
            if (user == null) return Unauthorized("Invalid email or password.");

            if (!user.IsActive) return Unauthorized("User is deactivated.");

            var token = GenerateJSONWebToken(user);
            return Ok(new { token, user.Role });
        }

        [SwaggerOperation(Summary = "User: Register", Description = "User register a new account.")]
        [HttpPost("register")]
        public async Task<IActionResult> SignUpUser([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password,
                Role = request.isLecturer ? 2 : 1,
                IsActive = true
            };

            var newUser = await _service.SignUpUser(user);
            if (newUser == null) return BadRequest("User registration failed! User already existed.");

            var token = GenerateJSONWebToken(newUser);
            return Ok(new { token, user.Role });
        }

        [SwaggerOperation(Summary = "Manager: Get users", Description = "Manager get paginated list of users.")]
        [HttpPost("getUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _service.GetUsers(currentPage, pageSize);
            return Ok(users);
        }

        // Manager: get user by id
        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Get user by id", Description = "Manager get user details by id.")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Manager: update user info (name, email, role, active)
        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Update user", Description = "Manager update user's information including role and active status.")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();

            existing.FullName = request.FullName;
            existing.Email = request.Email;
            existing.Role = request.Role;
            existing.IsActive = request.IsActive;

            var updated = await _service.Update(existing);
            return Ok(updated);
        }

        // Manager: activate user
        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Activate user", Description = "Manager activate a user account.")]
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _service.Activate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Manager: deactivate user
        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Deactivate user", Description = "Manager deactivate a user account.")]
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _service.Deactivate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Profile: get current user's profile
        [Authorize]
        [SwaggerOperation(Summary = "User: Get profile", Description = "Get current user's profile.")]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _service.GetByEmail(email);
            if (user == null) return NotFound();

            var actualPassword = user.Password ?? string.Empty;
            user.Password = new string('*', actualPassword.Length);

            return Ok(user);
        }

        // update current user's profile
        [Authorize]
        [SwaggerOperation(Summary = "User: Update profile", Description = "Update current user's profile.")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _service.GetByEmail(email);
            if (user == null) return NotFound();

            user.FullName = request.FullName;

            var updated = await _service.Update(user);
            if (updated != null) updated.Password = new string('*', (updated.Password ?? string.Empty).Length);

            return Ok(updated);
        }

        // change password
        [Authorize]
        [SwaggerOperation(Summary = "User: Change password", Description = "Change current user's password.")]
        [HttpPut("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _service.GetByEmail(email);
            if (user == null) return NotFound();

            var ok = await _service.ChangePassword(user.UserId, request.CurrentPassword, request.NewPassword);
            if (!ok) return BadRequest("Current password is incorrect or new password invalid.");

            return NoContent();
        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                , _config["Jwt:Audience"]
                , [
                    new(ClaimTypes.Name, user.FullName),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, user.Role.ToString()),
                ]
                , expires: DateTime.Now.AddDays(30)
                , signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public sealed record LoginRequest(string Email, string Password);
        public sealed record SignUpRequest(string FullName, string Email, string Password, bool isLecturer);
        public sealed record UpdateUserRequest(string FullName, string Email, int Role, bool IsActive);
        public sealed record UpdateProfileRequest(string FullName);
        public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    }
}
