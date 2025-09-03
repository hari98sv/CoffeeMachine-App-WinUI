using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CoffeeMachine.Core.Application.Commands;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Application.Models;
using CoffeeMachine.Core.Application.Queries;
using CoffeeMachine.Core.Common;
using CoffeeMachine.Core.Common.Messages;
using CoffeeMachine.Core.Common.Results;
using CoffeeMachine.Infrastructure.Abstractions;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeeMachine.ViewModels;

public class OrderStatusViewModel : ObservableRecipient
{
    private readonly IMessagingService _messagingService;
    private readonly IMediator _mediator;
    private readonly ILoggingService _loggingService;
    private IDisposable _messageSubscription;

    private OrderDto _currentOrder;
    private string _status;
    private int _progress;
    private bool _isProcessing;
    private string _errorMessage;

    public OrderStatusViewModel(
        IMessagingService messagingService,
        IMediator mediator,
        ILoggingService loggingService)
    {
        _messagingService = messagingService;
        _mediator = mediator;
        _loggingService = loggingService;

        SubscribeToMessages();
        InitializeCommands();
    }

    public OrderDto CurrentOrder
    {
        get => _currentOrder;
        set => SetProperty(ref _currentOrder, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public AsyncRelayCommand CancelOrderCommand
    {
        get; private set;
    }
    public AsyncRelayCommand RefreshStatusCommand
    {
        get; private set;
    }

    private void InitializeCommands()
    {
        CancelOrderCommand = new AsyncRelayCommand(CancelOrderAsync, CanCancelOrder);
        RefreshStatusCommand = new AsyncRelayCommand(RefreshStatusAsync);
    }

    private void SubscribeToMessages()
    {
        _messageSubscription = _messagingService.Subscribe<BeverageStatusMessage>(HandleStatusMessage);
        _messagingService.Subscribe<ErrorMessage>(HandleErrorMessage);
    }

    private void HandleStatusMessage(BeverageStatusMessage message)
    {
        CurrentOrder = message.Order;
        Status = message.Status;
        Progress = message.ProgressPercentage;
        IsProcessing = message.ProgressPercentage < 100;
        ErrorMessage = string.Empty;
    }

    private void HandleErrorMessage(ErrorMessage message)
    {
        ErrorMessage = message.Message;
        Status = "Error";
        IsProcessing = false;
    }

    private async Task CancelOrderAsync()
    {
        if (CurrentOrder == null) return;

        try
        {
            IsProcessing = true;
            var command = new CancelBeverageCommand(CurrentOrder.Id);
            var result = await _mediator.Send<CancelBeverageCommand, Result>(command);

            if (result.IsSuccess)
            {
                Status = "Cancelled";
                Progress = 0;
                await _loggingService.LogInformationAsync($"Order {CurrentOrder.Id} cancelled successfully");
            }
            else
            {
                ErrorMessage = result.Error;
                await _loggingService.LogWarningAsync($"Failed to cancel order {CurrentOrder.Id}: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error cancelling order: {ex.Message}";
            await _loggingService.LogErrorAsync($"Error cancelling order {CurrentOrder.Id}", ex);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanCancelOrder()
    {
        return CurrentOrder != null && IsProcessing && Progress < 100;
    }

    private async Task RefreshStatusAsync()
    {
        if (CurrentOrder == null) return;

        try
        {
            var query = new GetBeverageStatusQuery(CurrentOrder.Id);
            var result = await _mediator.Query<GetBeverageStatusQuery, Result<OrderStatusDto>>(query);

            if (result.IsSuccess)
            {
                Status = result.Data.Status;
                Progress = result.Data.ProgressPercentage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error refreshing status: {ex.Message}";
            await _loggingService.LogErrorAsync($"Error refreshing order status {CurrentOrder.Id}", ex);
        }
    }
}