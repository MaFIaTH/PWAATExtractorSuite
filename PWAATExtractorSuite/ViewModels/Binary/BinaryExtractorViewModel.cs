using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using PWAATExtractorSuite.ViewModels.Shared;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using FileMode = PWAATExtractorSuite.Models.FileMode;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Binary;

public enum BinaryOperationMode
{
    Extract,
    Insert,
}

public class BinaryExtractorViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel
{
    public string? UrlPathSegment => "binary-extractor";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();
    public WorkspaceTabViewModel WorkspaceTab { get; }
    public BinaryOperationTabViewModel OperationTab { get; }
    
    #region Commands and Properties
    public ReactiveCommand RunWizardCommand { get; } = new();
    public ReactiveCommand OpenWorkspaceCommand { get; } = new();
    #endregion
    
    private readonly BinaryExtractorModel _model;
    private readonly IDialogService _dialogService;
    private readonly IWizardService _wizardService;
    private readonly ISaveService _saveService;
    
    public BinaryExtractorViewModel()
    { 
        WorkspaceTab = new BinaryWorkspaceTabViewModel();
        OperationTab = new BinaryOperationTabViewModel();
    }
    
    public BinaryExtractorViewModel(
        BinaryExtractorModel model,
        [FromKeyedServices(ExtractorType.Binary)] WorkspaceTabViewModel workspaceTab,
        BinaryOperationTabViewModel operationTab,
        [FromKeyedServices(ViewModelType.MainRouter)] IScreen screen,
        IDialogService dialogService,
        IWizardService wizardService,
        ISaveService saveService) 
        :this()
    {
        _model = model;
        WorkspaceTab = workspaceTab;
        OperationTab = operationTab;
        HostScreen = screen;
        _dialogService = dialogService;
        _wizardService = wizardService;
        _saveService = saveService;
        this.WhenActivated(Bind);
    }

    private void Bind(CompositeDisposable disposables)
    {
        Console.WriteLine("BinaryExtractorViewModel activated");
        if (HostScreen is MainRouterViewModel mainRouter)
        {
            Observable.FromEvent<ViewModelBase>(
                    h => mainRouter.ReloadEvent += h,
                    h => mainRouter.ReloadEvent -= h)
                .Where(vm => vm == this)
                .Subscribe(_ =>
                {
                    Console.WriteLine("BinaryExtractorViewModel reloaded");
                    if (_saveService.CurrentWorkspaceData is BinaryWorkspaceData binaryWorkspaceData)
                    {
                        SetUpWorkspace(binaryWorkspaceData);
                    }
                })
                .AddTo(disposables);
        }
        RunWizardCommand
            .SubscribeAwait(onNextAsync: (_, _) => OnRunWizard(), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenWorkspaceCommand
            .SubscribeAwait(onNextAsync: (_, _) => OnOpenWorkSpace(), AwaitOperation.Drop)
            .AddTo(disposables);
        WorkspaceTab.BindWhenParentActivate(disposables);
        OperationTab.BindWhenParentActivate(disposables);
        Observable.FromEvent<IWorkspaceData>(
                h => WorkspaceTab.WorkspaceDataChanged += h,
                h => WorkspaceTab.WorkspaceDataChanged -= h)
            .Subscribe(data =>
            {
                OnWorkspaceDataChanged((data as BinaryWorkspaceData)!);
                Console.WriteLine("Workspace data updated in BinaryExtractorViewModel");
            })
            .AddTo(disposables);
        switch (_saveService.CurrentWorkspaceData)
        {
            case BinaryWorkspaceData binaryWorkspaceData:
                SetUpWorkspace(binaryWorkspaceData);
                break;
            case null:
                var newWorkspaceData = new BinaryWorkspaceData();
                SetUpWorkspace(newWorkspaceData);
                break;
        }
    }

    private async ValueTask OnRunWizard()
    { 
        var result = await _dialogService.ShowWizardDialog(ExtractorType.Binary);
        if (result == null)
        {
            Console.WriteLine("Wizard cancelled");
            return;
        }
        if (!Directory.Exists(result) || string.IsNullOrWhiteSpace(result))
        {
            await _dialogService.ShowNotificationDialog("Path Not Found", $"""The selected path "{result}" does not exist.""");
            Console.WriteLine($"Path does not exist: {result}");
            return;
        }
        IWorkspaceData workspaceData;
        try
        {
            _wizardService.StartWizard(ExtractorType.Binary, result, out workspaceData);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Wizard Error", $"An error occurred while starting the wizard:\n{ex.Message}");
            Console.WriteLine("Error starting wizard.");
            return;
        } 
        SetUpWorkspace((workspaceData as BinaryWorkspaceData)!);
    }
    
    private async ValueTask OnOpenWorkSpace()
    { 
        Console.WriteLine("Opening workspace...");
        var workspaceData = await _saveService.OpenWorkspaceAsync<BinaryWorkspaceData>();
        if (workspaceData == null)
        {
            Console.WriteLine("No workspace data loaded.");
            return;
        }
        SetUpWorkspace(workspaceData);
    }

    private void SetUpWorkspace(BinaryWorkspaceData workspaceData)
    {
        _saveService.CurrentWorkspaceData = workspaceData;
        _model.WorkspaceData.Value = workspaceData;
        WorkspaceTab.Root.Path.Value = workspaceData.RootWorkspacePath;
        WorkspaceTab.Children.Clear();
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Extraction Input", workspaceData.ExtractionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Extraction Output", workspaceData.ExtractionOutputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Insertion Input", workspaceData.InsertionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Insertion Output", workspaceData.InsertionOutputPath));
    }
    
    private void OnWorkspaceDataChanged(BinaryWorkspaceData data)
    {
        _saveService.CurrentWorkspaceData = data;
        _model.WorkspaceData.Value = data;
    }
}