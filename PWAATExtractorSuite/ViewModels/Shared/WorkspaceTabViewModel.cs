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
    public WorkspacePathHandler Root { get; } = new("Root", string.Empty)
    {
        IsRoot = true
    };
    public ObservableList<WorkspacePathHandler> Children { get; } =
    [
        new("Path 1", @"C:\Path\To\Folder1"),
        new("Path 2", @"C:\Path\To\Folder2"),
        new("Path 3", @"C:\Path\To\Folder3"),
        new("Path 4", @"C:\Path\To\Folder4")
    ];

    public NotifyCollectionChangedSynchronizedViewList<WorkspacePathHandler> ChildrenView { get; private set; }
    
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
        this.WhenActivated(BindWhenSelfActivate);
    }

    protected void BindWhenSelfActivate(CompositeDisposable disposables)
    {
        Root.PreviousPath = Root.Path.Value;
        foreach (var child in Children)
        {
            child.PreviousPath = child.Path.Value;
        }
    }

    public void BindWhenParentActivate(CompositeDisposable disposables)
    {
        ChildrenView = Children.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        BrowseRootPathCommand
            .SubscribeAwait((_, _) => GetFolderPath(Root), AwaitOperation.Drop)
            .AddTo(disposables);
        BrowseChildPathCommand
            .SubscribeAwait((handler, _) => GetFolderPath(handler), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenRootPathCommand
            .SubscribeAwait((_, _) => OpenDirectory(Root), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenChildPathCommand
            .SubscribeAwait((handler, _) => OpenDirectory(handler), AwaitOperation.Drop)
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
        await handler.GetDirectoryPath(_dialogService);
        ApplyToWorkspaceData(handler);
    }

    private async ValueTask OpenDirectory(WorkspacePathHandler handler)
    {
        await handler.OpenDirectory(_dialogService, _launcher);
    }

    private async ValueTask OnTextBoxLostFocus(WorkspacePathHandler handler)
    {
        await handler.OnDirectoryTextBoxLostFocus(_dialogService);
        ApplyToWorkspaceData(handler);
    }

    protected abstract void ApplyToWorkspaceData(WorkspacePathHandler handler);
}

public class WorkspacePathHandler
{
    public BindableReactiveProperty<string> Name { get; } = new(string.Empty);
    public BindableReactiveProperty<string> Path { get; } = new(string.Empty);
    public bool IsRoot { get; set; }
    public string PreviousPath { get; set; }
    
    public WorkspacePathHandler(string name = "", string path = "")
    {
        Name.Value = name;
        Path.Value = path;
        PreviousPath = path;
    }
}

public static class WorkspacePathUtils
{
    extension(WorkspacePathHandler handler)
    {
        public async Task GetDirectoryPath(IDialogService dialogService)
        {
            var result = await dialogService.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select Directory",
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
                await dialogService.ShowNotificationDialog("Path Not Found", $"""The selected path "{path}" does not exist.""");
                Console.WriteLine($"Path does not exist: {path}");
                return;
            }
            handler.Path.Value = path;
            Console.WriteLine($"{handler.Name.Value} path set to: {path}");
            handler.PreviousPath = path;
        }
        
        public async Task GetFilePath(IDialogService dialogService, FilePickerFileType[]? filter = null)
        {
            var typeFilter = filter ??
            [
                new FilePickerFileType("All Files")
                    {
                        Patterns = ["*"],
                        MimeTypes = ["*/*"]
                    }
            ];
            var result = await dialogService.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select File",
                AllowMultiple = false,
                FileTypeFilter = typeFilter
            });
            if (result.Count == 0)
            {
                Console.WriteLine("File picker cancelled");
                return;
            }
            var path = result[0].Path.LocalPath;
            if (!File.Exists(path))
            {
                await dialogService.ShowNotificationDialog("File Not Found", $"""The selected file "{path}" does not exist.""");
                Console.WriteLine($"File does not exist: {path}");
                return;
            }
            handler.Path.Value = path;
            Console.WriteLine($"{handler.Name.Value} file set to: {path}");
            handler.PreviousPath = path;
        }

        public async Task OpenDirectory(IDialogService dialogService, ILauncher launcher)
        {
            var path = handler.Path.Value;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                await dialogService.ShowNotificationDialog("Path Not Found",
                    $"""The selected path "{path}" does not exist.""");
                Console.WriteLine($"Path does not exist: {path}");
                return;
            }
            var directoryInfo = new DirectoryInfo(path);
            var success = await launcher.LaunchDirectoryInfoAsync(directoryInfo);
            if (!success)
            {
                await dialogService.ShowNotificationDialog("Error", $"Failed to open path: {path}");
                Console.WriteLine($"Failed to open path: {path}");
            }
        }

        public async Task OpenFile(IDialogService dialogService, ILauncher launcher)
        {
            var path = handler.Path.Value;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                await dialogService.ShowNotificationDialog("File Not Found",
                    $"""The selected file "{path}" does not exist.""");
                Console.WriteLine($"File does not exist: {path}");
                return;
            }
            var fileInfo = new FileInfo(path);
            var success = await launcher.LaunchFileInfoAsync(fileInfo);
            if (!success)
            {
                await dialogService.ShowNotificationDialog("Error", $"Failed to open file: {path}");
                Console.WriteLine($"Failed to open file: {path}");
            }
        }

        public async Task OnDirectoryTextBoxLostFocus(IDialogService dialogService)
        {
            var path = handler.Path.Value;
            if (handler.PreviousPath.Equals(path)) return;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                await dialogService.ShowNotificationDialog("Path Not Found",
                    $"""The selected path "{path}" does not exist. Resetting to previous value.""");
                Console.WriteLine($"Path does not exist: {path}");
                handler.Path.Value = handler.PreviousPath;
                return;
            }
            handler.PreviousPath = path;
        }
        
        public async Task OnFileTextBoxLostFocus(IDialogService dialogService)
        {
            var path = handler.Path.Value;
            if (handler.PreviousPath.Equals(path)) return;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                await dialogService.ShowNotificationDialog("File Not Found",
                    $"""The selected file "{path}" does not exist. Resetting to previous value.""");
                Console.WriteLine($"File does not exist: {path}");
                handler.Path.Value = handler.PreviousPath;
                return;
            }
            handler.PreviousPath = path;
        }
    }
}

public class DesignerWorkspaceTabViewModel : WorkspaceTabViewModel
{
    protected override void ApplyToWorkspaceData(WorkspacePathHandler handler)
    {
        throw new NotImplementedException();
    }
}