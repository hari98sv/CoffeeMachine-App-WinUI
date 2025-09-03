using CoffeeMachine.Core.Abstractions.Logging;

namespace CoffeeMachine.Infrastructure.Services.Logging;

public class ConsoleLogger : ILoggingService
{
    public async Task LogInformationAsync(string message)
    {
        await Task.Run(() => Console.WriteLine($"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}"));
    }

    public async Task LogWarningAsync(string message)
    {
        await Task.Run(() => Console.WriteLine($"[WARN] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}"));
    }

    public async Task LogErrorAsync(string message, Exception exception = null)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.Message}");
                Console.WriteLine($"Stack Trace: {exception.StackTrace}");
            }
        });
    }

    public async Task LogDebugAsync(string message)
    {
        await Task.Run(() => Console.WriteLine($"[DEBUG] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}"));
    }

    public async Task LogTraceAsync(string message)
    {
        await Task.Run(() => Console.WriteLine($"[TRACE] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}"));
    }
}