using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoffeeMachine.Core.Application.Commands;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Core.Common;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CoffeeMachine.Core.Application.Queries;
using CoffeeMachine.Core.Abstractions.Logging;

namespace CoffeeMachine.UI.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private readonly IMediator _mediator;
    private readonly ILoggingService _loggingService;
    private readonly IMessagingService _messagingService;

    private BeverageDto _selectedBeverage;
    private string _selectedSize = "Medium";
    private string _customerName = "Guest";
    private bool _addSugar;
    private bool _addMilk;
    private string _statusMessage;
    private bool _isProcessing;

    public MainViewModel(
        IMediator mediator,
        ILoggingService loggingService,
        IMessagingService messagingService)
    {
        _mediator = mediator;
        _loggingService = loggingService;
        _messagingService = messagingService;

        AvailableBeverages = new ObservableCollection<BeverageDto>();
        AvailableSizes = new ObservableCollection<string> { "Small", "Medium", "Large" };

        InitializeCommands();
        SubscribeToMessages();
        _ = LoadAvailableBeveragesAsync();
    }

    public ObservableCollection<BeverageDto> AvailableBeverages
    {
        get;
    }
    public ObservableCollection<string> AvailableSizes
    {
        get;
    }

    public BeverageDto SelectedBeverage
    {
        get => _selectedBeverage;
        set => SetProperty(ref _selectedBeverage, value);
    }

    public string SelectedSize
    {
        get => _selectedSize;
        set => SetProperty(ref _selectedSize, value);
    }

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public bool AddSugar
    {
        get => _addSugar;
        set => SetProperty(ref _addSugar, value);
    }

    public bool AddMilk
    {
        get => _addMilk;
        set => SetProperty(ref _addMilk, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public AsyncRelayCommand OrderCommand
    {
        get; private set;
    }
    public AsyncRelayCommand RefreshCommand
    {
        get; private set;
    }

    private void InitializeCommands()
    {
        OrderCommand = new AsyncRelayCommand(OrderBeverageAsync, CanOrder);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    private void SubscribeToMessages()
    {
        _messagingService.Subscribe<BeverageStatusMessage>(HandleStatusMessage);
        _messagingService.Subscribe<ErrorMessage>(HandleErrorMessage);
    }

    private void HandleStatusMessage(BeverageStatusMessage message)
    {
        App.GetDispatcher().TryEnqueue(() =>
        {
            StatusMessage = $"{message.BeverageName}: {message.Status} ({message.ProgressPercentage}%)";
        });
    }

    private void HandleErrorMessage(ErrorMessage message)
    {
        App.GetDispatcher().TryEnqueue(() =>
        {
            StatusMessage = $"Error: {message.Message}";
        });
    }

    private async Task LoadAvailableBeveragesAsync()
    {
        try
        {
            var query = new GetAvailableBeveragesQuery();
            var result = await _mediator.Query<GetAvailableBeveragesQuery, Result<BeverageDto>>(query);

            if (result.IsSuccess)
            {
                AvailableBeverages.Clear();
                //foreach (var beverage in result.Data)
                {
                    AvailableBeverages.Add(result.Data);
                }

                if (AvailableBeverages.Count > 0)
                {
                    SelectedBeverage = AvailableBeverages[0];
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load beverages: {ex.Message}";
        }
    }

    private async Task OrderBeverageAsync()
    {
        if (SelectedBeverage == null)
            return;

        try
        {
            IsProcessing = true;
            StatusMessage = "Placing order...";

            var command = new CreateBeverageCommand(
                SelectedBeverage.Type,
                SelectedSize,
                CustomerName,
                AddSugar,
                AddMilk
            );

            var result = await _mediator.Send<CreateBeverageCommand, Result<OrderDto>>(command);

            if (result.IsSuccess)
            {
                StatusMessage = $"Order placed! Order ID: {result.Data.Id}";
            }
            else
            {
                StatusMessage = $"Order failed: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanOrder()
    {
        return SelectedBeverage != null && !IsProcessing;
    }

    private async Task RefreshAsync()
    {
        await LoadAvailableBeveragesAsync();
        StatusMessage = "Refreshed";
    }
}