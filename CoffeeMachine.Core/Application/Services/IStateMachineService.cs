using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Services;

public interface IStateMachineService
{
    Task<Result> InitializeAsync();
    Task<Result> StartAsync();
    Task<Result> StopAsync();
    Task<Result> TransitionToStateAsync(string stateName);
    Task<Result<string>> GetCurrentStateAsync();
    Task<Result<bool>> CanHandleEventAsync(string eventName);
    Task<Result> SendEventAsync(string eventName, object eventData = null);
}
