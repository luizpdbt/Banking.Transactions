using Banking.Barramento.Api.Application;
using Banking.Barramento.Api.Application.Interface;
using Banking.Barramento.Api.Infrastructure.Interface;
using Banking.Barramento.Api.Infrastructure.Persistence;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration
    .GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException(
        "PostgreSQL connection string not configured.");

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(connectionString));

var jwtSettings = builder.Configuration.GetSection("Jwt");

var secret = jwtSettings["Secret"]
    ?? throw new InvalidOperationException(
        "JWT Secret not configured.");

var issuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer not configured.");

var audience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience not configured.");


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<IAccountMovement, AccountMovement>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
