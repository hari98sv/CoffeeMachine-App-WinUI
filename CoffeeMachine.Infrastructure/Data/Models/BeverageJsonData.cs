using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoffeeMachine.Infrastructure.Data.Models;

public class BeverageJsonData
{
    public List<BeverageJsonModel> Beverages { get; set; } = new List<BeverageJsonModel>();
}

public class BeverageJsonModel
{
    public string Type { get; set; } = string.Empty;
    public Guid Id
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price
    {
        get; set;
    }
    public TimeSpan PreparationTime
    {
        get; set;
    }

    // Beverage-specific properties
    public decimal? MilkFoamThickness
    {
        get; set;
    }
    public int? IntensityLevel
    {
        get; set;
    }
    public decimal? FoamToMilkRatio
    {
        get; set;
    }

    public RecipeJsonModel Recipe { get; set; } = new RecipeJsonModel();
}

public class RecipeJsonModel
{
    public Guid Id
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public TimeSpan EstimatedPreparationTime
    {
        get; set;
    }
    public List<IngredientJsonModel> Ingredients { get; set; } = new List<IngredientJsonModel>();
}

public class IngredientJsonModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity
    {
        get; set;
    }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice
    {
        get; set;
    }
    public bool IsOptional { get; set; } = false;
}