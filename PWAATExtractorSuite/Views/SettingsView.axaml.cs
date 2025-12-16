using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}