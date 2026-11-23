
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CentralITAssistant.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CentralITAssistant.Infrastructure.Services.Telegram;

public class TelegramPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TelegramPollingService> _logger;
    private readonly string _botToken;

    private long _offset;

    public TelegramPollingService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<TelegramPollingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        _botToken = configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException(
                "O token do bot do Telegram não foi configurado.");
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var httpClient = _httpClientFactory.CreateClient("Telegram");

        _logger.LogInformation(
            "Telegram Polling Service iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var url =
                    $"/bot{_botToken}/getUpdates?timeout=30&offset={_offset}";

                using var response = await httpClient.GetAsync(
                    url,
                    stoppingToken);

                response.EnsureSuccessStatusCode();

                await using var stream =
                    await response.Content.ReadAsStreamAsync(stoppingToken);

                using var document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: stoppingToken);

                var root = document.RootElement;

                if (!root.TryGetProperty("ok", out var okElement) ||
                    !okElement.GetBoolean())
                {
                    throw new InvalidOperationException(
                        "A API do Telegram retornou uma resposta inválida.");
                }

                if (!root.TryGetProperty("result", out var updates))
                {
                    throw new InvalidOperationException(
                        "A resposta do Telegram não contém atualizações.");
                }

                foreach (var update in updates.EnumerateArray())
                {
                    await ProcessUpdateAsync(update, stoppingToken);

                    _offset = update.GetProperty("update_id").GetInt64() + 1;
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao consultar ou processar atualizações do Telegram.");

                try
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        _logger.LogInformation(
            "Telegram Polling Service encerrado.");
    }

    private async Task ProcessUpdateAsync(
        JsonElement update,
        CancellationToken cancellationToken)
    {
        if (!update.TryGetProperty("message", out var message))
            return;

        if (!message.TryGetProperty("chat", out var chat))
            return;

        if (!chat.TryGetProperty("id", out var chatIdElement))
            return;

        if (!message.TryGetProperty("text", out var textElement))
            return;

        var chatId = chatIdElement.GetInt64().ToString(
            CultureInfo.InvariantCulture);

        var receivedMessage = textElement.GetString();

        if (string.IsNullOrWhiteSpace(receivedMessage))
            return;

        _logger.LogInformation(
            "Mensagem recebida do Telegram. Chat ID: {ChatId}",
            chatId);

        var responseMessage = receivedMessage.StartsWith(
            "/start",
            StringComparison.OrdinalIgnoreCase)
            ? "Olá! Sou o Central IT Assistant. Como posso ajudar?"
            : $"Recebi sua mensagem: {receivedMessage}";

        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var messageChannel = scope.ServiceProvider
            .GetRequiredService<IMessageChannel>();

        await messageChannel.SendMessageAsync(
            chatId,
            responseMessage,
            cancellationToken);
    }
}
