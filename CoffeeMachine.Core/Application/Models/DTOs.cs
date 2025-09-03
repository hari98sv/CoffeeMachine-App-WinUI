using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Application.Models;

public record BeverageDto(Guid Id
, string Name
, string Description
, BeverageType Type
, decimal BasePrice
, TimeSpan PreparationTime
, bool IsAvailable
, List<string> AvailableSizes
, List<string> CustomizationOptions
);

public record InventoryItemDto(Guid Id
, string Name
, decimal CurrentStock
, decimal MinimumStockLevel
, string Unit
, decimal UnitPrice
, DateTime LastRestocked
, bool IsLowStock
);

public record OrderDto(Guid Id
, BeverageType BeverageType
, string BeverageName
, string Size
, string CustomerName
, decimal TotalPrice
, string Status
, int ProgressPercentage
, DateTime OrderTime
, DateTime? CompletionTime
, bool AddSugar
, bool AddMilk
, string SpecialInstructions
, string ErrorMessage
);

public record OrderStatusDto(Guid OrderId
, string Status
, int ProgressPercentage
, string CurrentStep
, DateTime EstimatedCompletionTime
, string ErrorMessage
);
