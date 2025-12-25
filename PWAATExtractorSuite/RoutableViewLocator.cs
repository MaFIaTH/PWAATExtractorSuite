using System;
using PWAATExtractorSuite.ViewModels;
using PWAATExtractorSuite.ViewModels.Binary;
using PWAATExtractorSuite.ViewModels.Cryptography;
using PWAATExtractorSuite.ViewModels.Scenario;
using PWAATExtractorSuite.Views.Binary;
using PWAATExtractorSuite.Views.Cryptography;
using PWAATExtractorSuite.Views.Scenario;
using ReactiveUI;
using IViewLocator = ReactiveUI.IViewLocator;

namespace PWAATExtractorSuite;

public class RoutableViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        Console.WriteLine($"Resolving view for {viewModel?.GetType().Name}");
        switch (viewModel)
        {
            case OnBoardingViewModel context:
                return new Views.OnBoardingView
                {
                    ViewModel = context,
                    DataContext = context,
                };
            case SettingsViewModel context:
                return new Views.SettingsView
                {
                    ViewModel = context,
                    DataContext = context,
                };
            case BinaryExtractorViewModel context:
                return new BinaryExtractorView
                {
                    ViewModel = context,
                    DataContext = context,
                };
            case ScenarioExtractorViewModel context:
                return new ScenarioExtractorView
                {
                    ViewModel = context,
                    DataContext = context,
                };
            case CryptographyExtractorViewModel context:
                return new CryptographyExtractorView
                {
                    ViewModel = context,
                    DataContext = context,
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(viewModel));
        }
    }
}