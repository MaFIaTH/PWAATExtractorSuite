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

namespace PWAATExtractorSuite.ViewModels.Cryptography;

public enum CryptographyOperationMode
{
    Decrypt,
    Encrypt,
}

public class CryptographyExtractorViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel
{
    public string UrlPathSegment => "cryptography-extractor";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();
    public WorkspaceTabViewModel WorkspaceTab { get; } = new CryptographyWorkspaceTabViewModel();
    public CryptographyOperationTabViewModel OperationTab { get; } = new();
    
    #region Commands and Properties
    public ReactiveCommand RunWizardCommand { get; } = new();
    public ReactiveCommand OpenWorkspaceCommand { get; } = new();
    #endregion
    
    private readonly CryptographyExtractorModel _model;
    private readonly IDialogService _dialogService;
    private readonly IWizardService _wizardService;
    private readonly ISaveService _saveService;
    
    public CryptographyExtractorViewModel()
    { 
        this.WhenActivated(Bind);
    }
    
    public CryptographyExtractorViewModel(
        CryptographyExtractorModel model,
        [FromKeyedServices(ExtractorType.Cryptography)] WorkspaceTabViewModel workspaceTab,
        CryptographyOperationTabViewModel operationTab,
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
    }

    private void Bind(CompositeDisposable disposables)
    {
        Console.WriteLine("CryptographyExtractorViewModel activated");
        if (HostScreen is MainRouterViewModel mainRouter)
        {
            Observable.FromEvent<ViewModelBase>(
                    h => mainRouter.ReloadEvent += h,
                    h => mainRouter.ReloadEvent -= h)
                .Where(vm => vm == this)
                .Subscribe(_ =>
                {
                    Console.WriteLine("CryptographyExtractorViewModel reloaded");
                    if (_saveService.CurrentWorkspaceData is CryptographyWorkspaceData cryptographyWorkspaceData)
                    {
                        SetUpWorkspace(cryptographyWorkspaceData);
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
        _model.WorkspaceData
            .Skip(1)
            .Subscribe(data =>
            {
                OnWorkspaceDataChanged(data);
                Console.WriteLine("Workspace data updated in CryptographyExtractorViewModel");
            })
            .AddTo(disposables);
        switch (_saveService.CurrentWorkspaceData)
        {
            case CryptographyWorkspaceData cryptographyWorkspaceData:
                SetUpWorkspace(cryptographyWorkspaceData);
                break;
            default:
                var newWorkspaceData = new CryptographyWorkspaceData();
                SetUpWorkspace(newWorkspaceData);
                break;
        }
    }

    private async ValueTask OnRunWizard()
    { 
        var result = await _dialogService.ShowWizardDialog(ExtractorType.Cryptography);
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
            _wizardService.StartWizard(ExtractorType.Cryptography, result, out workspaceData);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Wizard Error", $"An error occurred while starting the wizard:\n{ex.Message}");
            Console.WriteLine("Error starting wizard.");
            return;
        } 
        SetUpWorkspace((workspaceData as CryptographyWorkspaceData)!);
    }
    
    private async ValueTask OnOpenWorkSpace()
    { 
        Console.WriteLine("Opening workspace...");
        var workspaceData = await _saveService.OpenWorkspaceAsync<CryptographyWorkspaceData>();
        if (workspaceData == null)
        {
            Console.WriteLine("No workspace data loaded.");
            return;
        }
        SetUpWorkspace(workspaceData);
    }

    private void SetUpWorkspace(CryptographyWorkspaceData workspaceData)
    {
        _saveService.CurrentWorkspaceData = workspaceData;
        _model.WorkspaceData.Value = workspaceData;
        WorkspaceTab.Root.Path.Value = workspaceData.RootWorkspacePath;
        WorkspaceTab.Children.Clear();
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Decryption Input", workspaceData.DecryptionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Decryption Output", workspaceData.DecryptionOutputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Encryption Input", workspaceData.EncryptionInputPath));
        WorkspaceTab.Children.Add(new WorkspacePathHandler("Encryption Output", workspaceData.EncryptionOutputPath));
    }
    
    private void OnWorkspaceDataChanged(CryptographyWorkspaceData data)
    {
        _saveService.CurrentWorkspaceData = data;
    }
}