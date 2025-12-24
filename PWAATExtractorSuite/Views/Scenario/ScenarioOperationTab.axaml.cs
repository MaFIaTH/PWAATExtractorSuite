using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels.Scenario;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Scenario;

public partial class ScenarioOperationTab : ReactiveUserControl<ScenarioOperationTabViewModel>
{
    public ScenarioOperationTab()
    {
        InitializeComponent();
    }
}