using System;
using Microsoft.UI.Xaml;
using CoffeeMachine.Core.Application.Mediator.Interfaces;
using CoffeeMachine.Core.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using CoffeeMachine.Core.Application.Mediator;
using Microsoft.Extensions.Hosting;
using CoffeeMachine.Core.Application.Commands.Handlers;
using CoffeeMachine.Core.Application.Queries;
using CoffeeMachine.UI.ViewModels;
using CoffeeMachine.Core.Application.Services;
using Microsoft.UI.Dispatching;
using CoffeeMachine.Core.Abstractions.Logging;
using CoffeeMachine.Infrastructure.Services.Logging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CoffeeMachine.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IHost _host;
        private static MainWindow _mainWindow;

        public App()
        {
            InitializeComponent();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            });

            _host = builder.Build();
        }

        public static DispatcherQueue GetDispatcher()
        {
            return _mainWindow.DispatcherQueue;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            // Register Mediator
            services.AddSingleton<IMediator, Mediator>();
            
            // Register all command handlers
            services.AddTransient<CreateBeverageCommandHandler>();
            services.AddTransient<CancelBeverageCommandHandler>();

            // Register all query handlers
            services.AddTransient<GetBeverageStatusQueryHandler>();
            services.AddTransient<GetAvailableBeveragesQueryHandler>();


            services.AddTransient<IBeverageService, BeverageService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IInventoryService, InventoryService>();

            // Register Services
            services.AddSingleton<ILoggingService, ConsoleLogger>();
            services.AddSingleton<IMessagingService, MessagingService>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _mainWindow = GetService<MainWindow>();
            _mainWindow.Activate();
        }

        public static T GetService<T>() where T : class
        {
            return _host.Services.GetService<T>() is T service
                ? service
                : throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }
    }
}
