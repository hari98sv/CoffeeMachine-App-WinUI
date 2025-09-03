using CoffeeMachine.Core.Application.Commands.Handlers;
using CoffeeMachine.Core.Application.Mediator;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Application.Commands;

// CreateBeverageCommand with explicit handler relationship
public class CreateBeverageCommand : Command<Result<OrderDto>>
{
    public BeverageType BeverageType
    {
        get;
    }
    public string Size
    {
        get;
    }
    public string CustomerName
    {
        get;
    }
    public bool AddSugar
    {
        get;
    }
    public bool AddMilk
    {
        get;
    }
    public string SpecialInstructions
    {
        get;
    }

    public CreateBeverageCommand(BeverageType beverageType, string size, string customerName,
                               bool addSugar = false, bool addMilk = false, string specialInstructions = null)
    {
        BeverageType = beverageType;
        Size = size;
        CustomerName = customerName;
        AddSugar = addSugar;
        AddMilk = addMilk;
        SpecialInstructions = specialInstructions;
    }

    // Explicit handler type - no reflection needed!
    public override Type HandlerType => typeof(CreateBeverageCommandHandler);
}
