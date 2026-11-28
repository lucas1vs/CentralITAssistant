
namespace CentralITAssistant.Application.Abstractions.Services;

public interface IAiService
{
    Task<string> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
