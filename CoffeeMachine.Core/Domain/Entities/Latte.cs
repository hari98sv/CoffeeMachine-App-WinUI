using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.ValueObjects;

namespace CoffeeMachine.Core.Domain.Entities;

public class Latte : Beverage
{
    public decimal MilkFoamThickness
    {
        get; private set;
    }

    public Latte(Guid id, string name, string description, Recipe recipe,
                decimal price, TimeSpan preparationTime, decimal milkFoamThickness)
        : base(id, name, description, BeverageType.Latte, recipe, price, preparationTime)
    {
        MilkFoamThickness = milkFoamThickness;
    }

    public void UpdateMilkFoamThickness(decimal thickness)
    {
        if (thickness < 0 || thickness > 1)
            throw new ArgumentException("Milk foam thickness must be between 0 and 1");
        MilkFoamThickness = thickness;
    }

    public override void ValidatePreparation()
    {
        var milkIngredient = Recipe.Ingredients.Find(i => i.Name.Equals("Milk", StringComparison.OrdinalIgnoreCase));
        if (milkIngredient == null || milkIngredient.Quantity <= 0)
            throw new InvalidOperationException("Latte must contain milk");
    }
}

