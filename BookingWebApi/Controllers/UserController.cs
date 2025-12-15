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

            var token = GenerateJSONWebToken(user);
            return Ok(new {token, user.Role});
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
                Role = 1,
                IsActive = true
            };

            var newUser = await _service.SignUpUser(user);
            if (newUser == null) return BadRequest("User registration failed! User already existed.");

            var token = GenerateJSONWebToken(newUser);
            return Ok(new {token, user.Role});
        }

        [HttpPost("getUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _service.GetUsers(currentPage, pageSize);
            return Ok(users);
        }

        // admin: get user by id
        [Authorize(Roles = "2")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // admin: update user info (name, email, role, active)
        [Authorize(Roles = "2")]
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

        // admin: delete user
        [Authorize(Roles = "2")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var ok = await _service.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // admin: activate user
        [Authorize(Roles = "2")]
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _service.Activate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // admin: deactivate user
        [Authorize(Roles = "2")]
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _service.Deactivate(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // profile: get current user's profile
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var usersPage = await _service.GetUsers(1, 1000);
            var user = usersPage.Items.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

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

            var usersPage = await _service.GetUsers(1, 1000);
            var user = usersPage.Items.FirstOrDefault(u => u.Email == email);
            if (user == null) return NotFound();

            user.FullName = request.FullName;

            var updated = await _service.Update(user);
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

            var usersPage = await _service.GetUsers(1, 1000);
            var user = usersPage.Items.FirstOrDefault(u => u.Email == email);
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
        public sealed record SignUpRequest(string FullName, string Email, string Password);
        public sealed record UpdateUserRequest(string FullName, string Email, int Role, bool IsActive);
        public sealed record UpdateProfileRequest(string FullName);
        public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    }
}
