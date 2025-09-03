using System.Collections.Concurrent;
using CoffeeMachine.Core.Application.Mediator.Interfaces;

namespace CoffeeMachine.Core.Application.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _handlerCache = new ConcurrentDictionary<Type, object>();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command is Command baseCommand)
        {
            var handler = GetHandler<ICommandHandler<TCommand>>(baseCommand.HandlerType);
            return handler.Handle(command, cancellationToken);
        }

        throw new InvalidOperationException($"Command must inherit from Command base class: {typeof(TCommand).Name}");
    }

    public Task<TResponse> Send<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        if (command is Command<TResponse> baseCommand)
        {
            var handler = GetHandler<ICommandHandler<TCommand, TResponse>>(baseCommand.HandlerType);
            return handler.Handle(command, cancellationToken);
        }

        throw new InvalidOperationException($"Command must inherit from Command<TResponse> base class: {typeof(TCommand).Name}");
    }

    public Task<TResponse> Query<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        if (query is Query<TResponse> baseQuery)
        {
            var handler = GetHandler<IQueryHandler<TQuery, TResponse>>(baseQuery.HandlerType);
            return handler.Handle(query, cancellationToken);
        }

        throw new InvalidOperationException($"Query must inherit from Query<TResponse> base class: {typeof(TQuery).Name}");
    }

    private THandler GetHandler<THandler>(Type handlerType)
    {
        return (THandler)_handlerCache.GetOrAdd(handlerType, type =>
        {
            var handler = _serviceProvider.GetService(type);
            if (handler == null)
            {
                throw new InvalidOperationException($"Handler not registered for type: {type.Name}");
            }

            if (handler is not THandler typedHandler)
            {
                throw new InvalidOperationException($"Handler {type.Name} does not implement {typeof(THandler).Name}");
            }

            return typedHandler;
        });
    }
}