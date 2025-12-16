using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ObservableCollections;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Shared;

public abstract class WorkspaceTabViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    public WorkspacePathHandler Root { get; } = new("Root", string.Empty);
    public ObservableList<WorkspacePathHandler> Children { get; } =
    [
        new("Path 1", @"C:\Path\To\Folder1"),
        new("Path 2", @"C:\Path\To\Folder2"),
        new("Path 3", @"C:\Path\To\Folder3"),
        new("Path 4", @"C:\Path\To\Folder4")
    ];
    
    public abstract IWorkspaceData WorkspaceData { get; }
    public event Action<IWorkspaceData> WorkspaceDataChanged;

    public NotifyCollectionChangedSynchronizedViewList<WorkspacePathHandler> ChildrenView { get; }
    
    public ReactiveCommand BrowseRootPathCommand { get; } = new();
    public ReactiveCommand OpenRootPathCommand { get; } = new();
    public ReactiveCommand RootTextBoxLostFocusCommand { get; } = new();
    public ReactiveCommand<WorkspacePathHandler> BrowseChildPathCommand { get; } = new();
    public ReactiveCommand<WorkspacePathHandler> OpenChildPathCommand { get; } = new();
    public ReactiveCommand<WorkspacePathHandler> ChildTextBoxLostFocusCommand { get; } = new();
    
    private readonly IDialogService _dialogService;
    private readonly ILauncher _launcher;
    
    public WorkspaceTabViewModel()
    {
        ChildrenView = Children.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
    }

    public WorkspaceTabViewModel(
        IDialogService dialogService,
        ILauncher launcher) 
        : this()
    {
        _dialogService = dialogService;
        _launcher = launcher;
        Children.Clear();
        Children.Add(new WorkspacePathHandler("Extraction Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Extraction Output", string.Empty));
        Children.Add(new WorkspacePathHandler("Insertion Input", string.Empty));
        Children.Add(new WorkspacePathHandler("Insertion Output", string.Empty));
        this.WhenActivated(BindWhenSelfActivate);
    }

    private void BindWhenSelfActivate(CompositeDisposable disposables)
    {
        Root.PreviousPath = Root.Path.Value;
        foreach (var child in Children)
        {
            child.PreviousPath = child.Path.Value;
        }
    }

    public void BindWhenParentActivate(CompositeDisposable disposables)
    {
        BrowseRootPathCommand
            .SubscribeAwait((_, _) => GetFolderPath(Root), AwaitOperation.Drop)
            .AddTo(disposables);
        BrowseChildPathCommand
            .SubscribeAwait((handler, _) => GetFolderPath(handler), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenRootPathCommand
            .SubscribeAwait((_, _) => OpenPath(Root), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenChildPathCommand
            .SubscribeAwait((handler, _) => OpenPath(handler), AwaitOperation.Drop)
            .AddTo(disposables);
        RootTextBoxLostFocusCommand
            .SubscribeAwait((_, _) => OnTextBoxLostFocus(Root), AwaitOperation.Drop)
            .AddTo(disposables);
        ChildTextBoxLostFocusCommand
            .SubscribeAwait((handler, _) => OnTextBoxLostFocus(handler), AwaitOperation.Drop)
            .AddTo(disposables);
        Disposable.Create(() =>
        {
            Console.WriteLine("Disposing WorkspaceTabViewModel");
            ChildrenView.Dispose();
        }).AddTo(disposables);
    }
    
    private async ValueTask GetFolderPath(WorkspacePathHandler handler)
    {
        var result = await _dialogService.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Root Workspace Path",
            AllowMultiple = false
        });
        if (result.Count == 0)
        {
            Console.WriteLine("Folder picker cancelled");
            return;
        }
        var path = result[0].Path.LocalPath;
        if (!Directory.Exists(path))
        {
            await _dialogService.ShowNotificationDialog("Path Not Found", $"""The selected path "{path}" does not exist.""");
            Console.WriteLine($"Path does not exist: {path}");
            return;
        }
        handler.Path.Value = path;
        Console.WriteLine($"{handler.Name.Value} path set to: {path}");
        handler.PreviousPath = path;
        WorkspaceDataChanged.Invoke(WorkspaceData);
    }

    private async ValueTask OpenPath(WorkspacePathHandler handler)
    {
        var path = handler.Path.Value;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            await _dialogService.ShowNotificationDialog("Path Not Found",
                $"""The selected path "{path}" does not exist.""");
            Console.WriteLine($"Path does not exist: {path}");
            return;
        }
        var directoryInfo = new DirectoryInfo(path);
        var success = await _launcher.LaunchDirectoryInfoAsync(directoryInfo);
        if (!success)
        {
            await _dialogService.ShowNotificationDialog("Error", $"Failed to open path: {path}");
            Console.WriteLine($"Failed to open path: {path}");
        }
    }

    private async ValueTask OnTextBoxLostFocus(WorkspacePathHandler handler)
    {
        var path = handler.Path.Value;
        if (handler.PreviousPath.Equals(path)) return;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            await _dialogService.ShowNotificationDialog("Path Not Found",
                $"""The selected path "{path}" does not exist. Resetting to previous value.""");
            Console.WriteLine($"Path does not exist: {path}");
            handler.Path.Value = handler.PreviousPath;
            return;
        }
        handler.PreviousPath = path;
        WorkspaceDataChanged.Invoke(WorkspaceData);
    }
}

public class WorkspacePathHandler
{
    public BindableReactiveProperty<string> Name { get; } = new(string.Empty);
    public BindableReactiveProperty<string> Path { get; } = new(string.Empty);
    
    public string PreviousPath { get; set; }
    
    public WorkspacePathHandler(string name = "", string path = "")
    {
        Name.Value = name;
        Path.Value = path;
        PreviousPath = path;
    }
}

public class DesignerWorkspaceTabViewModel : WorkspaceTabViewModel
{
    public override IWorkspaceData WorkspaceData => null!;
}