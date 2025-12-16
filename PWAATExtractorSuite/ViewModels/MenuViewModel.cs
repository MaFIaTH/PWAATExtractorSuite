using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;


namespace PWAATExtractorSuite.ViewModels;

public partial class MenuViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public ReactiveCommand<ExtractorType> NewWorkspaceCommand { get; } = new();
    public ReactiveCommand OpenWorkspaceCommand { get; } = new();
    public ReactiveCommand SaveCurrentWorkspaceCommand { get; } = new();
    public ReactiveCommand SaveNewWorkspaceCommand { get; } = new();
    public ReactiveCommand ExitApplicationCommand { get; } = new();
    public ReactiveCommand SettingsCommand { get; } = new();
    public ReactiveCommand AboutCommand { get; } = new();
    private readonly ISaveService _saveService;
    private readonly IDialogService _dialogService;
    private readonly IScreen _router;

    public MenuViewModel()
    {
        this.WhenActivated(Activate);
    }

    public MenuViewModel(
        ISaveService saveService,
        IDialogService dialogService,
        [FromKeyedServices(ViewModelType.MainRouter)] IScreen router) : this()
    {
        _saveService = saveService;
        _dialogService = dialogService;
        _router = router;
    }
    
    private void Activate(CompositeDisposable disposables)
    {
        NewWorkspaceCommand
            .SubscribeAwait((extractorType, _) => OnNewWorkSpace(extractorType), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenWorkspaceCommand
            .SubscribeAwait((_, _) => OnOpenWorkspace(), AwaitOperation.Drop)
            .AddTo(disposables);
        SaveCurrentWorkspaceCommand
            .SubscribeAwait((_, _) => OnSaveCurrentWorkspace(), AwaitOperation.Drop)
            .AddTo(disposables);
        SaveNewWorkspaceCommand
            .SubscribeAwait((_, _) => OnSaveNewWorkspace(), AwaitOperation.Drop)
            .AddTo(disposables);
        ExitApplicationCommand
            .SubscribeAwait((_, _) => OnExitApplication(), AwaitOperation.Drop)
            .AddTo(disposables);
        SettingsCommand
            .Subscribe(_ => OnSettings())
            .AddTo(disposables);
        AboutCommand
            .SubscribeAwait((_, _) => OnAbout(), AwaitOperation.Drop)
            .AddTo(disposables);
        Observable.EveryValueChanged(_saveService, service => service.CurrentWorkspaceData)
            .Subscribe(_ =>
            {
                OnWorkspaceChanged();
            })
            .AddTo(disposables);
        Observable.EveryValueChanged(_saveService, service => service.CurrentWorkspacePath)
            .Subscribe(_ =>
            {
                OnWorkspaceChanged();
            })
            .AddTo(disposables);
        OnWorkspaceChanged();
    }

    private void OnWorkspaceChanged()
    {
        var hasWorkspace = _saveService.CurrentWorkspaceData != null;
        var hasPath = !string.IsNullOrEmpty(_saveService.CurrentWorkspacePath) && File.Exists(_saveService.CurrentWorkspacePath);
        SaveCurrentWorkspaceCommand.ChangeCanExecute(hasWorkspace && hasPath);
        SaveNewWorkspaceCommand.ChangeCanExecute(hasWorkspace);
    }

    private async ValueTask OnNewWorkSpace(ExtractorType extractorType)
    {
        switch (extractorType)
        {
            case ExtractorType.Binary:
                if (_router is not MainRouterViewModel mainRouter)
                {
                    throw new InvalidOperationException("Router is not MainRouterViewModel");
                }
                var binaryWorkspaceData = new BinaryWorkspaceData();
                _saveService.CurrentWorkspaceData = binaryWorkspaceData;
                _saveService.CurrentWorkspacePath = null;
                mainRouter.NavigateTo(ViewModelType.BinaryExtractor);
                break;
            case ExtractorType.Scenario:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(extractorType), extractorType, null);
        }
    }

    private async ValueTask OnOpenWorkspace()
    {
        var workspaceData = await _saveService.OpenWorkspaceAsync();
        if (_router is not MainRouterViewModel mainRouter)
        {
            throw new InvalidOperationException("Router is not MainRouterViewModel");
        }
        switch (workspaceData)
        {
            case BinaryWorkspaceData:
                mainRouter.NavigateTo(ViewModelType.BinaryExtractor);
                break;
            default:
                throw new NotSupportedException("Unsupported workspace data type");
        }
    }

    private async ValueTask OnSaveCurrentWorkspace()
    {
        await _saveService.SaveCurrentWorkspaceAsync();
    }
    
    private async ValueTask OnSaveNewWorkspace()
    {
        await _saveService.SaveWorkspaceAsync();
    }
    
    private async ValueTask OnExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    private void OnSettings()
    {
        if (_router is not MainRouterViewModel mainRouter)
        {
            throw new InvalidOperationException("Router is not MainRouterViewModel");
        }
        mainRouter.NavigateTo(ViewModelType.Settings);
    }

    private async ValueTask OnAbout()
    {
        await _dialogService.ShowAboutDialog();
    }
}