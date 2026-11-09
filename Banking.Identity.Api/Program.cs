using Banking.History.Api.Extension;
using Banking.Identity.Api.Extension;
using Banking.Identity.Api.JwtService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddGlobalExceptionHandling();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
