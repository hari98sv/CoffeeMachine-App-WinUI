using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Infrastructure.Abstractions;

namespace CoffeeMachine.Core.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly ILoggingService _loggingService;
    private readonly List<InventoryItemDto> _inventory = new()
    {
        new InventoryItemDto ( Guid.NewGuid(), "Coffee Beans", 1000, 0, "g", 0.02m, DateTime.UtcNow, false ),
        new InventoryItemDto ( Guid.NewGuid(), "Milk", 1000, 0, "ml", 0.01m, DateTime.UtcNow, false ),
        new InventoryItemDto ( Guid.NewGuid(), "Sugar", 1000, 0, "g", 0.005m, DateTime.UtcNow, false ),
        new InventoryItemDto ( Guid.NewGuid(), "Water", 1000, 0, "ml", 0.001m, DateTime.UtcNow, false )
    };

    public InventoryService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<Result<List<InventoryItemDto>>> GetInventoryAsync()
    {
        try
        {
            return Result.Success(new List<InventoryItemDto>(_inventory));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting inventory: {ex.Message}", ex);
            return Result.Failure<List<InventoryItemDto>>($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdateInventoryAsync(Guid itemId, decimal quantity)
    {
        try
        {
            var item = _inventory.Find(i => i.Id == itemId);
            if (item == null)
            {
                return Result.Failure("Inventory item not found");
            }

            //item.CurrentStock += quantity;
            await _loggingService.LogInformationAsync($"Updated inventory {item.Name}: {quantity} {item.Unit}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error updating inventory: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CheckInventoryForBeverageAsync(BeverageType beverageType)
    {
        try
        {
            // Simple check - in real implementation, check actual recipe requirements
            var hasInventory = _inventory.TrueForAll(item => item.CurrentStock > 0);
            return Result.Success(hasInventory);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error checking inventory: {ex.Message}", ex);
            return Result.Failure<bool>($"Error: {ex.Message}");
        }
    }
}