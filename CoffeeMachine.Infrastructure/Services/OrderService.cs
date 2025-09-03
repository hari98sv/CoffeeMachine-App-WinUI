using CoffeeMachine.Core.Abstractions.Logging;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ILoggingService _loggingService;
    private readonly List<OrderDto> _orders = new();

    public OrderService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<Result<OrderDto>> GetOrderAsync(Guid orderId)
    {
        try
        {
            var order = _orders.Find(o => o.Id == orderId);
            if (order == null)
            {
                await _loggingService.LogWarningAsync($"Order not found: {orderId}");
                return Result.Failure<OrderDto>("Order not found");
            }

            return Result.Success(order);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting order: {ex.Message}", ex);
            return Result.Failure<OrderDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderDto>>> GetAllOrdersAsync()
    {
        try
        {
            return Result.Success(new List<OrderDto>(_orders));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting all orders: {ex.Message}", ex);
            return Result.Failure<List<OrderDto>>($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdateOrderStatusAsync(Guid orderId, string status, int progressPercentage = 0, string errorMessage = null)
    {
        try
        {
            var order = _orders.Find(o => o.Id == orderId);
            if (order == null)
            {
                return Result.Failure("Order not found");
            }

            //order.Status = status;
            //order.ProgressPercentage = progressPercentage;
            //order.ErrorMessage = errorMessage;

            //if (status == "Completed")
            //{
            //    order.CompletionTime = DateTime.UtcNow;
            //}

            await _loggingService.LogInformationAsync($"Updated order {orderId} status to: {status}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error updating order status: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> CompleteOrderAsync(Guid orderId)
    {
        return await UpdateOrderStatusAsync(orderId, "Completed", 100);
    }
}