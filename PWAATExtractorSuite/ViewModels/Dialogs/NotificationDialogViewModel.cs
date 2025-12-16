using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Dialogs;

public class NotificationDialogViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public BindableReactiveProperty<string> Title { get; } = new("Default Title");
    public BindableReactiveProperty<string>  Message { get; } = new("Default Message");
    public ReactiveCommand OkCommand { get; } = new();
    
    public NotificationDialogViewModel()
    {
        this.WhenActivated(Bind);
    }

    private void Bind(CompositeDisposable obj)
    {
        
    }
}