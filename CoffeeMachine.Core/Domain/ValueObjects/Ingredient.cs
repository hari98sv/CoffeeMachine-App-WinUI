namespace CoffeeMachine.Core.Domain.ValueObjects;

public class Ingredient : IEquatable<Ingredient>
{
    public string Name
    {
        get; private set;
    }
    public decimal Quantity
    {
        get; private set;
    }
    public string Unit
    {
        get; private set;
    }
    public decimal UnitPrice
    {
        get; private set;
    }
    public bool IsOptional
    {
        get; private set;
    }

    public Ingredient(string name, decimal quantity, string unit, decimal unitPrice, bool isOptional = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Quantity = quantity;
        Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        UnitPrice = unitPrice;
        IsOptional = isOptional;
    }

    public void UpdateQuantity(decimal newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative");
        Quantity = newQuantity;
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative");
        UnitPrice = newUnitPrice;
    }

    public bool Equals(Ingredient other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name &&
               Quantity == other.Quantity &&
               Unit == other.Unit;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Ingredient);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Quantity, Unit);
    }

    public static bool operator ==(Ingredient left, Ingredient right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Ingredient left, Ingredient right)
    {
        return !Equals(left, right);
    }
}