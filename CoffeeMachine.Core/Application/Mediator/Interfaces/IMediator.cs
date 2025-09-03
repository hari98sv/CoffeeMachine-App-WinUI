namespace CoffeeMachine.Core.Application.Mediator.Interfaces;

// Strongly-typed mediator interface
public interface IMediator
{
    Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResponse> Send<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;

    Task<TResponse> Query<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}