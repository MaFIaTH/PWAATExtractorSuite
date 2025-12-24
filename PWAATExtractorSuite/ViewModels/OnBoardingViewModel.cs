using System;
using Microsoft.Extensions.DependencyInjection;
using PWAATExtractorSuite.Models;
using R3;
using ReactiveUI;
using Splat;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels;

public class OnBoardingViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel
{
    public string? UrlPathSegment => "on-boarding";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }
    public ReactiveCommand<ViewModelType> ToExtractorCommand { get; } = new();

    public OnBoardingViewModel()
    {
        Activator = new ViewModelActivator();
        this.WhenActivated(Bind);
    }
    
    public OnBoardingViewModel(
        [FromKeyedServices(ViewModelType.MainRouter)] IScreen screen) 
        : this()
    {
        HostScreen = screen;
    }

    private void Bind(CompositeDisposable compositeDisposable)
    {
        Console.WriteLine("Bind OnBoardingViewModel");
        ToExtractorCommand
            .Subscribe(NavigateToExtractor)
            .AddTo(compositeDisposable);
    }
    
    private void NavigateToExtractor(ViewModelType extractorType)
    {
        Console.WriteLine("Navigating to binary extractor");
        Console.WriteLine($"HostScreen is {HostScreen?.GetType().Name}");
        if (HostScreen is MainRouterViewModel router)
        {
            router.NavigateTo(extractorType);
        }
    }

   
}