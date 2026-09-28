using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TH1_SOA.Models;

namespace TH1_SOA.Controllers
{
    [ApiController]
    [Route("")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        // Dữ liệu User mẫu
        private static readonly List<User> Users = new()
        {
            new User { IdUser = 1, UserName = "admin", Password = "password123", Token = null }
        };

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // Endpoint: POST /login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = Users.FirstOrDefault(u => u.UserName == request.UserName && u.Password == request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
            }

            var token = GenerateJwtToken(user.UserName);
            user.Token = token;

            return Ok(new { token });
        }

        // Endpoint: GET /auth (Xác thực token)
        [HttpGet("auth")]
        public IActionResult VerifyAuth()
        {
            var currentUser = HttpContext.Items["User"];
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(new { message = "Token hợp lệ.", userName = currentUser });
        }

        // Endpoint: GET /hello (API bài 1 có bảo vệ bằng Middleware)
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            var currentUser = HttpContext.Items["User"];
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Yêu cầu đăng nhập để truy cập." });
            }

            return Ok($"Hello World, {currentUser}!");
        }

        private string GenerateJwtToken(string userName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("username", userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}