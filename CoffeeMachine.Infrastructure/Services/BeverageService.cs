using System.Text.Json;
using CoffeeMachine.Core.Application.Logging;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Repository;
using CoffeeMachine.Core.Application.Services;
using CoffeeMachine.Core.Common;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Domain.Entities;
using CoffeeMachine.Core.Domain.Enums;
using CoffeeMachine.Core.Domain.Events;
using CoffeeMachine.Core.Domain.ValueObjects;
using CoffeeMachine.Infrastructure.Data.Models;
using CoffeeMachine.Infrastructure.Helpers;

namespace CoffeeMachine.Infrastructure.Services;

public class BeverageService : IBeverageService
{
    private readonly ILoggingService _loggingService;
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IStateMachineService _stateMachineService;
    private readonly IFileService _fileService;
    private readonly IMessagingService _messagingService;
    private const string BeveragesFilePath = "Data/beverages.json";

    // Mock data for demonstration - replace with actual database calls
    private List<Beverage> _availableBeverages = new();

    public BeverageService(
        ILoggingService loggingService,
        IOrderService orderService,
        IInventoryService inventoryService,
        IFileService fileService,
        IStateMachineService stateMachineService,
        IMessagingService messagingService
        )
    {
        _loggingService = loggingService;
        _orderService = orderService;
        _inventoryService = inventoryService;
        _fileService = fileService;
        _stateMachineService = stateMachineService;
        _messagingService = messagingService;
    }

    private async Task LoadBeveragesFromFileAsync()
    {
        try
        {
            await _loggingService.LogInformationAsync("Loading beverages from JSON file...");

            if (!(await _fileService.FileExistsAsync(BeveragesFilePath)).Data)
            {
                await _loggingService.LogWarningAsync($"Beverages file not found: {BeveragesFilePath}");
                return;
            }

            var jsonContent = await _fileService.ReadFileAsync(BeveragesFilePath);
            if (!jsonContent.IsSuccess)
            {
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new TimeSpanConverter() }
            };

            var jsonData = JsonSerializer.Deserialize<BeverageJsonData>(jsonContent.Data, options);
            if (jsonData?.Beverages == null)
            {
                return;
            }

            var beverages = new List<Beverage>();
            foreach (var beverageJson in jsonData.Beverages)
            {
                var beverageResult = MapToDomainEntity(beverageJson);
                if (beverageResult.IsSuccess)
                {
                    beverages.Add(beverageResult.Data);
                }
                else
                {
                    await _loggingService.LogWarningAsync($"Skipping invalid beverage: {beverageResult.Error}");
                }
            }

            _availableBeverages = beverages;
            await _loggingService.LogInformationAsync($"Successfully loaded {beverages.Count} beverages");
        }
        catch (Exception ex)
        {
            await _loggingService.LogWarningAsync($"Error reading beverages file: {BeveragesFilePath}.\n{ex.Message}");
        }      
    }

    public async Task<Result> SaveBeveragesAsync(List<Beverage> beverages)
    {
        try
        {
            var jsonData = new BeverageJsonData();
            foreach (var beverage in beverages)
            {
                var jsonModel = MapToJsonModel(beverage);
                jsonData.Beverages.Add(jsonModel);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new TimeSpanConverter() }
            };

            var jsonContent = JsonSerializer.Serialize(jsonData, options);
            var result = await _fileService.WriteFileAsync(BeveragesFilePath, jsonContent);

            if (result.IsSuccess)
            {
                await _loggingService.LogInformationAsync($"Successfully saved {beverages.Count} beverages");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error saving beverages: {ex.Message}", ex);
            return Result.Failure($"Error saving beverages: {ex.Message}");
        }
    }


    public async Task<Result<BeverageDto>> GetBeverageAsync(Guid beverageId)
    {
        try
        {
            var beverage = _availableBeverages.FirstOrDefault(b => b.Id == beverageId);
            if (beverage == null)
            {
                await _loggingService.LogWarningAsync($"Beverage not found: {beverageId}");
                return Result.Failure<BeverageDto>("Beverage not found");
            }

            return Result.Success(MapToDto(beverage));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting beverage: {ex.Message}", ex);
            return Result.Failure<BeverageDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BeverageDto>>> GetAllAvailableBeveragesAsync()
    {
        try
        {
            var availableBeverages = _availableBeverages
                .Where(b => b.IsAvailable)
                .Select(MapToDto)
                .ToList();

            await _loggingService.LogInformationAsync($"Retrieved {availableBeverages.Count} available beverages");
            return Result.Success(availableBeverages);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting available beverages: {ex.Message}", ex);
            return Result.Failure<List<BeverageDto>>($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(BeverageType beverageType, string size, string customerName,
                                                       bool addSugar = false, bool addMilk = false, string specialInstructions = null)
    {
        try
        {
            await LoadBeveragesFromFileAsync();
            var beverage = _availableBeverages.FirstOrDefault(b => b.Type == beverageType && b.IsAvailable);
            if (beverage == null)
            {
                await _loggingService.LogWarningAsync($"Beverage type not available: {beverageType}");
                return Result.Failure<OrderDto>("Beverage not available");
            }

            // Calculate price based on size
            var priceMultiplier = GetSizeMultiplier(size);
            var totalPrice = beverage.Price * priceMultiplier;

            var orderDto = new OrderDto
                (
                Id: Guid.NewGuid(),
                BeverageType: beverageType,
                BeverageName: beverage.Name,
                Size: size,
                CustomerName: customerName,
                TotalPrice: totalPrice,
                Status: "Created",
                ProgressPercentage: 0,
                OrderTime: DateTime.UtcNow,
                CompletionTime: DateTime.UtcNow,
                AddSugar: addSugar,
                AddMilk: addMilk,
                SpecialInstructions: specialInstructions,
                ErrorMessage: "");

            await _orderService.CreateOrderAsync(orderDto);

            await _loggingService.LogInformationAsync($"Created order for {beverage.Name}: {orderDto.Id}");
            return Result.Success(orderDto);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error creating order: {ex.Message}", ex);
            return Result.Failure<OrderDto>($"Error: {ex.Message}");
        }
    }

    public async Task<Result> StartBeveragePreparationAsync(OrderDto data)
    {
        try
        {
            await _loggingService.LogInformationAsync($"Starting preparation for order: {data.Id}");

            _ = Task.Run(() => ExecutePreparationWorkflowAsync(data.Id, data.BeverageType));            

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error starting preparation: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> CancelBeveragePreparationAsync(Guid orderId)
    {
        try
        {
            await _loggingService.LogInformationAsync($"Cancelling preparation for order: {orderId}");

            // Simulate cancellation
            await Task.Delay(100);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error cancelling preparation: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderStatusDto>> GetOrderStatusAsync(Guid orderId)
    {
        try
        {
            // This would normally come from a database or order service
            var statusDto = new OrderStatusDto(
                OrderId: orderId,
                Status: "InProgress",
                ProgressPercentage: 50,
                CurrentStep: "Brewing",
                EstimatedCompletionTime: DateTime.UtcNow.AddMinutes(2),
                ErrorMessage: string.Empty
            );

            return Result.Success(statusDto);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting order status: {ex.Message}", ex);
            return Result.Failure<OrderStatusDto>($"Error: {ex.Message}");
        }
    }

    private async Task ExecutePreparationWorkflowAsync(Guid orderId, BeverageType beverageType)
    {
        try
        {
            var orderResult = await _orderService.GetOrderAsync(orderId);
            if (!orderResult.IsSuccess)
            {
                await _loggingService.LogErrorAsync($"Order not found for preparation: {orderId}");
                return;
            }

            var order = orderResult.Data;
            var steps = GetPreparationSteps(beverageType);
            var totalSteps = steps.Count;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var progress = (int)((i + 1) / (double)totalSteps * 100);

                // Update state machine progress
                await _stateMachineService.SendEventAsync(new PreparationEvent(
                    orderId,
                    beverageType.ToString(),
                    "PreparationProgress",
                    progress,
                    step.Description
                ));

                // Send BeverageStatusMessage to UI
                await _messagingService.Publish(new BeverageStatusMessage(
                    order,
                    "Preparing",
                    progress,
                    step.Description
                ));

                await _loggingService.LogInformationAsync($"Step {i + 1}/{totalSteps}: {step.Description} ({progress}%)");

                // Execute the step
                await ExecutePreparationStep(step);

                // Simulate step duration
                await Task.Delay(step.Duration);

                // Check for cancellation
                if (await IsPreparationCancelled(orderId))
                {
                    await _stateMachineService.SendEventAsync(new OrderEvent(
                        orderId,
                        beverageType.ToString(),
                        "CancelPreparation"
                    ));

                    // Send cancellation status message
                    await _messagingService.Publish(new BeverageStatusMessage(
                        order,
                        "Cancelled",
                        0,
                        "Preparation cancelled"
                    ));

                    return;
                }
            }

            // Complete preparation
            await _stateMachineService.SendEventAsync(new OrderEvent(
                orderId,
                beverageType.ToString(),
                "PreparationComplete"
            ));

            // Send completion status message
            await _messagingService.Publish(new BeverageStatusMessage(
                order,
                "Completed",
                100,
                "Ready for pickup"
            ));

            await _loggingService.LogInformationAsync($"Preparation completed for order {orderId}");
        }
        catch (Exception ex)
        {
            await _stateMachineService.SendEventAsync(new OrderEvent(
                orderId,
                beverageType.ToString(),
                "PreparationFailed",
                new
                {
                    Error = ex.Message
                }
            ));

            // Send error status message
            var orderResult = await _orderService.GetOrderAsync(orderId);
            if (orderResult.IsSuccess)
            {
                await _messagingService.Publish(new BeverageStatusMessage(
                    orderResult.Data,
                    "Error",
                    0,
                    $"Error: {ex.Message}"
                ));
            }

            await _loggingService.LogErrorAsync($"Preparation failed for order {orderId}: {ex.Message}", ex);
        }
    }
    public async Task<Result> UpdateBeverageAvailabilityAsync(Guid beverageId, bool isAvailable)
    {
        try
        {
            var beverage = _availableBeverages.FirstOrDefault(b => b.Id == beverageId);
            if (beverage == null)
            {
                return Result.Failure("Beverage not found");
            }

            beverage.UpdateAvailability(isAvailable);
            await _loggingService.LogInformationAsync($"Updated beverage {beverageId} availability to: {isAvailable}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error updating availability: {ex.Message}", ex);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BeverageDto>>> GetBeveragesByTypeAsync(BeverageType type)
    {
        try
        {
            var beverages = _availableBeverages
                .Where(b => b.Type == type && b.IsAvailable)
                .Select(MapToDto)
                .ToList();

            return Result.Success(beverages);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error getting beverages by type: {ex.Message}", ex);
            return Result.Failure<List<BeverageDto>>($"Error: {ex.Message}");
        }
    }

    private BeverageDto MapToDto(Beverage beverage)
    {
        return new BeverageDto
        (
            Id: beverage.Id,
            Name: beverage.Name,
            Description: beverage.Description,
            Type: beverage.Type,
            BasePrice: beverage.Price,
            PreparationTime: beverage.PreparationTime,
            IsAvailable: beverage.IsAvailable,
            AvailableSizes: new List<string> { "Small", "Medium", "Large" },
            CustomizationOptions: new List<string> { "Extra Sugar", "Extra Milk", "No Foam" }
            );
    }

    private decimal GetSizeMultiplier(string size)
    {
        return size.ToLower() switch
        {
            "small" => 0.8m,
            "medium" => 1.0m,
            "large" => 1.2m,
            _ => 1.0m
        };
    }

    private Result<Beverage> MapToDomainEntity(BeverageJsonModel jsonModel)
    {
        try
        {
            var ingredients = new List<Ingredient>();
            foreach (var ingJson in jsonModel.Recipe.Ingredients)
            {
                ingredients.Add(new Ingredient(
                    ingJson.Name,
                    ingJson.Quantity,
                    ingJson.Unit,
                    ingJson.UnitPrice,
                    ingJson.IsOptional
                ));
            }

            var recipe = new Recipe(
                jsonModel.Recipe.Id,
                jsonModel.Recipe.Name,
                ingredients,
                jsonModel.Recipe.Instructions,
                jsonModel.Recipe.EstimatedPreparationTime
            );

            Beverage beverage = jsonModel.Type.ToLower() switch
            {
                "coffee" => new Coffee(
                    jsonModel.Id,
                    jsonModel.Name,
                    jsonModel.Description,
                    recipe,
                    jsonModel.Price,
                    jsonModel.PreparationTime
                ),
                "latte" => new Latte(
                    jsonModel.Id,
                    jsonModel.Name,
                    jsonModel.Description,
                    recipe,
                    jsonModel.Price,
                    jsonModel.PreparationTime,
                    jsonModel.MilkFoamThickness ?? 0.3m
                ),
                "espresso" => new Espresso(
                    jsonModel.Id,
                    jsonModel.Name,
                    jsonModel.Description,
                    recipe,
                    jsonModel.Price,
                    jsonModel.PreparationTime,
                    jsonModel.IntensityLevel ?? 8
                ),
                "cappuccino" => new Cappuccino(
                    jsonModel.Id,
                    jsonModel.Name,
                    jsonModel.Description,
                    recipe,
                    jsonModel.Price,
                    jsonModel.PreparationTime,
                    jsonModel.FoamToMilkRatio ?? 0.4m
                ),
                _ => throw new ArgumentException($"Unknown beverage type: {jsonModel.Type}")
            };

            return Result.Success(beverage);
        }
        catch (Exception ex)
        {
            return Result.Failure<Beverage>($"Error mapping beverage '{jsonModel.Name}': {ex.Message}");
        }
    }

    private BeverageJsonModel MapToJsonModel(Beverage beverage)
    {
        var jsonModel = new BeverageJsonModel
        {
            Id = beverage.Id,
            Name = beverage.Name,
            Description = beverage.Description,
            Price = beverage.Price,
            PreparationTime = beverage.PreparationTime,
            Recipe = new RecipeJsonModel
            {
                Id = beverage.Recipe.Id,
                Name = beverage.Recipe.Name,
                Instructions = beverage.Recipe.Instructions,
                EstimatedPreparationTime = beverage.Recipe.EstimatedPreparationTime
            }
        };

        // Set beverage type and specific properties
        switch (beverage)
        {
            case Latte latte:
                jsonModel.Type = "Latte";
                jsonModel.MilkFoamThickness = latte.MilkFoamThickness;
                break;
            case Espresso espresso:
                jsonModel.Type = "Espresso";
                jsonModel.IntensityLevel = espresso.IntensityLevel;
                break;
            case Cappuccino cappuccino:
                jsonModel.Type = "Cappuccino";
                jsonModel.FoamToMilkRatio = cappuccino.FoamToMilkRatio;
                break;
            default:
                jsonModel.Type = "Coffee";
                break;
        }

        // Map ingredients
        foreach (var ingredient in beverage.Recipe.Ingredients)
        {
            jsonModel.Recipe.Ingredients.Add(new IngredientJsonModel
            {
                Name = ingredient.Name,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                UnitPrice = ingredient.UnitPrice,
                IsOptional = ingredient.IsOptional
            });
        }

        return jsonModel;
    }

    // Preparation step classes and methods
    public class PreparationStep
    {
        public string Description
        {
            get; set;
        }
        public TimeSpan Duration
        {
            get; set;
        }
    }

    private List<PreparationStep> GetPreparationSteps(BeverageType beverageType)
    {
        return beverageType switch
        {
            BeverageType.Coffee => new List<PreparationStep>
                {
                    new() { Description = "Grinding coffee beans", Duration = TimeSpan.FromSeconds(2) },
                    new() { Description = "Heating water", Duration = TimeSpan.FromSeconds(3) },
                    new() { Description = "Brewing coffee", Duration = TimeSpan.FromSeconds(5) },
                    new() { Description = "Dispensing into cup", Duration = TimeSpan.FromSeconds(1) }
                },
            BeverageType.Latte => new List<PreparationStep>
                {
                    new() { Description = "Grinding coffee beans", Duration = TimeSpan.FromSeconds(2) },
                    new() { Description = "Brewing espresso", Duration = TimeSpan.FromSeconds(4) },
                    new() { Description = "Steaming milk", Duration = TimeSpan.FromSeconds(3) },
                    new() { Description = "Combining espresso and milk", Duration = TimeSpan.FromSeconds(2) },
                    new() { Description = "Adding foam", Duration = TimeSpan.FromSeconds(1) }
                },
            BeverageType.Espresso => new List<PreparationStep>
                {
                    new() { Description = "Fine grinding coffee beans", Duration = TimeSpan.FromSeconds(3) },
                    new() { Description = "Tamping grounds", Duration = TimeSpan.FromSeconds(1) },
                    new() { Description = "Brewing under pressure", Duration = TimeSpan.FromSeconds(4) },
                    new() { Description = "Dispensing shot", Duration = TimeSpan.FromSeconds(1) }
                },
            BeverageType.Cappuccino => new List<PreparationStep>
                {
                    new() { Description = "Grinding coffee beans", Duration = TimeSpan.FromSeconds(2) },
                    new() { Description = "Brewing espresso", Duration = TimeSpan.FromSeconds(4) },
                    new() { Description = "Steaming milk", Duration = TimeSpan.FromSeconds(3) },
                    new() { Description = "Creating microfoam", Duration = TimeSpan.FromSeconds(2) },
                    new() { Description = "Combining in perfect ratio", Duration = TimeSpan.FromSeconds(2) }
                },
            _ => new List<PreparationStep>
                {
                    new() { Description = "Preparing your beverage", Duration = TimeSpan.FromSeconds(3) },
                    new() { Description = "Finalizing preparation", Duration = TimeSpan.FromSeconds(2) }
                }
        };
    }

    private async Task ExecutePreparationStep(PreparationStep step)
    {
        try
        {
            // Simulate the actual work of the preparation step
            await Task.Delay(step.Duration);
            await _loggingService.LogDebugAsync($"Completed step: {step.Description}");
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error executing step {step.Description}: {ex.Message}", ex);
            throw;
        }
    }

    private async Task<bool> IsPreparationCancelled(Guid orderId)
    {
        try
        {
            var orderResult = await _orderService.GetOrderAsync(orderId);
            if (orderResult.IsSuccess)
            {
                return orderResult.Data.Status == "Cancelled";
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
