using CoffeeMachine.Core.Application.Logging;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Queries;

public class GetBeverageStatusQueryHandler : IQueryHandler<GetBeverageStatusQuery, Result<OrderStatusDto>>
{
    private readonly IOrderService _orderService;
    private readonly ILoggingService _loggingService;

    public GetBeverageStatusQueryHandler(IOrderService orderService, ILoggingService loggingService)
    {
        _orderService = orderService;
        _loggingService = loggingService;
    }

    public async Task<Result<OrderStatusDto>> Handle(GetBeverageStatusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            await _loggingService.LogDebugAsync($"Fetching status for order: {query.OrderId}");

            var orderResult = await _orderService.GetOrderAsync(query.OrderId);
            if (!orderResult.IsSuccess)
            {
                return Result.Failure<OrderStatusDto>("Order not found");
            }

            var order = orderResult.Data;
            var statusDto = new OrderStatusDto(
                OrderId: order.Id,
                Status: order.Status,
                ProgressPercentage: order.ProgressPercentage,
                CurrentStep: DetermineCurrentStep(order.Status),
                EstimatedCompletionTime: DateTime.UtcNow.AddMinutes(1),
                ErrorMessage: "");

            return Result.Success(statusDto);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error fetching status: {ex.Message}", ex);
            return Result.Failure<OrderStatusDto>($"Error: {ex.Message}");
        }
    }

    private string DetermineCurrentStep(string status)
    {
        return status switch
        {
            "Preparation started" => "Initializing",
            "Brewing" => "Brewing coffee",
            "Steaming" => "Steaming milk",
            "Completed" => "Ready",
            _ => status
        };
    }
}