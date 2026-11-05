using Banking.Identity.Api.Dto;
using Banking.Identity.Api.JwtService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Banking.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtService;
        private readonly IConfiguration _configuration;
        public AuthController(JwtTokenService jwtService,
                              IConfiguration configuration)
        {
            _jwtService = jwtService;
            _configuration = configuration;

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var username = _configuration["DemoAuth:Username"];
            var password = _configuration["DemoAuth:Password"];

            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                return StatusCode(
                    503,
                    "Demo authentication is not configured.");
            }

            if (!Matches(request.Username, username) ||
                !Matches(request.Password, password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = _jwtService.GenerateToken(request.Username);

            return Ok(new TokenResponseDto
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = _configuration.GetValue<int>(
                    "Jwt:ExpirationMinutes") * 60
            });
        }

        private static bool Matches(string actual, string expected)
        {
            var actualHash = SHA256.HashData(
                Encoding.UTF8.GetBytes(actual));

            var expectedHash = SHA256.HashData(
                Encoding.UTF8.GetBytes(expected));

            return CryptographicOperations.FixedTimeEquals(
                actualHash, expectedHash);
        }
    }
}
