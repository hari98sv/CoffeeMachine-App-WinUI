using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.ValueObjects;

namespace CoffeeMachine.Core.Domain.Entities;

public class Cappuccino : Beverage
{
    public decimal FoamToMilkRatio
    {
        get; private set;
    }

    public Cappuccino(Guid id, string name, string description, Recipe recipe,
                     decimal price, TimeSpan preparationTime, decimal foamToMilkRatio)
        : base(id, name, description, BeverageType.Cappuccino, recipe, price, preparationTime)
    {
        FoamToMilkRatio = foamToMilkRatio;
    }

    public void UpdateFoamToMilkRatio(decimal ratio)
    {
        if (ratio < 0 || ratio > 1)
            throw new ArgumentException("Foam to milk ratio must be between 0 and 1");
        FoamToMilkRatio = ratio;
    }

    public override void ValidatePreparation()
    {
        var milkIngredient = Recipe.Ingredients.Find(i => i.Name.Equals("Milk", StringComparison.OrdinalIgnoreCase));
        var foamIngredient = Recipe.Ingredients.Find(i => i.Name.Equals("Milk Foam", StringComparison.OrdinalIgnoreCase));

        if (milkIngredient == null || foamIngredient == null)
            throw new InvalidOperationException("Cappuccino must contain both milk and milk foam");
    }
}

