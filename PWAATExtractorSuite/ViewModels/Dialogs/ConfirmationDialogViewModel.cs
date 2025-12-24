using R3;
using ReactiveUI;

namespace PWAATExtractorSuite.ViewModels.Dialogs;

public enum ConfirmationDialogResult
{
    Yes,
    No,
    Cancel
}

public class ConfirmationDialogViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public BindableReactiveProperty<string> Title { get; } = new("Default Title");
    public BindableReactiveProperty<string> Message { get; } = new("Default Message");
    public BindableReactiveProperty<string> YesButtonText { get; } = new("Yes");
    public BindableReactiveProperty<string> NoButtonText { get; } = new("No");
    public BindableReactiveProperty<bool> HasCancel { get; } = new(false);
    public BindableReactiveProperty<string> CancelButtonText { get; } = new("Cancel");
}