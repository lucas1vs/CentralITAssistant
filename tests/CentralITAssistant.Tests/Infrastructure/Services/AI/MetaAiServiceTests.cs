using System.Net;
using System.Text;
using CentralITAssistant.Infrastructure.Services.AI;

namespace CentralITAssistant.Tests.Infrastructure.Services.AI;

public class MetaAiServiceTests
{
    private const string ApiKey = "test-api-key";

    private static MetaAiService CreateService(
        HttpResponseMessage response,
        Action<HttpRequestMessage>? validateRequest = null)
    {
        var handler = new FakeHttpMessageHandler(
            response,
            validateRequest);

        var httpClient = new HttpClient(handler);

        return new MetaAiService(httpClient, ApiKey);
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldReturnGeneratedText_WhenRequestIsSuccessful()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                    "output": [
                        {
                            "type": "message",
                            "content": [
                                {
                                    "type": "output_text",
                                    "text": "Olá! Como posso ajudar?"
                                }
                            ]
                        }
                    ]
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var service = CreateService(response);

        // Act
        var result = await service.GenerateResponseAsync(
            "Olá, assistente!");

        // Assert
        Assert.Equal("Olá! Como posso ajudar?", result);
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldThrowArgumentException_WhenPromptIsEmpty()
    {
        // Arrange
        var service = CreateService(
            new HttpResponseMessage(HttpStatusCode.OK));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GenerateResponseAsync(" "));
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldThrowHttpRequestException_WhenApiReturnsError()
    {
        // Arrange
        var response = new HttpResponseMessage(
            HttpStatusCode.Unauthorized);

        var service = CreateService(response);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GenerateResponseAsync("Olá"));

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            exception.StatusCode);
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldThrowInvalidOperationException_WhenResponseHasNoOutputText()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                    "output": []
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var service = CreateService(response);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateResponseAsync("Olá"));
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                    "output": [
                        {
                            "type": "message",
                            "content": [
                                {
                                    "type": "output_text",
                                    "text": "Resposta"
                                }
                            ]
                        }
                    ]
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var service = CreateService(
            response,
            request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);

                Assert.Equal(
                    "https://api.meta.ai/v1/responses",
                    request.RequestUri!.ToString());

                Assert.Equal(
                    "Bearer",
                    request.Headers.Authorization!.Scheme);

                Assert.Equal(
                    ApiKey,
                    request.Headers.Authorization.Parameter);
            });

        // Act
        await service.GenerateResponseAsync("Teste");

        // Assert
        // As validações da requisição são realizadas pelo handler.
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        private readonly Action<HttpRequestMessage>? _validateRequest;

        public FakeHttpMessageHandler(
            HttpResponseMessage response,
            Action<HttpRequestMessage>? validateRequest = null)
        {
            _response = response;
            _validateRequest = validateRequest;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _validateRequest?.Invoke(request);

            return Task.FromResult(_response);
        }
    }
}
