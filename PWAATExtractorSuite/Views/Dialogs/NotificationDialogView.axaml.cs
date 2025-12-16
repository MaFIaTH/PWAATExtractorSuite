using System;
using PWAATExtractorSuite.ViewModels.Dialogs;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views.Dialogs;

public partial class NotificationDialogView : ReactiveUserControl<NotificationDialogViewModel>
{
    public NotificationDialogView()
    {
        Console.WriteLine("NotificationDialogView Constructor");
        InitializeComponent();
    }
}