using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Enums;

namespace CoffeeMachine.Core.Application.Queries;

public class GetAvailableBeveragesQueryHandler : IQueryHandler<GetAvailableBeveragesQuery, Result<BeverageDto>>
{
    public async Task<Result<BeverageDto>> Handle(GetAvailableBeveragesQuery query, CancellationToken cancellationToken)
    {
        var sampleBeverage = new BeverageDto(Id: Guid.NewGuid(),
            Name: "Caramel Latte",
            Description: "Rich espresso blended with steamed milk and caramel syrup.",
            Type: BeverageType.Coffee,
            BasePrice: 3.75m,
            PreparationTime: TimeSpan.FromMinutes(4),
            IsAvailable: true,
            AvailableSizes: new List<string> { "Small", "Medium", "Large" },
            CustomizationOptions: new List<string> { "Extra Shot", "Oat Milk", "No Foam", "Whipped Cream" });

        return Result.Success(sampleBeverage);

    }
}
