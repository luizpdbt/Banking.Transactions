using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Banking.Identity.Api.JwtService
{
    public class JwtTokenService(IConfiguration configuration)
    {
        public string GenerateToken(string username)
        {
            var secret = configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException(
                    "JWT secret not configured.");

            if (Encoding.UTF8.GetByteCount(secret) < 32)
                throw new InvalidOperationException(
                    "JWT secret must contain at least 32 bytes.");

            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer not configured.");

            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience not configured.");

            var expirationMinutes = configuration
                .GetValue<int>("Jwt:ExpirationMinutes");

            if (expirationMinutes <= 0)
                throw new InvalidOperationException(
                    "JWT expiration must be positive.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
