using CoffeeMachine.Core.Application.Logging;

namespace CoffeeMachine.Infrastructure.Services.Logging;

public class CentralizedLogger : ILoggingService
{
    private readonly ILoggingService _fallbackLogger;
    private readonly string _serviceUrl;

    public CentralizedLogger(string serviceUrl, ILoggingService fallbackLogger = null)
    {
        _serviceUrl = serviceUrl;
        _fallbackLogger = fallbackLogger ?? new ConsoleLogger();
    }

    public async Task LogInformationAsync(string message)
    {
        try
        {
            // Implementation for centralized logging (e.g., HTTP call to logging service)
            await Task.Delay(100); // Simulate network call
            Console.WriteLine($"[CENTRAL-INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        catch
        {
            await _fallbackLogger.LogInformationAsync(message);
        }
    }

    public async Task LogWarningAsync(string message)
    {
        try
        {
            await Task.Delay(100);
            Console.WriteLine($"[CENTRAL-WARN] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        catch
        {
            await _fallbackLogger.LogWarningAsync(message);
        }
    }

    public async Task LogErrorAsync(string message, Exception exception = null)
    {
        try
        {
            await Task.Delay(100);
            Console.WriteLine($"[CENTRAL-ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }
        catch
        {
            await _fallbackLogger.LogErrorAsync(message, exception);
        }
    }

    public async Task LogDebugAsync(string message)
    {
        try
        {
            await Task.Delay(100);
            Console.WriteLine($"[CENTRAL-DEBUG] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        catch
        {
            await _fallbackLogger.LogDebugAsync(message);
        }
    }

    public async Task LogTraceAsync(string message)
    {
        try
        {
            await Task.Delay(100);
            Console.WriteLine($"[CENTRAL-TRACE] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        catch
        {
            await _fallbackLogger.LogTraceAsync(message);
        }
    }
}