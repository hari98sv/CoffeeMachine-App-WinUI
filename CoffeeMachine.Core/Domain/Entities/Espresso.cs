using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.ValueObjects;

namespace CoffeeMachine.Core.Domain.Entities;

public class Espresso : Beverage
{
    public int IntensityLevel
    {
        get; private set;
    }

    public Espresso(Guid id, string name, string description, Recipe recipe,
                   decimal price, TimeSpan preparationTime, int intensityLevel)
        : base(id, name, description, BeverageType.Espresso, recipe, price, preparationTime)
    {
        IntensityLevel = intensityLevel;
    }

    public void UpdateIntensityLevel(int level)
    {
        if (level < 1 || level > 10)
            throw new ArgumentException("Intensity level must be between 1 and 10");
        IntensityLevel = level;
    }

    public override void ValidatePreparation()
    {
        if (Recipe.Ingredients.Find(i => i.Name.Equals("Coffee", StringComparison.OrdinalIgnoreCase))?.Quantity < 7)
            throw new InvalidOperationException("Espresso requires at least 7g of coffee");
    }
}

