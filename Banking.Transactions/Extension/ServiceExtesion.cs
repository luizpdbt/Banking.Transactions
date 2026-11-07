using Banking.Barramento.Api.Application;
using Banking.Barramento.Api.Application.Interface;
using Banking.Barramento.Api.Infrastructure.Interface;
using Banking.Barramento.Api.Infrastructure.Persistence;
using Npgsql;

namespace Banking.Barramento.Api.Extension
{
    public static class ServiceExtesion
    {
       public static IServiceCollection AddApplication(
       this IServiceCollection services)
        {
            services.AddTransient<IAccountMovement, AccountMovement>();
            return services;
        }

        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration
                .GetConnectionString("PostgreSql")
                ?? throw new InvalidOperationException(
                    "PostgreSQL connection string not configured.");

            services.AddSingleton(
                _ => NpgsqlDataSource.Create(connectionString));

            services.AddScoped<IAccountRepository, AccountRepository>();

            return services;
        }
    }
}
