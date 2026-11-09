using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

namespace Banking.History.Api.Extension
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var serviceName = configuration[
                "OpenTelemetry:ServiceName"
            ] ?? "Banking.History.Api";

            var otlpEndpoint = configuration[
                "OpenTelemetry:OtlpEndpoint"
            ] ?? "http://localhost:4317";

            services
                .AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                });

            return services;
        }
    }
}
