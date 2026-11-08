using Banking.History.Api.Extension;
using Banking.History.Api.Infrastructure.Persistence;
using Banking.History.Api.Infrastructure.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;


namespace Banking.Barramento.Api.Tests.UnitTests
{
    public class ServiceExtesionTests
    {
        private static ServiceProvider CreateServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:PostgreSql"] =
                            "Host=localhost;Port=5432;Database=test;" +
                            "Username=test;Password=test"
                    })
                .Build();

            var services = new ServiceCollection();

            services.AddPersistence(configuration);

            return services.BuildServiceProvider(
                validateScopes: true);
        }

        [Fact]
        public void NpgsqlDataSource_ShouldBeRegisteredAsSingleton()
        {
            // Arrange
            using var provider = CreateServiceProvider();

            // Act
            var instance1 =
                provider.GetRequiredService<NpgsqlDataSource>();

            using var scope = provider.CreateScope();

            var instance2 =
                scope.ServiceProvider
                    .GetRequiredService<NpgsqlDataSource>();

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void AccountRepository_ShouldBeRegisteredAsScoped()
        {
            // Arrange
            using var provider = CreateServiceProvider();

            using var scope1 = provider.CreateScope();
            using var scope2 = provider.CreateScope();

            // Act
            var repository1 =
                scope1.ServiceProvider
                    .GetRequiredService<IHistoryRepository>();

            var repository2 =
                scope1.ServiceProvider
                    .GetRequiredService<IHistoryRepository>();

            var repository3 =
                scope2.ServiceProvider
                    .GetRequiredService<IHistoryRepository>();

            // Assert

            // Mesmo escopo: mesma instância.
            Assert.Same(repository1, repository2);

            // Escopos diferentes: instâncias diferentes.
            Assert.NotSame(repository1, repository3);
        }

        [Fact]
        public void NpgsqlDataSource_ShouldHaveSingletonLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:PostgreSql"] =
                            "Host=localhost;Database=test;" +
                            "Username=test;Password=test"
                    })
                .Build();

            // Act
            services.AddPersistence(configuration);

            var descriptor = services.Single(
                x => x.ServiceType == typeof(NpgsqlDataSource));

            // Assert
            Assert.Equal(
                ServiceLifetime.Singleton,
                descriptor.Lifetime);
        }

        [Fact]
        public void AccountRepository_ShouldHaveScopedLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:PostgreSql"] =
                            "Host=localhost;Database=test;" +
                            "Username=test;Password=test"
                    })
                .Build();

            // Act
            services.AddPersistence(configuration);

            var descriptor = services.Single(
                x => x.ServiceType == typeof(IHistoryRepository));

            // Assert
            Assert.Equal(
                ServiceLifetime.Scoped,
                descriptor.Lifetime);

            Assert.Equal(
                typeof(HistoryRepository),
                descriptor.ImplementationType);
        }
    }
}
