using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Testcontainers.MongoDb;
using FitPlan.Application.Common.Interfaces;
using Xunit;

namespace FitPlan.Api.IntegrationTests.Infrastructure;

public class FitPlanWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:7")
        .Build();

    // Stable JWT config — set as env vars early so Program.cs can read them
    private static readonly string JwtSecret = "integration-test-secret-key-must-be-long!";

    public Mock<IEmailService> EmailServiceMock { get; } = new();
    public Mock<IGoogleOAuthService> GoogleOAuthServiceMock { get; } = new();

    // Database name is per-factory to avoid cross-test pollution
    private readonly string _dbName = $"fitplan-test-{Guid.NewGuid():N}";

    public async Task InitializeAsync() => await _mongo.StartAsync();

    public new async Task DisposeAsync()
    {
        await _mongo.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the Test environment so appsettings.Test.json is loaded (provides valid Jwt:Secret).
        // Then override only the per-instance MongoDB settings via in-memory collection.
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = _mongo.GetConnectionString(),
                ["MongoDB:DatabaseName"] = _dbName,
            });
        });

        builder.ConfigureServices(services =>
        {
            // Program.cs uses Serilog two-phase bootstrap logger (CreateBootstrapLogger).
            // When test classes run in parallel, multiple factories concurrently call
            // ReloadableLogger.Freeze() on the same global Log.Logger — the second call throws.
            // Replace all ILoggerFactory registrations with NullLoggerFactory to avoid this.
            var loggerFactories = services.Where(d => d.ServiceType == typeof(ILoggerFactory)).ToList();
            foreach (var d in loggerFactories) services.Remove(d);
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            var emailDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailDesc != null) services.Remove(emailDesc);
            services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);

            var googleDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IGoogleOAuthService));
            if (googleDesc != null) services.Remove(googleDesc);
            services.AddScoped<IGoogleOAuthService>(_ => GoogleOAuthServiceMock.Object);
        });
    }
}
