namespace CoffeeMachine.Core.Abstractions.Logging;

public interface ITelemetryService
{
    Task TrackEventAsync(string eventName, object properties = null);
    Task TrackMetricAsync(string metricName, double value);
    Task TrackExceptionAsync(Exception exception);
    Task TrackDependencyAsync(string dependencyType, string dependencyName, string data, DateTime startTime, TimeSpan duration, bool success);
}
