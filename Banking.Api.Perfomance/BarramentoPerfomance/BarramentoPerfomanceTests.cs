using Dapper;
using Npgsql;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace Banking.Api.Perfomance.BarramentoPerfomance
{
    public class BarramentoPerfomanceTests
    {
        public sealed class BarramentoPerformanceTests
        {
            private readonly ITestOutputHelper _output;

            public static HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            public BarramentoPerformanceTests(ITestOutputHelper output)
            {
                _output = output;
            }

            [Fact]
            [Trait("Category", "Performance")]
            public async Task Should_Maintain_Correct_Balance_With_Mixed_Concurrent_Operations()
            {
                // Arrange
                const string connectionString =
                    "Host=localhost;Port=5432;Database=banking;" +
                    "Username=banking;Password=BankingLocal2026;";

                await using var dataSource =
                    NpgsqlDataSource.Create(connectionString);

                var accountNumber = await CreateTestAccountAsync(dataSource);

                var token = await GetJwtTokenAsync();

                using var client = new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:8080")
                };

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Debit = 1, Credit = 2
                var operations = new[]
                {
                    new { OperationType = 1, Value = 1.00m }, // -1
                    new { OperationType = 2, Value = 1.00m }, // +1
                    new { OperationType = 1, Value = 1.00m }, // -1
                    new { OperationType = 1, Value = 1.00m }  // -1
                };

                var expectedBalance = operations.Sum(operation =>
                    operation.OperationType == 2
                        ? operation.Value
                        : -operation.Value);

                var timer = Stopwatch.StartNew();

                // Act: executa todas as operações concorrentemente.
                var tasks = operations.Select(async operation =>
                {
                    var request = new
                    {
                        TransactionId = Guid.NewGuid(),
                        AccountNumber = accountNumber,
                        operation.Value,
                        operation.OperationType
                    };

                    using var response = await client.PostAsJsonAsync("/api/TransactionAccount", request);

                    return response.StatusCode;
                });

                var statuses = await Task.WhenAll(tasks);

                timer.Stop();

                // Consulta o resultado real no PostgreSQL.
                await using var connection = await dataSource.OpenConnectionAsync();

                var finalBalance = await connection.ExecuteScalarAsync<decimal>(
                    """
                        SELECT balance
                        FROM accounts
                        WHERE account_number = @AccountNumber;
                        """,
                    new { AccountNumber = accountNumber });

                var transactionCount = await connection.ExecuteScalarAsync<int>(
                    """
                        SELECT COUNT(*)
                        FROM transactions
                        WHERE account_number = @AccountNumber;
                        """,
                    new { AccountNumber = accountNumber });

                // Assert
                Assert.All(statuses, status =>
                    Assert.Equal(HttpStatusCode.OK, status));

                Assert.Equal(operations.Length, transactionCount);
                Assert.Equal(expectedBalance, finalBalance);

                _output.WriteLine($"Account: {accountNumber}");
                _output.WriteLine($"Operations: {operations.Length}");
                _output.WriteLine($"Expected balance: {expectedBalance}");
                _output.WriteLine($"Final balance: {finalBalance}");
                _output.WriteLine($"Elapsed: {timer.Elapsed.TotalMilliseconds:F2} ms");
            }

            [Fact]
            [Trait("Category", "Performance")]
            public async Task Should_Process_100_Concurrent_Credits()
            {
                const string connectionString =
                "Host=localhost;" +
                "Port=5432;" +
                "Database=banking;" +
                "Username=banking;" +
                "Password=BankingLocal2026;" +
                "Pooling=true;" +
                "Maximum Pool Size=100;";

                // Arrange
                const int totalRequests = 19000;
                const decimal creditValue = 1.00m;
                
                var token = await GetJwtTokenAsync();

                await using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Cria uma conta exclusiva para esta execução.
                var accountNumber =
                    await CreateTestAccountAsync(dataSource);

                //using var client = new HttpClient
                //{
                //    BaseAddress = new Uri("http://localhost:8080")
                //};

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var latencies = new ConcurrentBag<double>();
                var statuses = new ConcurrentBag<HttpStatusCode>();

                var totalTimer = Stopwatch.StartNew();

                // Act
                var tasks = Enumerable.Range(0, totalRequests)
                    .Select(async _ =>
                    {
                        var request = new
                        {
                            TransactionId = Guid.NewGuid(),
                            AccountNumber = accountNumber,
                            Value = creditValue,
                            OperationType = 2
                        };

                        var timer = Stopwatch.StartNew();

                        using var response = await client.PostAsJsonAsync(
                            "/api/TransactionAccount",
                            request);

                        timer.Stop();

                        latencies.Add(timer.Elapsed.TotalMilliseconds);
                        statuses.Add(response.StatusCode);
                    });

                await Task.WhenAll(tasks);

                totalTimer.Stop();

                // Assert
                var successCount = statuses.Count(status => status == HttpStatusCode.OK);

                var ordered = latencies.OrderBy(x => x).ToArray();

                var throughput = totalRequests / totalTimer.Elapsed.TotalSeconds;

                _output.WriteLine($"Account: {accountNumber}");
                _output.WriteLine($"Success: {successCount}");
                _output.WriteLine($"Errors: {totalRequests - successCount}");
                _output.WriteLine($"Throughput: {throughput:F2} req/s");

                if (ordered.Length > 0)
                {
                    _output.WriteLine(
                        $"P50: {Percentile(ordered, 0.50):F2} ms");

                    _output.WriteLine(
                        $"P95: {Percentile(ordered, 0.95):F2} ms");

                    _output.WriteLine(
                        $"P99: {Percentile(ordered, 0.99):F2} ms");
                }

                // Verifica o saldo final diretamente no PostgreSQL.
                await using var connection = await dataSource.OpenConnectionAsync();

                var finalBalance = await connection.ExecuteScalarAsync<decimal>(
                        """
                SELECT balance
                FROM accounts
                WHERE account_number = @AccountNumber;
                """,
                        new { AccountNumber = accountNumber });

                var transactionCount =
                    await connection.ExecuteScalarAsync<int>(
                        """
                SELECT COUNT(*)
                FROM transactions
                WHERE account_number = @AccountNumber;
                """,
                        new { AccountNumber = accountNumber });

                _output.WriteLine($"Final balance: {finalBalance}");
                _output.WriteLine($"Transactions: {transactionCount}");

                Assert.Equal(totalRequests, successCount);
                Assert.Equal(totalRequests * creditValue,finalBalance);
                Assert.Equal(totalRequests, transactionCount);
            }


            private sealed class TokenResponse
            {
                [JsonPropertyName("accessToken")]
                public string AccessToken { get; set; } = string.Empty;
            }

            private static async Task<string> GetJwtTokenAsync()
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:8082")
                };

                var login = new
                {
                    Username = "tester",
                    Password = "senha-local-de-teste"
                };

                using var response = await client.PostAsJsonAsync(
                    "/api/Auth",
                    login);

                response.EnsureSuccessStatusCode();

                var tokenResponse =
                    await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                {
                    throw new InvalidOperationException(
                        "Identity API did not return a valid access token.");
                }

                return tokenResponse.AccessToken;
            }
            private static async Task<string> CreateTestAccountAsync(
                NpgsqlDataSource dataSource)
            {
                var accountNumber =
                    $"PERF-{Guid.NewGuid():N}"[..17];

                await using var connection =
                    await dataSource.OpenConnectionAsync();

                await connection.ExecuteAsync(
                    """
            INSERT INTO accounts (account_number, balance)
            VALUES (@AccountNumber, 0);
            """,
                    new { AccountNumber = accountNumber });

                return accountNumber;
            }

            private static double Percentile(
                double[] values,
                double percentile)
            {
                var index =
                    (int)Math.Ceiling(percentile * values.Length) - 1;

                return values[Math.Max(0, index)];
            }
        }
    }
}

