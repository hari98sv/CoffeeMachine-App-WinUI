using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Application.Services;

public interface IInventoryService
{
    Task<Result<List<InventoryItemDto>>> GetInventoryAsync();
    Task<Result> UpdateInventoryAsync(Guid itemId, decimal quantity);
    Task<Result<bool>> CheckInventoryForBeverageAsync(BeverageType beverageType);
}
