using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Abstractions.Logging;

namespace CoffeeMachine.Core.Application.Commands.Handlers;

public class CancelBeverageCommandHandler : ICommandHandler<CancelBeverageCommand, Result>
{
    private readonly IOrderService _orderService;
    private readonly IStateMachineService _stateMachineService;
    private readonly ILoggingService _loggingService;
    private readonly IMessagingService _messagingService;

    public CancelBeverageCommandHandler(
        IOrderService orderService,
        //IStateMachineService stateMachineService,
        ILoggingService loggingService,
        IMessagingService messagingService)
    {
        _orderService = orderService;
        //_stateMachineService = stateMachineService;
        _loggingService = loggingService;
        _messagingService = messagingService;
    }

    public async Task<Result> Handle(CancelBeverageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _loggingService.LogInformationAsync($"Cancelling order: {command.OrderId}");

            // Implementation here
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error cancelling order: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }
}