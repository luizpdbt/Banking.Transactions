using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Banking.Barramento.Api.Extension
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var jwtSettings = configuration.GetRequiredSection("Jwt");

            var secret = jwtSettings["Secret"]
                ?? throw new InvalidOperationException(
                    "JWT Secret not configured.");

            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer not configured.");

            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience not configured.");

            if (Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw new InvalidOperationException(
                    "JWT Secret must contain at least 32 bytes.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = issuer,

                            ValidateAudience = true,
                            ValidAudience = audience,

                            ValidateLifetime = true,
                            RequireExpirationTime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secret)),

                            RequireSignedTokens = true,
                            ClockSkew = TimeSpan.Zero
                        };
                });

            return services;
        }
    }
}
