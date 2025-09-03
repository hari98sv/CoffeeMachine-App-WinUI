using CoffeeMachine.Core.Application.Mediator.Interfaces;

namespace CoffeeMachine.Core.Application.Mediator;

// Base command with handler registration
public abstract class Command : ICommand
{
    public abstract Type HandlerType
    {
        get;
    }
}

public abstract class Command<TResponse> : ICommand<TResponse>
{
    public abstract Type HandlerType
    {
        get;
    }
}
