using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels.Dialogs;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Dialogs;

public partial class WizardDialogView : ReactiveUserControl<WizardDialogViewModel>
{
    public WizardDialogView()
    {
        InitializeComponent();
    }
}