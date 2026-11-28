
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CentralITAssistant.Application.Abstractions.Services;

namespace CentralITAssistant.Infrastructure.Services.AI;

public class MetaAiService : IAiService
{
    private const string ApiUrl = "https://api.meta.ai/v1/responses";
    private const string DefaultModel = "muse-spark-1.3";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public MetaAiService(
        HttpClient httpClient,
        string apiKey,
        string model = DefaultModel)
    {
        _httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException(
                "A chave da Meta API não foi configurada.",
                nameof(apiKey));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException(
                "O modelo da Meta não foi configurado.",
                nameof(model));

        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(
                "O prompt não pode ser vazio.",
                nameof(prompt));

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            ApiUrl);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = JsonContent.Create(new
        {
            model = _model,
            input = prompt
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        // Tratamento detalhado de erros HTTP
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Erro na Meta Model API. " +
                $"Status HTTP: {(int)response.StatusCode}. " +
                $"Detalhes: {responseContent}",
                null,
                response.StatusCode);
        }

        // Leitura da resposta JSON
        using var jsonDocument = JsonDocument.Parse(responseContent);
        var root = jsonDocument.RootElement;

        if (root.TryGetProperty("output", out var output) &&
            output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                if (!item.TryGetProperty("type", out var itemType) ||
                    itemType.GetString() != "message")
                    continue;

                if (!item.TryGetProperty("content", out var content) ||
                    content.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var contentItem in content.EnumerateArray())
                {
                    if (contentItem.ValueKind != JsonValueKind.Object)
                        continue;

                    if (!contentItem.TryGetProperty(
                        "type", out var contentType) ||
                        contentType.GetString() != "output_text")
                        continue;

                    if (contentItem.TryGetProperty("text", out var text) &&
                        text.ValueKind == JsonValueKind.String)
                    {
                        var generatedText = text.GetString();

                        if (!string.IsNullOrWhiteSpace(generatedText))
                            return generatedText;
                    }
                }
            }
        }

        throw new InvalidOperationException(
            "A Meta Model API não retornou um texto válido. " +
            $"Resposta recebida: {responseContent}");
    }
}
