using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Events;

namespace CoffeeMachine.Core.Application.Services;

 public interface IStateMachineService
    {
        Task<Result> InitializeAsync();
        Task<Result> StartAsync();
        Task<Result> StopAsync();
        Task<Result> TransitionToStateAsync(string stateName, MachineEvent triggerEvent = null);
        Task<Result<string>> GetCurrentStateAsync();
        Task<Result<bool>> CanHandleEventAsync(MachineEvent machineEvent);
        Task<Result> SendEventAsync(MachineEvent machineEvent);
        Task<Result<bool>> IsInStateAsync(string stateName);
        Task<Result<List<string>>> GetAvailableStatesAsync();
        Task<Result<BeveragePreparationContext>> GetPreparationContextAsync(Guid orderId);
}

public class BeveragePreparationContext
{
    public BeveragePreparationContext()
        {}
    public BeveragePreparationContext(Guid orderId, string currentState, int progressPercentage, string currentStep, DateTime startTime, DateTime? estimatedCompletionTime, string errorMessage, Dictionary<string, object> parameters)
    {
        OrderId = orderId;
        CurrentState = currentState;
        ProgressPercentage = progressPercentage;
        CurrentStep = currentStep;
        StartTime = startTime;
        EstimatedCompletionTime = estimatedCompletionTime;
        ErrorMessage = errorMessage;
        Parameters = parameters;
    }

    public Guid OrderId
    {
        get; set;
    }
    public string CurrentState
    {
        get; set;
    }
    public int ProgressPercentage
    {
        get; set;
    }
    public string CurrentStep
    {
        get; set;
    }
    public DateTime StartTime
    {
        get; set;
    }
    public DateTime? EstimatedCompletionTime
    {
        get; set;
    }
    public string ErrorMessage
    {
        get; set;
    }
    public Dictionary<string, object> Parameters { get; set; } = new();
}