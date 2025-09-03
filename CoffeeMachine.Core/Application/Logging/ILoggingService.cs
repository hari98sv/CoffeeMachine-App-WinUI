namespace CoffeeMachine.Core.Application.Logging;

public interface ILoggingService
{
    Task LogInformationAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message, Exception exception = null);
    Task LogDebugAsync(string message);
    Task LogTraceAsync(string message);
}