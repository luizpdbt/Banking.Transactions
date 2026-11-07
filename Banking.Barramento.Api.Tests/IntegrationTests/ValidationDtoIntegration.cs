using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Banking.Barramento.Api.Tests.IntegrationTests
{
    public class ValidationDtoIntegration
    {
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000","123456", 100, 1)]
        [InlineData("e43c62c7-c795-420d-8e82-cc4f63847f01","", 100, 1)]
        [InlineData("e43c62c7-c795-420d-8e82-cc4f63847f01","123456", 0, 1)]
        [InlineData("e43c62c7-c795-420d-8e82-cc4f63847f01","123456", -10, 1)]
        [InlineData("e43c62c7-c795-420d-8e82-cc4f63847f01","123456", 100, 99)]
        public async Task TransactionOperation_InvalidDto_ReturnsBadRequest(string transactionId,string accountNumber,int value,int operationType)
        {
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
                        });
                    });

            using var client = factory.CreateClient();

            var request = new
            {
                TransactionId = Guid.Parse(transactionId),
                AccountNumber = accountNumber,
                Value = value,
                OperationType = operationType
            };

            using var response = await client.PostAsJsonAsync("/api/TransactionAccount",request);

            Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        }
    }
    public sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder): AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "integration-test")],"Test");

            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,"Test")));
        }
    }
}
