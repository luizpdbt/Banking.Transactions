using Banking.Barramento.Api.Application.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Barramento.Api.Tests.IntegrationTests
{
    public class TransactionAccountSuccessTests
    {
        [Fact]
        public async Task TransactionOperation_ValidDto_ReturnsOk()
        {
            // Arrange
            var movementMock = new Mock<IAccountMovement>();

            await using var factory =
                new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureTestServices(services =>
                        {
                            services.AddAuthentication(options =>
                            {
                                options.DefaultAuthenticateScheme = "Test";
                                options.DefaultChallengeScheme = "Test";
                            })
                            .AddScheme<AuthenticationSchemeOptions,
                                TestAuthenticationHandler>("Test", _ => { });

                            services.RemoveAll<IAccountMovement>();

                            services.AddScoped(_ => movementMock.Object);
                        });
                    });

            using var client = factory.CreateClient();

            var request = new
            {
                TransactionId = Guid.NewGuid(),
                AccountNumber = "123456",
                Value = 100.00m,
                OperationType = 1
            };

            // Act
            using var response = await client.PostAsJsonAsync("/api/TransactionAccount",request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
