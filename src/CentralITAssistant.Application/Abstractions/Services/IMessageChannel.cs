
namespace CentralITAssistant.Application.Abstractions.Services;

public interface IMessageChannel
{
    Task SendMessageAsync(
        string recipient,
        string message,
        CancellationToken cancellationToken = default);
}
