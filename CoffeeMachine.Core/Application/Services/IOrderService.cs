using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> GetOrderAsync(Guid orderId);
    Task<Result<List<OrderDto>>> GetAllOrdersAsync();
    Task<Result> UpdateOrderStatusAsync(Guid orderId, string status, int progressPercentage = 0, string errorMessage = null);
    Task<Result> CompleteOrderAsync(Guid orderId);
}
