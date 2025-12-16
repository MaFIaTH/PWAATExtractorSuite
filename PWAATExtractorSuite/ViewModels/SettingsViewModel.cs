using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels;

public class SettingsViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel
{
    public string? UrlPathSegment => "settings";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();
    
    public BindableReactiveProperty<bool> OpenLastWorkspaceOnStartup { get; } = new(false);
    public ReactiveCommand ApplyCommand { get; } = new();
    public ReactiveCommand SaveCommand { get; } = new();
    public ReactiveCommand BackCommand { get; } = new();
    
    private readonly AppSettings _settings;

    public SettingsViewModel()
    {
        this.WhenActivated(Bind);
    }

    public SettingsViewModel(
        AppSettings appSettings,
        [FromKeyedServices(ViewModelType.MainRouter)] IScreen router) : this()
    {
        _settings = appSettings;
        HostScreen = router;
    }

    private void Bind(CompositeDisposable disposables)
    {
        _ = BindAsync(disposables);
    }
    
    private async Task BindAsync(CompositeDisposable disposables)
    {
        await _settings.Load();
        OpenLastWorkspaceOnStartup.Value = _settings.Data.OpenLastWorkspaceOnStartup;
        ApplyCommand
            .SubscribeAwait(async (_, _) => await OnApplySettings(), AwaitOperation.Drop)
            .AddTo(disposables);
        SaveCommand
            .SubscribeAwait(async (_, _) => await OnSaveSettings(), AwaitOperation.Drop)
            .AddTo(disposables);
        BackCommand
            .Subscribe(_ => OnBack())
            .AddTo(disposables);
    }
    
    private async Task OnApplySettings()
    {
        _settings.Data.OpenLastWorkspaceOnStartup = OpenLastWorkspaceOnStartup.Value;
        await _settings.Save();
    }
    
    private async Task OnSaveSettings()
    {
        await OnApplySettings();
        OnBack();
    }
    
    private void OnBack()
    {
        HostScreen.Router.NavigateBack.Execute();
    }
}