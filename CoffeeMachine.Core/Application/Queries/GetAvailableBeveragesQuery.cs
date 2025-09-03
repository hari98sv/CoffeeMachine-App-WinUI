using CoffeeMachine.Core.Application.Mediator;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Results;

namespace CoffeeMachine.Core.Application.Queries;

public class GetAvailableBeveragesQuery : Query<Result<BeverageDto>>
{
    public override Type HandlerType => typeof(GetAvailableBeveragesQueryHandler);
}
