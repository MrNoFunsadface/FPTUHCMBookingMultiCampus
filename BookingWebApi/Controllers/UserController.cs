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
    }
}
