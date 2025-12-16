using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PWAATExtractorSuite.ViewModels;
using R3.Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace PWAATExtractorSuite.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private readonly AvaloniaRenderingFrameProvider _frameProvider;
    private readonly MainViewModel _mainViewModel;
    public MainWindow()
    {
        Console.WriteLine("MainWindow initialized");
        InitializeComponent();
        var topLevel = GetTopLevel(this);
        _frameProvider = new AvaloniaRenderingFrameProvider(topLevel!);
        this.WhenActivated(Activate);
        // initialize RenderingFrameProvider
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        _mainViewModel = viewModel;
    }
    
    private void Activate(Action<IDisposable> obj)
    {
        Console.WriteLine("MainWindow Activated");
        DataContext = _mainViewModel;
        ViewModel = _mainViewModel;
        obj(Disposable.Create(() => Console.WriteLine("MainWindow Deactivated")));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Console.WriteLine("MainWindow OnLoaded");
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _frameProvider.Dispose();
    }
}