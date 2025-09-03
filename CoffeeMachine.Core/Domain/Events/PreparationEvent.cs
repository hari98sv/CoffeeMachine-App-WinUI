namespace CoffeeMachine.Core.Domain.Events;

public class PreparationEvent : OrderEvent
{
    public int Progress
    {
        get; set;
    }
    public string CurrentStep
    {
        get; set;
    }

    public PreparationEvent(Guid orderId, string beverageType, string eventType,
                          int progress, string currentStep, object eventData = null)
        : base(orderId, beverageType, eventType, eventData)
    {
        Progress = progress;
        CurrentStep = currentStep;
    }
}