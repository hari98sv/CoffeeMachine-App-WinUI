namespace CoffeeMachine.Core.Common.Messages;

public class MessagingService : IMessagingService
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();
    private readonly object _lock = new object();

    public Task Publish<TMessage>(TMessage message) where TMessage : class
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        Type messageType = typeof(TMessage);
        List<Delegate> handlers;

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(messageType, out handlers) || handlers.Count == 0)
                return Task.CompletedTask;
        }

        // Invoke handlers on the current synchronization context
        foreach (var handler in handlers.ToArray()) // ToArray to avoid modification during iteration
        {
            if (handler is Action<TMessage> action)
            {
                // Use Task.Run to avoid blocking the publisher
                Task.Run(() => action(message));
            }
        }

        return Task.CompletedTask;
    }

    public IDisposable Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        Type messageType = typeof(TMessage);

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(messageType, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscribers[messageType] = handlers;
            }
            handlers.Add(handler);
        }

        return new Subscription<TMessage>(this, handler);
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        Type messageType = typeof(TMessage);

        lock (_lock)
        {
            if (_subscribers.TryGetValue(messageType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _subscribers.Remove(messageType);
                }
            }
        }
    }

    private class Subscription<TMessage> : IDisposable where TMessage : class
    {
        private readonly MessagingService _messagingService;
        private readonly Action<TMessage> _handler;
        private bool _disposed;

        public Subscription(MessagingService messagingService, Action<TMessage> handler)
        {
            _messagingService = messagingService;
            _handler = handler;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _messagingService.Unsubscribe(_handler);
                _disposed = true;
            }
        }
    }

    // For compatibility with the original interface
    public IObservable<TMessage> Subscribe<TMessage>() where TMessage : class
    {
        throw new NotSupportedException("Use Subscribe<TMessage>(Action<TMessage>) instead");
    }

    public void Unsubscribe<TMessage>(IObservable<TMessage> subscription)
    {
        throw new NotSupportedException("Use IDisposable from Subscribe<TMessage>(Action<TMessage>) instead");
    }
}