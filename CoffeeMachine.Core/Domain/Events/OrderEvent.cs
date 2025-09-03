namespace CoffeeMachine.Core.Domain.Events;

public class OrderEvent : MachineEvent
{
    public Guid OrderId
    {
        get; set;
    }
    public string BeverageType
    {
        get; set;
    }

    public OrderEvent(Guid orderId, string beverageType, string eventType, object eventData = null)
        : base(eventType, eventData)
    {
        OrderId = orderId;
        BeverageType = beverageType;
    }
}
