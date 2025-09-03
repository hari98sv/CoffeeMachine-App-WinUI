using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.ValueObjects;

namespace CoffeeMachine.Core.Domain.Entities;

public class Coffee : Beverage
{
    public Coffee(Guid id, string name, string description, Recipe recipe,
                 decimal price, TimeSpan preparationTime)
        : base(id, name, description, BeverageType.Coffee, recipe, price, preparationTime)
    {
    }

    public override void ValidatePreparation()
    {
        if (Recipe.Ingredients.Count == 0)
            throw new InvalidOperationException("Coffee recipe must have ingredients");
    }
}

