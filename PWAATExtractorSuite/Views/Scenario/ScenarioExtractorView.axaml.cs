using PWAATExtractorSuite.ViewModels;
using PWAATExtractorSuite.ViewModels.Binary;
using PWAATExtractorSuite.ViewModels.Scenario;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Scenario;

public partial class ScenarioExtractorView : ReactiveUserControl<ScenarioExtractorViewModel>
{
    public ScenarioExtractorView()
    {
        InitializeComponent();
    }
}