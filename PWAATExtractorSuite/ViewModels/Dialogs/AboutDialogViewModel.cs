using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Dialogs;

public class AboutDialogViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public BindableReactiveProperty<string> VersionInfo { get; } = new("1.0.0");
    public BindableReactiveProperty<string> Copyright { get; } = new("© 2024 Translate Kordai");
    public ReactiveCommand<string> OpenWebsiteCommand { get; } = new();
    
    private readonly ILauncher _launcher;
    
    public AboutDialogViewModel()
    {
        this.WhenActivated(Bind);
    }
    
    public AboutDialogViewModel(
        ILauncher launcher) 
        : this()
    {
        _launcher = launcher;
    }

    private void Bind(CompositeDisposable disposables)
    {
        VersionInfo.Value = $"{Application.Current?.Resources["AppVersion"] as string}";
        Copyright.Value = $"Brought to you by:\nMaFIa_TH from Translate Kordai";
        OpenWebsiteCommand
            .SubscribeAwait(async (url, _) => await OpenWebsite(url), AwaitOperation.Drop)
            .AddTo(disposables);
    }
    
    private async ValueTask OpenWebsite(string url)
    {
        await _launcher.LaunchUriAsync(new Uri(url));
    }
}