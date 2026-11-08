using Banking.History.Api.Application;
using Banking.History.Api.Application.Interface;
using Banking.History.Api.Infrastructure.Interface;
using Banking.History.Api.Infrastructure.Persistence;
using Npgsql;

namespace Banking.History.Api.Extension
{
    public static  class ServiceExtension
    {
        public static IServiceCollection AddApplication(
       this IServiceCollection services)
        {
            services.AddTransient<IHistoryMovement, HistoryMovement>();
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

            services.AddScoped<IHistoryRepository, HistoryRepository>();

            return services;
        }
    }
}
