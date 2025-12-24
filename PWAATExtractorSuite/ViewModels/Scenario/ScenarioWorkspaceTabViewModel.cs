using Avalonia.Platform.Storage;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Shared;
using ReactiveUI;

namespace PWAATExtractorSuite.ViewModels.Scenario;

public class ScenarioWorkspaceTabViewModel : WorkspaceTabViewModel
{ 
    private readonly ScenarioExtractorModel _model;
    public ScenarioWorkspaceTabViewModel()
    {
        
    }

    public ScenarioWorkspaceTabViewModel(
        ScenarioExtractorModel model,
        IDialogService dialogService, 
        ILauncher launcher)
        : base(dialogService, launcher)
    {
        _model = model;
        Children.Clear();
        Children.Add(new WorkspacePathHandler("Extraction Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Extraction Output", string.Empty));
        Children.Add(new WorkspacePathHandler("Insertion Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Insertion Output", string.Empty));
        Children.Add(new WorkspacePathHandler("Simplification Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Simplification Output", string.Empty));
        Children.Add(new WorkspacePathHandler("Desimplification Original", string.Empty));
        Children.Add(new WorkspacePathHandler("Desimplification Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Desimplification Output", string.Empty));
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
                workspaceData.ExtractionInputPath = path;
                break;
            case 1:
                workspaceData.ExtractionOutputPath = path;
                break;
            case 2:
                workspaceData.InsertionInputPath = path;
                break;
            case 3:
                workspaceData.InsertionOutputPath = path;
                break;
            case 4:
                workspaceData.SimplificationInputPath = path;
                break;
            case 5:
                workspaceData.SimplificationOutputPath = path;
                break;
            case 6:
                workspaceData.DesimplificationOriginalPath = path;
                break;
            case 7:
                workspaceData.DesimplificationInputPath = path;
                break;
            case 8:
                workspaceData.DesimplificationOutputPath = path;
                break;
        }
        _model.WorkspaceData.OnNext(workspaceData);
    }
}