using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.Interfaces;
using CoffeeMachine.Core.Domain.ValueObjects;

namespace CoffeeMachine.Core.Domain.Entities;

public abstract class Beverage : IEntity<Guid>
{
    public Guid Id
    {
        get; protected set;
    }
    public string Name
    {
        get; protected set;
    }
    public string Description
    {
        get; protected set;
    }
    public BeverageType Type
    {
        get; protected set;
    }
    public Recipe Recipe
    {
        get; protected set;
    }
    public decimal Price
    {
        get; protected set;
    }
    public TimeSpan PreparationTime
    {
        get; protected set;
    }
    public bool IsAvailable
    {
        get; protected set;
    }

    protected Beverage(Guid id, string name, string description, BeverageType type,
                      Recipe recipe, decimal price, TimeSpan preparationTime)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));
        Price = price;
        PreparationTime = preparationTime;
        IsAvailable = true;
    }

    public virtual void UpdateAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public virtual void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");
        Price = newPrice;
    }

    public abstract void ValidatePreparation();

    public override bool Equals(object obj)
    {
        return obj is Beverage beverage &&
               Id.Equals(beverage.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}

