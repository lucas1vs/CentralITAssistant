using System.Reflection;
using CentralITAssistant.Infrastructure.Services.AI;
using Microsoft.Extensions.Configuration;

namespace CentralITAssistant.Tests.Infrastructure.Services.AI;

public class MetaAiLiveIntegrationTests
{
    [Fact]
    [Trait("Category", "MetaLive")]
    public async Task GenerateResponseAsync_ShouldReturnRealResponseFromMeta()
    {
        var runLiveTests = Environment.GetEnvironmentVariable(
            "RUN_META_LIVE_TESTS");

        if (!string.Equals(
            runLiveTests,
            "true",
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var apiAssembly = Assembly.Load("CentralITAssistant.API");

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(apiAssembly, optional: true)
            .Build();

        var apiKey = configuration["Meta:ApiKey"]
            ?? throw new InvalidOperationException(
                "Meta:ApiKey não encontrada nos User Secrets da API.");

        var model = configuration["Meta:Model"]
            ?? "muse-spark-1.3";

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        var aiService = new MetaAiService(
            httpClient,
            apiKey,
            model);

        var prompt = "Responda em português: diga apenas 'Conexão com a Meta funcionando!'.";

        var result = await aiService.GenerateResponseAsync(prompt);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
