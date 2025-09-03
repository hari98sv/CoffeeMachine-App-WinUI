using CoffeeMachine.Core.Application.Mediator.Interfaces;

namespace CoffeeMachine.Core.Application.Mediator;

// Base query with handler registration
public abstract class Query<TResponse> : IQuery<TResponse>
{
    public abstract Type HandlerType
    {
        get;
    }
}
