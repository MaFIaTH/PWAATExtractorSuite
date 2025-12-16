using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Binary;
using R3;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Binary;

public partial class BinaryOperationTab : ReactiveUserControl<BinaryOperationTabViewModel>
{
    public BinaryOperationTab()
    {
        InitializeComponent();
    }
}