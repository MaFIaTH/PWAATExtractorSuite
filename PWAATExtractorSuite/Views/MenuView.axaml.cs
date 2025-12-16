using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views;

public partial class MenuView : ReactiveUserControl<MenuViewModel>
{
    public MenuView()
    {
        InitializeComponent();
    }
}