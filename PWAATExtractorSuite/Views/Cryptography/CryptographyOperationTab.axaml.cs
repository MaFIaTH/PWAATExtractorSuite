using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels.Cryptography;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Cryptography;

public partial class CryptographyOperationTab : ReactiveUserControl<CryptographyOperationTabViewModel>
{
    public CryptographyOperationTab()
    {
        InitializeComponent();
    }
}