namespace CoffeeMachine.Core.Domain.Events;

public class MachineEvent
{
    public string EventType
    {
        get; set;
    }
    public object EventData
    {
        get; set;
    }
    public DateTime Timestamp
    {
        get; set;
    }
    public Guid CorrelationId
    {
        get; set;
    }

    public MachineEvent()
    {
        Timestamp = DateTime.UtcNow;
        CorrelationId = Guid.NewGuid();
    }

    public MachineEvent(string eventType, object eventData = null) : this()
    {
        EventType = eventType;
        EventData = eventData;
    }
}