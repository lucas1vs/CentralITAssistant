using CentralITAssistant.Application.Abstractions.Services;
using System.Net.Http.Json;

namespace CentralITAssistant.Infrastructure.Services.Telegram;

public class TelegramBotService : IMessageChannel
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    public TelegramBotService(HttpClient httpClient, string botToken)
    {
        _httpClient = httpClient;

        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new ArgumentException(
                "O token do bot do Telegram não foi configurado.",
                nameof(botToken));
        }

        _botToken = botToken;
    }

    public async Task SendMessageAsync(
        string recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            throw new ArgumentException(
                "O destinatário não pode ser vazio.",
                nameof(recipient));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "A mensagem não pode ser vazia.",
                nameof(message));
        }

        var request = new
        {
            chat_id = recipient,
            text = message
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/bot{_botToken}/sendMessage",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
