using Banking.History.Api.Application.Interface;
using Banking.History.Api.Domain;
using Banking.History.Api.Dto;
using Banking.History.Api.Validator;
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
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Banking.History.Api.Tests.IntegrationTests
{
    public class ValidationDtoIntegration
    {
        private static readonly DateTime ReferenceDate = new(2026, 11, 5, 17, 40, 0, DateTimeKind.Utc);

        private static string CreateUrl(string account)
        {
            return "/api/HistoryAccount"
                + $"?Account={Uri.EscapeDataString(account)}"
                + $"&ReferenceDate={Uri.EscapeDataString(ReferenceDate.ToString("O"))}";
        }

        [Theory]
        [InlineData("")]
        public async Task GetHistory_InvalidAccount_ReturnsBadRequest(
            string account)
        {
            // Arrange
            var movementMock = new Mock<IHistoryMovement>();

            await using var factory = CreateFactory(movementMock);
            using var client = factory.CreateClient();

            var accounUrl = CreateUrl(account);

            // Act
            using var response = await client.GetAsync(accounUrl);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains(
                "The Account field is required.",
                body);

            movementMock.Verify(
                x => x.GetHistoryAsync(
                    It.IsAny<DtoAccount>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetHistory_ValidAccountAndDate_ReturnsOK()
        {
            // Arrange
            var movementMock = new Mock<IHistoryMovement>();

            var accountStatement = new AccountStatement();

            movementMock
                .Setup(x => x.GetHistoryAsync(
                    It.IsAny<DtoAccount>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountStatement);

            await using var factory = CreateFactory(movementMock);
            using var client = factory.CreateClient();

            // Act
            using var response = await client.GetAsync(
                CreateUrl("123456"));

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            movementMock.Verify(
                x => x.GetHistoryAsync(
                    It.Is<DtoAccount>(dto =>
                        dto.Account == "123456" &&
                        dto.ReferenceDate == ReferenceDate),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetHistory_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            await using var factory =
                new WebApplicationFactory<Program>();

            using var client = factory.CreateClient();

            // Act
            using var response = await client.GetAsync(
                CreateUrl("123456"));

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
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
}
