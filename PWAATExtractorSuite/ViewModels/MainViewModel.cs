using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;

namespace PWAATExtractorSuite.ViewModels;

public enum ViewModelType
{
    MainRouter,
    Menu,
    Settings,
    OnBoarding,
    BinaryExtractor,
    ScenarioExtractor,
    CryptographyExtractor,
}

public partial class MainViewModel : ViewModelBase, IActivatableViewModel, IDisposable
{
    public RoutingState Router => MainRouterViewModel.Router;
    public IReadOnlyDictionary<ViewModelType, ViewModelBase> ViewModels => _viewModels;
    public ViewModelActivator Activator { get; } = new();
    
    public ReactiveCommand<WindowClosingEventArgs> WindowClosingCommand { get; } = new();
    
    public BindableReactiveProperty<string> WindowTitle { get; } = new("PWAAT Extractor Suite");
    
    private MainRouterViewModel MainRouterViewModel => (MainRouterViewModel)_viewModels[ViewModelType.MainRouter];
    private readonly Dictionary<ViewModelType, ViewModelBase> _viewModels = new();
    private readonly AppSettings _appSettings;
    private readonly IDialogService _dialogService;
    private readonly ISaveService _saveService;
    private readonly IDisposable _shutdownSubscription;
    
    private bool _isShuttingDown;
    
    public MainViewModel()
    {
        this.WhenActivated(Activate);
        var disposableBuilder = Disposable.CreateBuilder();
        WindowClosingCommand
            .SubscribeAwait((args, _) => OnWindowClosing(args), AwaitOperation.Parallel)
            .AddTo(ref disposableBuilder);
        _shutdownSubscription = disposableBuilder.Build();
    }

    public MainViewModel(
        AppSettings appSettings,
        IDialogService dialogService,
        ISaveService saveService) 
        : this()
    {
        _appSettings = appSettings;
        _dialogService = dialogService;
        _saveService = saveService;
    }
    
    public void AddViewModel(ViewModelType type, ViewModelBase viewModel)
    {
        _viewModels[type] = viewModel;
    }
    
    private void Activate(CompositeDisposable disposables)
    {
        Console.WriteLine("MainViewModel Activated");
        Observable.FromEvent(
                handler => _saveService.WorkspaceChanged += handler,
                handler => _saveService.WorkspaceChanged -= handler)
            .Subscribe(_ =>
            {
                OnWorkspaceChanged();
            })
            .AddTo(disposables);
        
        OnWorkspaceChanged();
        _ = LoadSettingsAsync();
    }
    
    public void Dispose()
    {
        _shutdownSubscription.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private void OnWorkspaceChanged()
    {
        var workspacePath = _saveService.CurrentWorkspacePath;
        var hasWorkspace = _saveService.CurrentWorkspaceData != null;
        var hasPath = !string.IsNullOrEmpty(workspacePath) && File.Exists(workspacePath);
        var dirtyMarker = _saveService.IsDirty ? "*" : string.Empty;
        var workspaceName = hasPath
            ? Path.GetFileName(workspacePath)
            : "Untitled Workspace";
        var title = hasWorkspace
            ? $"PWAAT Extractor Suite - {workspaceName}{dirtyMarker}"
            : "PWAAT Extractor Suite";
        Console.WriteLine($"Window title updated to: {title}");
        WindowTitle.Value = title;
    }
    
    private async Task LoadSettingsAsync()
    {
        await _appSettings.Load();
        if (_appSettings.Data.OpenLastWorkspaceOnStartup)
        {
            var path = _appSettings.Data.LastOpenedWorkspacePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MainRouterViewModel.NavigateTo(ViewModelType.OnBoarding);
                return;
            }
            var workspaceData = await _saveService.TryLoadWorkspaceDataAsync(path);
            if (workspaceData == null)
            {
                MainRouterViewModel.NavigateTo(ViewModelType.OnBoarding);
                return;
            }
            switch (workspaceData)
            {
                case BinaryWorkspaceData:
                    MainRouterViewModel.NavigateTo(ViewModelType.BinaryExtractor);
                    break;
                case ScenarioWorkspaceData:
                    MainRouterViewModel.NavigateTo(ViewModelType.ScenarioExtractor);
                    break;
                case CryptographyWorkspaceData:
                    MainRouterViewModel.NavigateTo(ViewModelType.CryptographyExtractor);
                    break;
                default:
                    MainRouterViewModel.NavigateTo(ViewModelType.OnBoarding);
                    break;
            }
        }
        else
        {
            MainRouterViewModel.NavigateTo(ViewModelType.OnBoarding);
        }
    }

    private async ValueTask OnWindowClosing(WindowClosingEventArgs args)
    {
        if (args.CloseReason is WindowCloseReason.OSShutdown)
        {
            (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            return;
        }
        if (_isShuttingDown)
        {
            args.Cancel = true;
            return;
        }
        _isShuttingDown = true;
        if (_saveService.IsDirty)
        {
            args.Cancel = true;
            var saveWhenExitResult = await _dialogService.ShowConfirmationDialog(
                title: "Unsaved Changes",
                message: "You have unsaved changes in your workspace. Do you want to save before exiting?",
                hasCancel: true);
            switch (saveWhenExitResult)
            {
                case ConfirmationDialogResult.Cancel or null:
                    _isShuttingDown = false;
                    return;
                case ConfirmationDialogResult.No:
                    break;
                case ConfirmationDialogResult.Yes:
                    var hasWorkspace = _saveService.CurrentWorkspaceData != null;
                    var hasPath = !string.IsNullOrEmpty(_saveService.CurrentWorkspacePath) && File.Exists(_saveService.CurrentWorkspacePath);
                    if (hasPath && hasWorkspace)
                    {
                        await _saveService.SaveCurrentWorkspaceAsync();
                        break;
                    }
                    await _saveService.SaveNewWorkspaceAsync();
                    break;
            }
        }
        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }
}