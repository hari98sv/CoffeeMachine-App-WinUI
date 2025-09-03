using CoffeeMachine.Core.Application.Mediator;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Queries;

// GetBeverageStatusQuery with explicit handler
public class GetBeverageStatusQuery : Query<Result<OrderStatusDto>>
{
    public Guid OrderId
    {
        get;
    }

    public GetBeverageStatusQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public override Type HandlerType => typeof(GetBeverageStatusQueryHandler);
}
