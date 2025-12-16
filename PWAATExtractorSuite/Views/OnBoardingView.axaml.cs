using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views;

public partial class OnBoardingView : ReactiveUserControl<OnBoardingViewModel>
{
    public OnBoardingView()
    {
        InitializeComponent();
        this.WhenActivated(Activate);
    }

    private void Activate(CompositeDisposable obj)
    {
        Console.WriteLine("OnBoardingView Activated");
    }
}