using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Common;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Abstractions.Logging;

namespace CoffeeMachine.Core.Application.Commands.Handlers;

// Handler with explicit command type
public class CreateBeverageCommandHandler : ICommandHandler<CreateBeverageCommand, Result<OrderDto>>
{
    private readonly IBeverageService _beverageService;
    private readonly IInventoryService _inventoryService;
    private readonly ILoggingService _loggingService;
    private readonly ITelemetryService _telemetryService;
    private readonly IMessagingService _messagingService;

    public CreateBeverageCommandHandler(
        IBeverageService beverageService,
        IInventoryService inventoryService,
        ILoggingService loggingService,
        //ITelemetryService telemetryService,
        IMessagingService messagingService)
    {
        _beverageService = beverageService;
        _inventoryService = inventoryService;
        _loggingService = loggingService;
        //_telemetryService = telemetryService;
        _messagingService = messagingService;
    }

    // Direct implementation - easy to debug and navigate
    public async Task<Result<OrderDto>> Handle(CreateBeverageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _loggingService.LogInformationAsync($"Starting beverage preparation: {command.BeverageType}");

            // Check inventory
            var inventoryCheck = await _inventoryService.CheckInventoryForBeverageAsync(command.BeverageType);
            if (!inventoryCheck.IsSuccess)
            {
                return Result.Failure<OrderDto>("Insufficient inventory");
            }

            // Create order
            var orderResult = await _beverageService.CreateOrderAsync(
                command.BeverageType, command.Size, command.CustomerName,
                command.AddSugar, command.AddMilk, command.SpecialInstructions);

            if (!orderResult.IsSuccess)
            {
                return Result.Failure<OrderDto>(orderResult.Error);
            }

            // Start preparation
            var preparationResult = await _beverageService.StartBeveragePreparationAsync(orderResult.Data.Id);
            if (!preparationResult.IsSuccess)
            {
                return Result.Failure<OrderDto>(preparationResult.Error);
            }

            // Notify UI
            await _messagingService.Publish(new BeverageStatusMessage(
                orderResult.Data, "Preparation started", 0
            ));

            return Result.Success(orderResult.Data);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error creating beverage: {ex.Message}", ex);
            return Result.Failure<OrderDto>($"Error: {ex.Message}");
        }
    }
}
