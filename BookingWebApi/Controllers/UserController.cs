using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        [HttpPost("getUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _service.GetUsers(currentPage, pageSize);
            return Ok(users);
        }

        // Admin: get user by id
        [Authorize(Roles = "0, 3")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Admin: update user info (name, email, role, active)
        [Authorize(Roles = "0, 3")]
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

        // Admin: activate user
        [Authorize(Roles = "0, 3")]
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _service.Activate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Admin: deactivate user
        [Authorize(Roles = "0, 3")]
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _service.Deactivate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Profile: get current user's profile
        [Authorize]
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
