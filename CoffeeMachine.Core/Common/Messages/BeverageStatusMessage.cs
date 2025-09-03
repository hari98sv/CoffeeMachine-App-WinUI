using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Common;

public class BeverageStatusMessage
{
    public Guid OrderId
    {
        get;
    }
    public OrderDto Order
    {
        get;
    }
    public string Status
    {
        get;
    }
    public int ProgressPercentage
    {
        get;
    }
    public string CurrentStep
    {
        get;
    }
    public BeverageType BeverageType
    {
        get;
    }
    public string BeverageName
    {
        get;
    }
    public DateTime Timestamp
    {
        get;
    }
    public bool IsComplete => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    public bool IsError => Status.Equals("Error", StringComparison.OrdinalIgnoreCase);
    public bool IsCancelled => Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

    public BeverageStatusMessage(OrderDto order, string status, int progressPercentage, string currentStep = null)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
        OrderId = order.Id;
        Status = status ?? throw new ArgumentNullException(nameof(status));
        ProgressPercentage = progressPercentage;
        CurrentStep = currentStep ?? DetermineCurrentStep(status, progressPercentage);
        BeverageType = order.BeverageType;
        BeverageName = order.BeverageName;
        Timestamp = DateTime.UtcNow;
    }

    public BeverageStatusMessage(Guid orderId, BeverageType beverageType, string beverageName,
                               string status, int progressPercentage, string currentStep = null)
    {
        OrderId = orderId;
        BeverageType = beverageType;
        BeverageName = beverageName;
        Status = status ?? throw new ArgumentNullException(nameof(status));
        ProgressPercentage = progressPercentage;
        CurrentStep = currentStep ?? DetermineCurrentStep(status, progressPercentage);
        Timestamp = DateTime.UtcNow;
    }

    private string DetermineCurrentStep(string status, int progress)
    {
        return status switch
        {
            "Queued" => "Waiting to start",
            "Preparation started" => "Initializing machine",
            "Heating" => "Heating components",
            "Grinding" => "Grinding coffee beans",
            "Brewing" when progress < 30 => "Preparing brew",
            "Brewing" when progress < 70 => "Extracting coffee",
            "Brewing" => "Finalizing extraction",
            "Steaming" when progress < 50 => "Heating milk",
            "Steaming" => "Creating foam",
            "Mixing" => "Combining ingredients",
            "Dispensing" => "Dispensing beverage",
            "Completed" => "Ready for serving",
            "Cancelled" => "Process cancelled",
            "Error" => "Error occurred",
            _ => status
        };
    }

    public override string ToString()
    {
        return $"{BeverageName} - {Status} ({ProgressPercentage}%) - {CurrentStep}";
    }
}