using Banking.History.Api.Infraestructure.Interface;
using Banking.History.Api.Infraestructure.Persistence;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration
    .GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException(
        "PostgreSQL connection string not configured.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
