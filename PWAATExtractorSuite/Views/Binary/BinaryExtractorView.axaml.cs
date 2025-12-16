using PWAATExtractorSuite.ViewModels;
using PWAATExtractorSuite.ViewModels.Binary;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Binary;

public partial class BinaryExtractorView : ReactiveUserControl<BinaryExtractorViewModel>
{
    public BinaryExtractorView()
    {
        InitializeComponent();
    }
}