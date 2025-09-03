namespace CoffeeMachine.Core.Domain.Interfaces;

public interface IEntity<TId>
{
    TId Id
    {
        get;
    }
}