using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Entities;
using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Application.Services;

public interface IBeverageService
{
    Task<Result<BeverageDto>> GetBeverageAsync(Guid beverageId);
    Task<Result<List<BeverageDto>>> GetAllAvailableBeveragesAsync();
    Task<Result<OrderDto>> CreateOrderAsync(BeverageType beverageType, string size, string customerName,
                                          bool addSugar = false, bool addMilk = false, string specialInstructions = null);
    Task<Result> StartBeveragePreparationAsync(OrderDto data);
    Task<Result> CancelBeveragePreparationAsync(Guid orderId);
    Task<Result<OrderStatusDto>> GetOrderStatusAsync(Guid orderId);
    Task<Result> UpdateBeverageAvailabilityAsync(Guid beverageId, bool isAvailable);
    Task<Result> SaveBeveragesAsync(List<Beverage> beverages);
}
