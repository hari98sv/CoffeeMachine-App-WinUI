using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Entities;
using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.ValueObjects;
using CoffeeMachine.Infrastructure.Abstractions;

namespace CoffeeMachine.Core.Application.Services;

public class BeverageService : IBeverageService
{
    private readonly ILoggingService _loggingService;
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IStateMachineService _stateMachineService;

    // Mock data for demonstration - replace with actual database calls
    private readonly List<Beverage> _availableBeverages = new()
    {
        new Coffee(
            id: Guid.NewGuid(),
            name: "Black Coffee",
            description: "Rich black coffee",
            recipe: new Recipe(
                id: Guid.NewGuid(),
                name: "Black Coffee Recipe",
                ingredients: new List<Ingredient>
                {
                    new("Coffee Beans", 15, "g", 0.25m),
                    new("Water", 250, "ml", 0.05m)
                },
                instructions: "Brew coffee with hot water",
                TimeSpan.FromMinutes(2)
            ),
            price: 2.50m,
            preparationTime: TimeSpan.FromMinutes(3)
        ),
        new Latte(
            id: Guid.NewGuid(),
            name: "Latte",
            description: "Creamy latte with steamed milk",
            recipe: new Recipe(
                id: Guid.NewGuid(),
                name: "Latte Recipe",
                ingredients: new List<Ingredient>
                {
                    new("Coffee Beans", 18, "g", 0.30m),
                    new("Milk", 200, "ml", 0.20m),
                    new("Water", 50, "ml", 0.01m)
                },
                instructions: "Brew espresso and steam milk",
                TimeSpan.FromMinutes(3)
            ),
            price: 3.50m,
            preparationTime: TimeSpan.FromMinutes(4),
            milkFoamThickness: 0.3m
        ),
        new Espresso(
            id: Guid.NewGuid(),
            name: "Espresso",
            description: "Strong espresso shot",
            recipe: new Recipe(
                id: Guid.NewGuid(),
                name: "Espresso Recipe",
                ingredients: new List<Ingredient>
                {
                    new("Coffee Beans", 20, "g", 0.35m),
                    new("Water", 30, "ml", 0.01m)
                },
                instructions: "Brew under high pressure",
                TimeSpan.FromMinutes(1)
            ),
            price: 2.00m,
            preparationTime: TimeSpan.FromMinutes(2),
            intensityLevel: 8
        ),
        new Cappuccino(
            id: Guid.NewGuid(),
            name: "Cappuccino",
            description: "Classic cappuccino with foam",
            recipe: new Recipe(
                id: Guid.NewGuid(),
                name: "Cappuccino Recipe",
                ingredients: new List<Ingredient>
                {
                    new("Coffee Beans", 18, "g", 0.30m),
                    new("Milk", 150, "ml", 0.15m),
                    new("Milk Foam", 50, "ml", 0.05m)
                },
                instructions: "Brew espresso and create milk foam",
                TimeSpan.FromMinutes(4)
            ),
            price: 3.75m,
            preparationTime: TimeSpan.FromMinutes(5),
            foamToMilkRatio: 0.4m
        )
    };

    public BeverageService(
        ILoggingService loggingService,
        IOrderService orderService,
        IInventoryService inventoryService
        //IStateMachineService stateMachineService
        )
    {
        _loggingService = loggingService;
        _orderService = orderService;
        _inventoryService = inventoryService;
        //_stateMachineService = stateMachineService;
    }

    public async Task<Result<BeverageDto>> GetBeverageAsync(Guid beverageId)
    {
        try
        {
            var beverage = _availableBeverages.FirstOrDefault(b => b.Id == beverageId);
            if (beverage == null)
            {
                await _loggingService.LogWarningAsync($"Beverage not found: {beverageId}");
                return Result.Failure<BeverageDto>("Beverage not found");
            }

            return Result.Success(MapToDto(beverage));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting beverage: {ex.Message}", ex);
            return Result.Failure<BeverageDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BeverageDto>>> GetAllAvailableBeveragesAsync()
    {
        try
        {
            var availableBeverages = _availableBeverages
                .Where(b => b.IsAvailable)
                .Select(MapToDto)
                .ToList();

            await _loggingService.LogInformationAsync($"Retrieved {availableBeverages.Count} available beverages");
            return Result.Success(availableBeverages);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting available beverages: {ex.Message}", ex);
            return Result.Failure<List<BeverageDto>>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(BeverageType beverageType, string size, string customerName,
                                                       bool addSugar = false, bool addMilk = false, string specialInstructions = null)
    {
        try
        {
            var beverage = _availableBeverages.FirstOrDefault(b => b.Type == beverageType && b.IsAvailable);
            if (beverage == null)
            {
                await _loggingService.LogWarningAsync($"Beverage type not available: {beverageType}");
                return Result.Failure<OrderDto>("Beverage not available");
            }

            // Calculate price based on size
            var priceMultiplier = GetSizeMultiplier(size);
            var totalPrice = beverage.Price * priceMultiplier;

            var orderDto = new OrderDto
                (
                Id: Guid.NewGuid(),
                BeverageType: beverageType,
                BeverageName: beverage.Name,
                Size: size,
                CustomerName: customerName,
                TotalPrice: totalPrice,
                Status: "Created",
                ProgressPercentage: 0,
                OrderTime: DateTime.UtcNow,
                CompletionTime: DateTime.UtcNow,
                AddSugar: addSugar,
                AddMilk: addMilk,
                SpecialInstructions: specialInstructions,
                ErrorMessage: "");

            await _loggingService.LogInformationAsync($"Created order for {beverage.Name}: {orderDto.Id}");
            return Result.Success(orderDto);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error creating order: {ex.Message}", ex);
            return Result.Failure<OrderDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result> StartBeveragePreparationAsync(Guid orderId)
    {
        try
        {
            // In a real implementation, this would trigger the state machine
            // and workflow for beverage preparation
            await _loggingService.LogInformationAsync($"Starting preparation for order: {orderId}");

            // Simulate preparation start
            await Task.Delay(100);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error starting preparation: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> CancelBeveragePreparationAsync(Guid orderId)
    {
        try
        {
            await _loggingService.LogInformationAsync($"Cancelling preparation for order: {orderId}");

            // Simulate cancellation
            await Task.Delay(100);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error cancelling preparation: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderStatusDto>> GetOrderStatusAsync(Guid orderId)
    {
        try
        {
            // This would normally come from a database or order service
            var statusDto = new OrderStatusDto(
                OrderId: orderId,
                Status: "InProgress",
                ProgressPercentage: 50,
                CurrentStep: "Brewing",
                EstimatedCompletionTime: DateTime.UtcNow.AddMinutes(2),
                ErrorMessage: string.Empty
            );

            return Result.Success(statusDto);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting order status: {ex.Message}", ex);
            return Result.Failure<OrderStatusDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdateBeverageAvailabilityAsync(Guid beverageId, bool isAvailable)
    {
        try
        {
            var beverage = _availableBeverages.FirstOrDefault(b => b.Id == beverageId);
            if (beverage == null)
            {
                return Result.Failure("Beverage not found");
            }

            beverage.UpdateAvailability(isAvailable);
            await _loggingService.LogInformationAsync($"Updated beverage {beverageId} availability to: {isAvailable}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error updating availability: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BeverageDto>>> GetBeveragesByTypeAsync(BeverageType type)
    {
        try
        {
            var beverages = _availableBeverages
                .Where(b => b.Type == type && b.IsAvailable)
                .Select(MapToDto)
                .ToList();

            return Result.Success(beverages);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting beverages by type: {ex.Message}", ex);
            return Result.Failure<List<BeverageDto>>($"Error: {ex.Message}");
        }
    }

    private BeverageDto MapToDto(Beverage beverage)
    {
        return new BeverageDto
        (
            Id: beverage.Id,
            Name: beverage.Name,
            Description: beverage.Description,
            Type: beverage.Type,
            BasePrice: beverage.Price,
            PreparationTime: beverage.PreparationTime,
            IsAvailable: beverage.IsAvailable,
            AvailableSizes: new List<string> { "Small", "Medium", "Large" },
            CustomizationOptions: new List<string> { "Extra Sugar", "Extra Milk", "No Foam" }
            );
    }

    private decimal GetSizeMultiplier(string size)
    {
        return size.ToLower() switch
        {
            "small" => 0.8m,
            "medium" => 1.0m,
            "large" => 1.2m,
            _ => 1.0m
        };
    }
}