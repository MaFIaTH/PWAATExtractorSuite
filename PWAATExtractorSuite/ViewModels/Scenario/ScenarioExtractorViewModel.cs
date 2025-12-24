using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using PWAATExtractorSuite.ViewModels.Shared;
using R3;
using ReactiveUI;
using ReactiveCommand = R3.ReactiveCommand;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;

namespace PWAATExtractorSuite.ViewModels.Scenario;

public enum ScenarioOperationMode
{
    Extract,
    Insert,
    Simplify,
    Desimplify
}

public class ScenarioExtractorViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel
{
    public string UrlPathSegment => "scenario-extractor";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();

    public WorkspaceTabViewModel WorkspaceTab { get; } = new ScenarioWorkspaceTabViewModel();
    public ScenarioSpeakerDefinitionTabViewModel SpeakerDefinitionTab { get; } = new();
    public ScenarioOperationTabViewModel OperationTab { get; } = new();
    
    #region Commands and Properties
    public ReactiveCommand RunWizardCommand { get; } = new();
    public ReactiveCommand OpenWorkspaceCommand { get; } = new();
    #endregion
    
    private readonly ScenarioExtractorModel _model;
    private readonly IDialogService _dialogService;
    private readonly IWizardService _wizardService;
    private readonly ISaveService _saveService;

    public ScenarioExtractorViewModel()
    {
        this.WhenActivated(Bind);
    }

    public ScenarioExtractorViewModel(
        ScenarioExtractorModel model,
        [FromKeyedServices(ExtractorType.Scenario)] WorkspaceTabViewModel workspaceTab,
        ScenarioSpeakerDefinitionTabViewModel speakerDefinitionTab,
        ScenarioOperationTabViewModel operationTab,
        [FromKeyedServices(ViewModelType.MainRouter)] IScreen hostScreen,
        IDialogService dialogService,
        IWizardService wizardService,
        ISaveService saveService)
        :this()
    {
        WorkspaceTab = workspaceTab;
        SpeakerDefinitionTab = speakerDefinitionTab;
        OperationTab = operationTab;
        HostScreen = hostScreen;
        _model = model;
        _dialogService = dialogService;
        _wizardService = wizardService;
        _saveService = saveService;
    }
    
    private void Bind(CompositeDisposable disposables)
    {
        Console.WriteLine("ScenarioExtractorViewModel activated");
        if (HostScreen is MainRouterViewModel mainRouter)
        {
            Observable.FromEvent<ViewModelBase>(
                    h => mainRouter.ReloadEvent += h,
                    h => mainRouter.ReloadEvent -= h)
                .Where(vm => vm == this)
                .Subscribe(_ =>
                {
                    Console.WriteLine("ScenarioExtractorViewModel reloaded");
                    if (_saveService.CurrentWorkspaceData is ScenarioWorkspaceData scenarioWorkspaceData)
                    {
                        SetUpWorkspace(scenarioWorkspaceData);
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
        SpeakerDefinitionTab.BindWhenParentActivate(disposables);
        OperationTab.BindWhenParentActivate(disposables);
        _model.WorkspaceData
            .Skip(1)
            .Subscribe(data =>
            {
                OnWorkspaceDataChanged(data);
                Console.WriteLine("Workspace data updated in BinaryExtractorViewModel");
            })
            .AddTo(disposables);
        switch (_saveService.CurrentWorkspaceData)
        {
            case ScenarioWorkspaceData scenarioWorkspaceData:
                SetUpWorkspace(scenarioWorkspaceData);
                break;
            default:
                var newWorkspaceData = new ScenarioWorkspaceData();
                SetUpWorkspace(newWorkspaceData);
                break;
        }
    }

    private async ValueTask OnRunWizard()
    { 
        var result = await _dialogService.ShowWizardDialog(ExtractorType.Scenario);
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
            _wizardService.StartWizard(ExtractorType.Scenario, result, out workspaceData);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Wizard Error", $"An error occurred while starting the wizard:\n{ex.Message}");
            Console.WriteLine($"Error starting wizard.");
            return;
        } 
        SetUpWorkspace((workspaceData as ScenarioWorkspaceData)!);
    }
    
    private async ValueTask OnOpenWorkSpace()
    { 
        Console.WriteLine("Opening workspace...");
        var workspaceData = await _saveService.OpenWorkspaceAsync<ScenarioWorkspaceData>();
        if (workspaceData == null)
        {
            Console.WriteLine("No workspace data loaded.");
            return;
        }
        SetUpWorkspace(workspaceData);
    }

    private void SetUpWorkspace(ScenarioWorkspaceData workspaceData)
    {
        //_saveService.CurrentWorkspaceData = workspaceData;
        _model.WorkspaceData.Value = workspaceData;
        WorkspaceTab.Root.Path.Value = workspaceData.RootWorkspacePath;
        WorkspaceTab.Children.Clear();
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Extraction Input", workspaceData.ExtractionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Extraction Output", workspaceData.ExtractionOutputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Insertion Input", workspaceData.InsertionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Insertion Output", workspaceData.InsertionOutputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Simplification Input", workspaceData.SimplificationInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Simplification Output", workspaceData.SimplificationOutputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Desimplification Original", workspaceData.DesimplificationOriginalPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Desimplification Input", workspaceData.DesimplificationInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Desimplification Output", workspaceData.DesimplificationOutputPath));
        SpeakerDefinitionTab.SpeakerDefinition.Path.Value = workspaceData.SpeakerDefinitionPath;
        _ = SpeakerDefinitionTab.LoadSpeakerDefinitions(workspaceData.SpeakerDefinitionPath);
    }
    
    private void OnWorkspaceDataChanged(ScenarioWorkspaceData data)
    {
        _saveService.CurrentWorkspaceData = data;
    }
}