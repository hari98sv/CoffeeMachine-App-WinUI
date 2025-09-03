using CoffeeMachine.Core.Application.Commands.Handlers;
using CoffeeMachine.Core.Application.Mediator;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Commands;

// CancelBeverageCommand with explicit handler
public class CancelBeverageCommand : Command<Result>
{
    public Guid OrderId
    {
        get;
    }

    public CancelBeverageCommand(Guid orderId)
    {
        OrderId = orderId;
    }

    public override Type HandlerType => typeof(CancelBeverageCommandHandler);
}
