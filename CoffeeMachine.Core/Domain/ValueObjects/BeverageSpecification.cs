using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Domain.ValueObjects;

public class BeverageSpecification : IEquatable<BeverageSpecification>
{
    public BeverageType Type
    {
        get; private set;
    }
    public Dictionary<string, decimal> SizeOptions
    {
        get; private set;
    } // SizeName -> PriceMultiplier
    public List<string> CustomizationOptions
    {
        get; private set;
    }
    public int MaxSugarLevel
    {
        get; private set;
    }
    public int MaxMilkLevel
    {
        get; private set;
    }
    public bool SupportsHotAndCold
    {
        get; private set;
    }

    public BeverageSpecification(BeverageType type, Dictionary<string, decimal> sizeOptions,
                               List<string> customizationOptions, int maxSugarLevel = 3,
                               int maxMilkLevel = 3, bool supportsHotAndCold = true)
    {
        Type = type;
        SizeOptions = sizeOptions ?? throw new ArgumentNullException(nameof(sizeOptions));
        CustomizationOptions = customizationOptions ?? new List<string>();
        MaxSugarLevel = maxSugarLevel;
        MaxMilkLevel = maxMilkLevel;
        SupportsHotAndCold = supportsHotAndCold;
    }

    public bool IsValidSize(string size)
    {
        return SizeOptions.ContainsKey(size);
    }

    public decimal GetPriceMultiplier(string size)
    {
        return SizeOptions.TryGetValue(size, out var multiplier) ? multiplier : 1.0m;
    }

    public bool SupportsCustomization(string customization)
    {
        return CustomizationOptions.Contains(customization, StringComparer.OrdinalIgnoreCase);
    }

    public void AddCustomizationOption(string option)
    {
        if (!CustomizationOptions.Contains(option, StringComparer.OrdinalIgnoreCase))
        {
            CustomizationOptions.Add(option);
        }
    }

    public void RemoveCustomizationOption(string option)
    {
        CustomizationOptions.RemoveAll(x => x.Equals(option, StringComparison.OrdinalIgnoreCase));
    }

    public bool Equals(BeverageSpecification other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as BeverageSpecification);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type);
    }

    public static bool operator ==(BeverageSpecification left, BeverageSpecification right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BeverageSpecification left, BeverageSpecification right)
    {
        return !Equals(left, right);
    }
}