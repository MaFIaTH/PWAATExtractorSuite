using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PWAATExtractorSuite.Models;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Dialogs;

public class WizardDialogViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public BindableReactiveProperty<ExtractorType> ExtractorType { get; } = new(Models.ExtractorType.Binary);
    public BindableReactiveProperty<string> RootWorkspacePath { get; } = new(string.Empty);
    public ReactiveCommand BrowseRootWorkspacePathCommand { get; } = new();
    
    private readonly IDialogService _dialogService;
    
    //For design time
    public WizardDialogViewModel(){}
    
    public WizardDialogViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        this.WhenActivated(Bind);
    }

    private void Bind(CompositeDisposable disposable)
    {
        BrowseRootWorkspacePathCommand
            .SubscribeAwait((_, _) => OnBrowseRootWorkspacePath(), AwaitOperation.Drop)
            .AddTo(disposable);
    }
    
    private async ValueTask OnBrowseRootWorkspacePath()
    {
        var folderPickerOptions = new FolderPickerOpenOptions
        {
            Title = "Select Root Workspace Folder",
            AllowMultiple = false
        };
        var folderPickerResult = await _dialogService.OpenFolderPickerAsync(folderPickerOptions);
        if (folderPickerResult.Count > 0)
        {
            RootWorkspacePath.Value = folderPickerResult[0].Path.LocalPath;
        }
    }
}