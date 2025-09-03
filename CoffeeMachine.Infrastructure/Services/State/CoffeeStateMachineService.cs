using CoffeeMachine.Core.Application.Logging;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Events;

namespace CoffeeMachine.Infrastructure.Services.State;

public class CoffeeStateMachineService : IStateMachineService
{
    private readonly ILoggingService _loggingService;
    private readonly IMessagingService _messagingService;
    private readonly IOrderService _orderService;

    private Dictionary<Guid, BeveragePreparationContext> _preparationContexts;
    private string _currentState = "Idle";
    private bool _isInitialized = false;

    public CoffeeStateMachineService(
        ILoggingService loggingService,
        IMessagingService messagingService,
        IOrderService orderService)
    {
        _loggingService = loggingService;
        _messagingService = messagingService;
        _orderService = orderService;
        _preparationContexts = new Dictionary<Guid, BeveragePreparationContext>();
    }

    public async Task<Result> InitializeAsync()
    {
        try
        {
            if (_isInitialized)
            {
                return Result.Success();
            }

            _preparationContexts.Clear();
            _currentState = "Idle";

            await _loggingService.LogInformationAsync("State machine initialized");
            _isInitialized = true;

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Failed to initialize state machine", ex);
            return Result.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public async Task<Result> StartAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }

        _currentState = "Ready";
        await _loggingService.LogInformationAsync("State machine started");
        return Result.Success();
    }

    public async Task<Result> StopAsync()
    {
        _currentState = "Stopped";
        await _loggingService.LogInformationAsync("State machine stopped");
        return Result.Success();
    }

    public async Task<Result> SendEventAsync(MachineEvent machineEvent)
    {
        try
        {
            if (!_isInitialized)
            {
                return Result.Failure("State machine not initialized");
            }

            if (machineEvent is OrderEvent orderEvent)
            {
                return await HandleOrderEvent(orderEvent);
            }

            return await HandleSystemEvent(machineEvent);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error handling event {machineEvent.EventType}", ex);
            return Result.Failure($"Event handling failed: {ex.Message}");
        }
    }

    private async Task<Result> HandleOrderEvent(OrderEvent orderEvent)
    {
        switch (orderEvent.EventType)
        {
            case "StartPreparation":
                return await StartBeveragePreparation(orderEvent);

            case "PreparationProgress":
                return await UpdatePreparationProgress(orderEvent);

            case "PreparationComplete":
                return await CompletePreparation(orderEvent);

            case "PreparationFailed":
                return await FailPreparation(orderEvent);

            case "CancelPreparation":
                return await CancelPreparation(orderEvent);

            default:
                return Result.Failure($"Unknown event type: {orderEvent.EventType}");
        }
    }

    private async Task<Result> HandleSystemEvent(MachineEvent machineEvent)
    {
        switch (machineEvent.EventType)
        {
            case "MaintenanceRequired":
                _currentState = "Maintenance";
                await _loggingService.LogInformationAsync("Maintenance mode activated");
                return Result.Success();

            case "Reset":
                _currentState = "Ready";
                await _loggingService.LogInformationAsync("State machine reset");
                return Result.Success();

            default:
                return Result.Failure($"Unknown system event: {machineEvent.EventType}");
        }
    }

    private async Task<Result> StartBeveragePreparation(OrderEvent orderEvent)
    {
        if (_currentState != "Ready" && _currentState != "Idle")
        {
            return Result.Failure($"Cannot start preparation in state: {_currentState}");
        }

        var context = new BeveragePreparationContext
        {
            OrderId = orderEvent.OrderId,
            CurrentState = "Preparing",
            ProgressPercentage = 0,
            CurrentStep = "Initializing",
            StartTime = DateTime.UtcNow,
            EstimatedCompletionTime = DateTime.UtcNow.AddMinutes(5)
        };

        _preparationContexts[orderEvent.OrderId] = context;
        _currentState = "Preparing";

        // Notify UI via state machine event
        await _messagingService.Publish(new PreparationEvent(
            orderEvent.OrderId,
            orderEvent.BeverageType,
            "PreparationStarted",
            0,
            "Initializing"
        ));

        await _loggingService.LogInformationAsync($"Started preparation for order {orderEvent.OrderId}");
        await _orderService.UpdateOrderStatusAsync(orderEvent.OrderId, "Preparing", 0, "Initializing");

        return Result.Success();
    }

    private async Task<Result> UpdatePreparationProgress(OrderEvent orderEvent)
    {
        if (orderEvent is PreparationEvent preparationEvent)
        {
            if (!_preparationContexts.TryGetValue(preparationEvent.OrderId, out var context))
            {
                return Result.Failure($"No preparation context found for order {preparationEvent.OrderId}");
            }

            context.ProgressPercentage = preparationEvent.Progress;
            context.CurrentStep = preparationEvent.CurrentStep;
            context.CurrentState = "Preparing";

            // Update estimated completion time based on progress
            var elapsed = DateTime.UtcNow - context.StartTime;
            var totalEstimatedTime = elapsed / (context.ProgressPercentage / 100.0);
            context.EstimatedCompletionTime = context.StartTime + totalEstimatedTime;

            // Notify UI via messaging service
            var orderResult = await _orderService.GetOrderAsync(preparationEvent.OrderId);
            if (orderResult.IsSuccess)
            {
                await _messagingService.Publish(new BeverageStatusMessage(
                    orderResult.Data,
                    "Preparing",
                    preparationEvent.Progress,
                    preparationEvent.CurrentStep
                ));
            }

            await _orderService.UpdateOrderStatusAsync(
                preparationEvent.OrderId,
                "Preparing",
                preparationEvent.Progress,
                preparationEvent.CurrentStep
            );

            await _loggingService.LogDebugAsync($"Progress update for order {preparationEvent.OrderId}: {preparationEvent.Progress}%");

            return Result.Success();
        }

        return Result.Failure("Invalid preparation event");
    }

    private async Task<Result> CompletePreparation(OrderEvent orderEvent)
    {
        if (!_preparationContexts.TryGetValue(orderEvent.OrderId, out var context))
        {
            return Result.Failure($"No preparation context found for order {orderEvent.OrderId}");
        }

        context.ProgressPercentage = 100;
        context.CurrentStep = "Completed";
        context.CurrentState = "Completed";
        context.EstimatedCompletionTime = DateTime.UtcNow;

        _currentState = "Ready";

        // Notify UI via messaging service
        var orderResult = await _orderService.GetOrderAsync(orderEvent.OrderId);
        if (orderResult.IsSuccess)
        {
            await _messagingService.Publish(new BeverageStatusMessage(
                orderResult.Data,
                "Completed",
                100,
                "Ready for pickup"
            ));
        }

        await _orderService.CompleteOrderAsync(orderEvent.OrderId);
        await _loggingService.LogInformationAsync($"Completed preparation for order {orderEvent.OrderId}");

        // Clean up context after completion
        _preparationContexts.Remove(orderEvent.OrderId);

        return Result.Success();
    }

    private async Task<Result> FailPreparation(OrderEvent orderEvent)
    {
        if (!_preparationContexts.TryGetValue(orderEvent.OrderId, out var context))
        {
            return Result.Failure($"No preparation context found for order {orderEvent.OrderId}");
        }

        context.CurrentState = "Error";
        context.ErrorMessage = orderEvent.EventData?.ToString() ?? "Unknown error";
        _currentState = "Error";

        // Notify UI via messaging service
        var orderResult = await _orderService.GetOrderAsync(orderEvent.OrderId);
        if (orderResult.IsSuccess)
        {
            await _messagingService.Publish(new BeverageStatusMessage(
                orderResult.Data,
                "Error",
                context.ProgressPercentage,
                $"Error: {context.ErrorMessage}"
            ));
        }

        await _orderService.UpdateOrderStatusAsync(
            orderEvent.OrderId,
            "Error",
            context.ProgressPercentage,
            context.ErrorMessage
        );

        await _loggingService.LogErrorAsync($"Preparation failed for order {orderEvent.OrderId}: {context.ErrorMessage}");

        return Result.Success();
    }

    private async Task<Result> CancelPreparation(OrderEvent orderEvent)
    {
        if (!_preparationContexts.TryGetValue(orderEvent.OrderId, out var context))
        {
            return Result.Failure($"No preparation context found for order {orderEvent.OrderId}");
        }

        context.CurrentState = "Cancelled";
        _currentState = "Ready";

        // Notify UI via messaging service
        var orderResult = await _orderService.GetOrderAsync(orderEvent.OrderId);
        if (orderResult.IsSuccess)
        {
            await _messagingService.Publish(new BeverageStatusMessage(
                orderResult.Data,
                "Cancelled",
                context.ProgressPercentage,
                "Cancelled by user"
            ));
        }

        await _orderService.UpdateOrderStatusAsync(
            orderEvent.OrderId,
            "Cancelled",
            context.ProgressPercentage,
            "Cancelled by user"
        );

        await _loggingService.LogInformationAsync($"Cancelled preparation for order {orderEvent.OrderId}");

        // Clean up context
        _preparationContexts.Remove(orderEvent.OrderId);

        return Result.Success();
    }

    public async Task<Result> TransitionToStateAsync(string stateName, MachineEvent triggerEvent = null)
    {
        var validTransitions = new Dictionary<string, List<string>>
        {
            ["Idle"] = new List<string> { "Ready", "Maintenance" },
            ["Ready"] = new List<string> { "Preparing", "Maintenance", "Idle" },
            ["Preparing"] = new List<string> { "Ready", "Error", "Maintenance" },
            ["Error"] = new List<string> { "Ready", "Maintenance" },
            ["Maintenance"] = new List<string> { "Ready", "Idle" }
        };

        if (!validTransitions.ContainsKey(_currentState) ||
            !validTransitions[_currentState].Contains(stateName))
        {
            return Result.Failure($"Invalid transition from {_currentState} to {stateName}");
        }

        _currentState = stateName;
        await _loggingService.LogInformationAsync($"State transition: {_currentState} -> {stateName}");

        return Result.Success();
    }

    public Task<Result<string>> GetCurrentStateAsync()
    {
        return Task.FromResult(Result.Success(_currentState));
    }

    public Task<Result<bool>> CanHandleEventAsync(MachineEvent machineEvent)
    {
        var canHandle = _currentState switch
        {
            "Ready" or "Idle" => machineEvent.EventType == "StartPreparation",
            "Preparing" => new[] { "PreparationProgress", "PreparationComplete", "PreparationFailed", "CancelPreparation" }
                .Contains(machineEvent.EventType),
            "Error" => machineEvent.EventType == "Reset",
            "Maintenance" => machineEvent.EventType == "Reset",
            _ => false
        };

        return Task.FromResult(Result.Success(canHandle));
    }

    public Task<Result<bool>> IsInStateAsync(string stateName)
    {
        return Task.FromResult(Result.Success(_currentState == stateName));
    }

    public Task<Result<List<string>>> GetAvailableStatesAsync()
    {
        var states = new List<string> { "Idle", "Ready", "Preparing", "Error", "Maintenance", "Stopped" };
        return Task.FromResult(Result.Success(states));
    }

    public Task<Result<BeveragePreparationContext>> GetPreparationContextAsync(Guid orderId)
    {
        if (_preparationContexts.TryGetValue(orderId, out var context))
        {
            return Task.FromResult(Result.Success(context));
        }
        return Task.FromResult(Result.Failure<BeveragePreparationContext>($"No context found for order {orderId}"));
    }
}