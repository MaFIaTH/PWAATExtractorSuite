using ObservableCollections;
using PWAATExtractorSuite.ViewModels.Shared;

namespace PWAATExtractorSuite.Models;

public class ScenarioExtractorModel
{
    public readonly R3.ReactiveProperty<ScenarioWorkspaceData> WorkspaceData = new(new());
}