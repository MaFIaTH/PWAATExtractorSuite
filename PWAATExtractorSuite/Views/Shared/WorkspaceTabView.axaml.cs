using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels.Shared;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Shared;

public partial class WorkspaceTabView : ReactiveUserControl<WorkspaceTabViewModel>
{
    public WorkspaceTabView()
    {
        InitializeComponent();
    }
}