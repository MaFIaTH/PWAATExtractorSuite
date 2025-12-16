using Avalonia.Platform.Storage;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Shared;

namespace PWAATExtractorSuite.ViewModels.Binary;

public class BinaryWorkspaceTabViewModel : WorkspaceTabViewModel
{
    public override IWorkspaceData WorkspaceData => new BinaryWorkspaceData
    {
        RootWorkspacePath = Root.Path.Value,
        ExtractionInputPath = Children[0].Path.Value,
        ExtractionOutputPath = Children[1].Path.Value,
        InsertionInputPath = Children[2].Path.Value,
        InsertionOutputPath = Children[3].Path.Value
    };
    
    public BinaryWorkspaceTabViewModel()
    {
        
    }

    public BinaryWorkspaceTabViewModel(IDialogService dialogService, ILauncher launcher) 
        : base(dialogService, launcher) { }
}