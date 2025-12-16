using System;
using System.Collections.Generic;
using ReactiveUI;

namespace PWAATExtractorSuite.ViewModels;

public class MainRouterViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();
    private readonly MainViewModel _mainViewModel;

    public event Action<ViewModelBase> ReloadEvent;
    
    public MainRouterViewModel(){}
    
    public MainRouterViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        Console.WriteLine("RouterViewModel initialized");
    }
    
    public void NavigateTo(ViewModelType type)
    {
        if (!_mainViewModel.ViewModels.TryGetValue(type, out var viewModel)) return;
        if (viewModel is not IRoutableViewModel routableViewModel)
        {
            Console.WriteLine("Error: ViewModel is not routable");
            return;
        }
        if (Router.GetCurrentViewModel() != routableViewModel)
        {
            Console.WriteLine($"Navigated to {viewModel.GetType()}");
            Router.Navigate.Execute(routableViewModel);
        }
        else
        {
            Console.WriteLine($"Reloading {viewModel.GetType()}");
            ReloadEvent?.Invoke(viewModel);
        }
    }
}