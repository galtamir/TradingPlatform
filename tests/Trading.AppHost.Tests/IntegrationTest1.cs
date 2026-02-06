using Microsoft.Extensions.Logging;

namespace Trading.AppHost.Tests;

public class WebAppIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task TradingWebReturnsOkOnRoot()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Trading_AppHost>(cancellationToken);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("trading-web");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("trading-web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpointReturnsHealthy()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Trading_AppHost>(cancellationToken);

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("trading-web");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("trading-web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/health", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LoginPageIsAccessible()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Trading_AppHost>(cancellationToken);

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("trading-web");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("trading-web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var response = await httpClient.GetAsync("/Account/Login", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
