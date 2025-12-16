using Avalonia.Controls;
using PWAATExtractorSuite.ViewModels;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }
}