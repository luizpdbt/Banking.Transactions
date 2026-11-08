using Banking.History.Api.Application.Common;
using Banking.History.Api.Application.Interface;
using Banking.History.Api.Domain;
using Banking.History.Api.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Banking.History.Api.Tests.IntegrationTests
{
    public class TransactionHistorySuccessTests
    {
        [Fact]
        public async Task GetHistory_ValidAccountAndReferenceDate_ShouldReturn200OK()
        {
            // Arrange
            var movementMock = new Mock<IHistoryMovement>();

            var referenceDate = new DateTime(
                2026, 11, 5, 17, 40, 0,
                DateTimeKind.Utc);

            var accountStatement = new AccountStatement();

            var pagedResult = new PagedResult<AccountStatement>(
                            Items: new[] { accountStatement },
                            Page: 1,
                            PageSize: 20,
                            TotalItems: 1,
                            TotalPages: 1
                );

            movementMock
                .Setup(x => x.GetHistoryAsync(
                    It.Is<DtoAccount>(dto =>
                        dto.Account == "123456" &&
                        dto.ReferenceDate == referenceDate),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            await using var factory = CreateFactory(movementMock);

            using var client = factory.CreateClient();

            var url = "/api/HistoryAccount"
                + "?Account=123456"
                + $"&ReferenceDate={Uri.EscapeDataString(referenceDate.ToString("O"))}";

            // Act
            using var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            movementMock.Verify(
                x => x.GetHistoryAsync(
                    It.Is<DtoAccount>(dto =>
                        dto.Account == "123456" &&
                        dto.ReferenceDate == referenceDate),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static WebApplicationFactory<Program> CreateFactory(
           Mock<IHistoryMovement> movementMock)
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = "Test";
                            options.DefaultChallengeScheme = "Test";
                        })
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            HistoryTestAuthenticationHandler>(
                            "Test", _ => { });

                        services.RemoveAll<IHistoryMovement>();

                        services.AddScoped(
                            _ => movementMock.Object);
                    });
                });
        }
    }

    public sealed class HistoryTestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(
            options, logger, encoder)
    {
        protected override Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(
                    ClaimTypes.Name,
                    "integration-test")
                ],
                "Test");

            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(
                AuthenticateResult.Success(
                    new AuthenticationTicket(
                        principal,
                        "Test")));
        }
    }
}
