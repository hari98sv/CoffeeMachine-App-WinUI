namespace CoffeeMachine.Core.Domain.ValueObjects;

public class Recipe : IEquatable<Recipe>
{
    public Guid Id
    {
        get; private set;
    }
    public string Name
    {
        get; private set;
    }
    public List<Ingredient> Ingredients
    {
        get; private set;
    }
    public string Instructions
    {
        get; private set;
    }
    public TimeSpan EstimatedPreparationTime
    {
        get; private set;
    }

    public Recipe(Guid id, string name, List<Ingredient> ingredients, string instructions, TimeSpan estimatedPreparationTime)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Ingredients = ingredients ?? throw new ArgumentNullException(nameof(ingredients));
        Instructions = instructions;
        EstimatedPreparationTime = estimatedPreparationTime;
    }

    public decimal CalculateCost()
    {
        return Ingredients.Sum(ingredient => ingredient.UnitPrice * ingredient.Quantity);
    }

    public bool HasIngredient(string ingredientName)
    {
        return Ingredients.Any(i => i.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase));
    }

    public Ingredient GetIngredient(string ingredientName)
    {
        return Ingredients.FirstOrDefault(i => i.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase));
    }

    public bool Equals(Recipe other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Recipe);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public static bool operator ==(Recipe left, Recipe right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Recipe left, Recipe right)
    {
        return !Equals(left, right);
    }
}