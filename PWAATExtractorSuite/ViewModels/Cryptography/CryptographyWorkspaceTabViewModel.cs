using Avalonia.Platform.Storage;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Shared;

namespace PWAATExtractorSuite.ViewModels.Cryptography;

public class CryptographyWorkspaceTabViewModel : WorkspaceTabViewModel
{
    private readonly CryptographyExtractorModel _model;
    public CryptographyWorkspaceTabViewModel()
    {
        
    }

    public CryptographyWorkspaceTabViewModel(
        CryptographyExtractorModel model,
        IDialogService dialogService, 
        ILauncher launcher)
        : base(dialogService, launcher)
    {
        _model = model;
        Children.Clear();
        Children.Add(new WorkspacePathHandler("Decryption Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Decryption Output", string.Empty));
        Children.Add(new WorkspacePathHandler("Encryption Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Encryption Output", string.Empty));
        //this.WhenActivated(BindWhenSelfActivate);
    }

    protected override void ApplyToWorkspaceData(WorkspacePathHandler handler)
    {
        var index = Children.IndexOf(handler);
        var workspaceData = _model.WorkspaceData.Value;
        var path = handler.Path.Value;
        if (handler.IsRoot)
        {           
            workspaceData.RootWorkspacePath = path;
            _model.WorkspaceData.OnNext(workspaceData);
            return;
        }
        switch (index)
        {
            case 0:
                workspaceData.DecryptionInputPath = path;
                break;
            case 1:
                workspaceData.DecryptionOutputPath = path;
                break;
            case 2:
                workspaceData.EncryptionInputPath = path;
                break;
            case 3:
                workspaceData.EncryptionOutputPath = path;
                break;
        }
        _model.WorkspaceData.OnNext(workspaceData);
    }
}