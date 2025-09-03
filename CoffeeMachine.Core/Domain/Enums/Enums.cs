namespace CoffeeMachine.Core.Domain.Enums;
public enum BeverageType
{
    Coffee,
    Latte,
    Espresso,
    Cappuccino,
    Tea,
    HotChocolate,
    Milk
}

public enum BeverageSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

public enum MachineState
{
    Idle,
    Heating,
    Brewing,
    Steaming,
    Dispensing,
    Cleaning,
    Error,
    Maintenance
}

