namespace CoffeeMachine.Core.Common.Messages;

public interface IMessagingService
{
    Task Publish<TMessage>(TMessage message) where TMessage : class;

    // New method for simpler subscription
    IDisposable Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class;

    void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class;

    // Keep original methods for backward compatibility (mark as obsolete if desired)
    IObservable<TMessage> Subscribe<TMessage>() where TMessage : class;
    void Unsubscribe<TMessage>(IObservable<TMessage> subscription);
}
