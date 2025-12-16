using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels;
using PWAATExtractorSuite.ViewModels.Binary;
using PWAATExtractorSuite.ViewModels.Dialogs;
using PWAATExtractorSuite.ViewModels.Shared;
using PWAATExtractorSuite.Views;
using ReactiveUI;

namespace PWAATExtractorSuite;

public partial class App : Application
{
    public ServiceProvider ServiceProvider { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        RegisterServices(collection);
        ServiceProvider = collection.BuildServiceProvider();
        RegisterViewModels(ServiceProvider);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
            Console.WriteLine("Application started in Desktop mode.");
        }
        // else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        // {
        //     singleViewPlatform.MainView = new MainView
        //     {
        //         ViewModel = mainViewModel,
        //         DataContext = mainViewModel
        //     };
        //     StorageProvider = TopLevel.GetTopLevel(singleViewPlatform.MainView)!.StorageProvider;
        // }
        base.OnFrameworkInitializationCompleted();
    }
    
    private void RegisterServices(ServiceCollection services)
    {
        //Main Window
        services.AddSingleton<MainWindow>();
        
        //Messaging
        services.AddMessagePipe();
        
        //Settings
        services.AddSingleton<AppSettings>();
        
        //Top Level Services
        services.AddSingleton<IStorageProvider>(sp =>
        {
            var mainWindow = sp.GetRequiredService<MainWindow>();
            return mainWindow.StorageProvider;
        });
        services.AddSingleton<ILauncher>(sp =>
        {
            var mainWindow = sp.GetRequiredService<MainWindow>();
            return mainWindow.Launcher;
        });
        
        //Router ViewModels
        services.AddKeyedSingleton<IScreen, MainRouterViewModel>(ViewModelType.MainRouter);
        
        //Main ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MenuViewModel>();
        services.AddSingleton<OnBoardingViewModel>();
        services.AddSingleton<SettingsViewModel>();
        
        //Binary Extractor
        services.AddSingleton<BinaryExtractorModel>();
        services.AddTransient<BinaryExtractorViewModel>();
        services.AddTransient<BinaryOperationTabViewModel>();
        
        //Shared ViewModels
        services.AddKeyedTransient<WorkspaceTabViewModel, BinaryWorkspaceTabViewModel>(ExtractorType.Binary);
        
        //Dialog ViewModels
        services.AddTransient<NotificationDialogViewModel>();
        services.AddTransient<WizardDialogViewModel>();
        services.AddTransient<AboutDialogViewModel>();
        
        //Dialog Services
        services.AddSingleton<IDialogService>(sp => new DialogService(
            viewModelFactory: sp.GetRequiredService,
            storageProviderFactory: sp.GetRequiredService<IStorageProvider>));

        //Wizard Services
        services.AddSingleton<IWizardService, WizardService>();
        
        //Save Services
        services.AddSingleton<ISaveService, SaveService>();
    }
    
    private void RegisterViewModels(ServiceProvider provider)
    {
        var mainViewModel = provider.GetRequiredService<MainViewModel>();
        var menuViewModel = provider.GetRequiredService<MenuViewModel>();
        var routerViewModel = provider.GetRequiredKeyedService<IScreen>(ViewModelType.MainRouter) as MainRouterViewModel;
        var settingsViewModel = provider.GetRequiredService<SettingsViewModel>();
        var onBoardingViewModel = provider.GetRequiredService<OnBoardingViewModel>();
        var binaryExtractorViewModel = provider.GetRequiredService<BinaryExtractorViewModel>();
        mainViewModel.AddViewModel(ViewModelType.Menu, menuViewModel);
        mainViewModel.AddViewModel(ViewModelType.MainRouter, routerViewModel!);
        mainViewModel.AddViewModel(ViewModelType.Settings, settingsViewModel);
        mainViewModel.AddViewModel(ViewModelType.OnBoarding, onBoardingViewModel);
        mainViewModel.AddViewModel(ViewModelType.BinaryExtractor, binaryExtractorViewModel);
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}