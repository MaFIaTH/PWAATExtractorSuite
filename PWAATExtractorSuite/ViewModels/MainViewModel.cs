using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PWAATExtractorSuite.Models;
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
}

public partial class MainViewModel : ViewModelBase, IActivatableViewModel
{
    public RoutingState Router => MainRouterViewModel.Router;
    public IReadOnlyDictionary<ViewModelType, ViewModelBase> ViewModels => _viewModels;
    public ViewModelActivator Activator { get; } = new();
    
    public BindableReactiveProperty<string> WindowTitle { get; } = new("PWAAT Extractor Suite");
    
    private MainRouterViewModel MainRouterViewModel => (MainRouterViewModel)_viewModels[ViewModelType.MainRouter];
    private readonly Dictionary<ViewModelType, ViewModelBase> _viewModels = new();
    private readonly AppSettings _appSettings;
    private readonly ISaveService _saveService;
    
    public MainViewModel()
    {
        this.WhenActivated(Activate);
    }

    public MainViewModel(
        AppSettings appSettings,
        ISaveService saveService) 
        : this()
    {
        _appSettings = appSettings;
        _saveService = saveService;
    }
    
    public void AddViewModel(ViewModelType type, ViewModelBase viewModel)
    {
        _viewModels[type] = viewModel;
    }
    
    private void Activate(CompositeDisposable disposables)
    {
        Console.WriteLine("MainViewModel Activated");
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
        _ = LoadSettingsAsync();
    }
    
    private void OnWorkspaceChanged()
    {
        var workspacePath = _saveService.CurrentWorkspacePath;
        var hasWorkspace = _saveService.CurrentWorkspaceData != null;
        var hasPath = !string.IsNullOrEmpty(workspacePath) && File.Exists(workspacePath);
        var workspaceName = hasPath
            ? Path.GetFileName(workspacePath)
            : "Untitled Workspace";
        var title = hasWorkspace
            ? $"PWAAT Extractor Suite - {workspaceName}"
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
}