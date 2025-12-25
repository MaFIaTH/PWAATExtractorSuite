using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MessagePack;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;

namespace PWAATExtractorSuite;

public interface ISaveService
{
    IWorkspaceData? CurrentWorkspaceData { get; set; }
    string? CurrentWorkspacePath { get; set; }
    bool IsDirty { get; }
    IDialogService DialogService { get; }
    Task<IWorkspaceData?> TryLoadWorkspaceDataAsync(string filePath, Type? expectedType = null);
    Task<bool> TrySaveWorkspaceDataAsync(string filePath, IWorkspaceData workspaceData);
    event Action WorkspaceChanged;
}

public class SaveService : ISaveService
{
    public IDialogService DialogService => _dialogService;

    public IWorkspaceData? CurrentWorkspaceData
    {
        get;
        set
        {
            IsDirty = _lastSavedWorkspaceData == null || 
                      !_lastSavedWorkspaceData.CompareMemberwise(value) || 
                      _lastSavedWorkspacePath == null ||
                      !_lastSavedWorkspacePath.Equals(CurrentWorkspacePath);
            field = value;
            WorkspaceChanged.Invoke();
        }
    }

    public string? CurrentWorkspacePath
    {
        get;
        set
        {
            field = value;
            WorkspaceChanged.Invoke();
        }
    }

    public bool IsDirty { get; private set; }

    public event Action WorkspaceChanged = () => { };
    
    private readonly IDialogService _dialogService;
    private readonly AppSettings _appSettings;
    private IWorkspaceData? _lastSavedWorkspaceData;
    private string? _lastSavedWorkspacePath;

    public SaveService(IDialogService dialogService, AppSettings appSettings)
    {
        _dialogService = dialogService;
        _appSettings = appSettings;
    }

    public async Task<IWorkspaceData?> TryLoadWorkspaceDataAsync(string filePath, Type? expectedType = null)
    {
        if (!File.Exists(filePath))
        {
            await _dialogService.ShowNotificationDialog("File not found", $"""The file at path "{filePath}" does not exist.""");
            return null;
        }
        try
        {
            var stream = File.OpenRead(filePath);
            var workspaceData = await MessagePackSerializer.DeserializeAsync<IWorkspaceData>(stream);
            if (expectedType != null)
            {
                if (workspaceData.GetType() != expectedType)
                {
                    await _dialogService.ShowNotificationDialog("Error loading file",
                        $"The loaded workspace data is of incorrect type.\n" +
                        $"Expected: {expectedType.Name} Got: {workspaceData?.GetType().Name ?? "null"}");
                    stream.Close();
                    await stream.DisposeAsync();
                    return null;
                }
            }
            stream.Close();
            await stream.DisposeAsync();
            await workspaceData.Load();
            _lastSavedWorkspaceData = workspaceData.Copy();
            _lastSavedWorkspacePath = filePath;
            CurrentWorkspaceData = workspaceData;
            CurrentWorkspacePath = filePath;
            _appSettings.Data.LastOpenedWorkspacePath = filePath;
            await _appSettings.Save();
            return workspaceData;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Error loading file", $"""An error occurred while loading the file: {ex.Message}""");
            return null;
        }
    }
    
    public async Task<bool> TrySaveWorkspaceDataAsync(string filePath, IWorkspaceData workspaceData)
    {
        try
        {
            var stream = File.OpenWrite(filePath);
            await MessagePackSerializer.SerializeAsync(stream, workspaceData);
            await stream.FlushAsync();
            stream.Close();
            await stream.DisposeAsync();
            await workspaceData.Save();
            _lastSavedWorkspaceData = workspaceData.Copy();
            _lastSavedWorkspacePath = filePath;
            CurrentWorkspaceData = workspaceData;
            CurrentWorkspacePath = filePath;
            _appSettings.Data.LastOpenedWorkspacePath = filePath;
            await _appSettings.Save();
            return true;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Error saving file", $"""An error occurred while saving the file: {ex.Message}""");
            return false;
        }
    }
}

public static class SaveServiceExtensions
{
    extension(ISaveService saveService)
    {
        public async Task<IWorkspaceData?> OpenWorkspaceAsync(Type? expectedType = null)
        {
            var filePath = await saveService.DialogService.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Workspace",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Workspace Files")
                    {
                        Patterns = ["*.pwaatws"],
                        MimeTypes = ["application/octet-stream"]
                    }
                ]
            });
            if (filePath.Count == 0)
            {
                Console.WriteLine("Open workspace cancelled.");
                return null;
            }
            var path = filePath[0].Path.LocalPath;
            var workspaceData = await saveService.TryLoadWorkspaceDataAsync(path, expectedType);
            return workspaceData;
        }
        
        public async Task<T?> OpenWorkspaceAsync<T>() where T : class, IWorkspaceData
        {
            var workspaceData = await saveService.OpenWorkspaceAsync(typeof(T));
            switch (workspaceData)
            {
                case null:
                    return null;
                case T typedWorkspaceData:
                    return typedWorkspaceData;
            }
            Console.WriteLine("Loaded workspace data is of incorrect type.");
            return null;
        }

        public async Task<bool> SaveNewWorkspaceAsync()
        {
            if (saveService.CurrentWorkspaceData == null)
            {
                Console.WriteLine("No workspace data to save.");
                return false;
            }
            var fileName = saveService.CurrentWorkspacePath != null ? 
                Path.GetFileNameWithoutExtension(saveService.CurrentWorkspacePath) : 
                "Untitled Workspace";
            var fullFileName = fileName + ".pwaatws";
            var filePickerFileType = new FilePickerFileType("Workspace Files")
            {
                Patterns = ["*.pwaatws"],
                MimeTypes = ["application/octet-stream"]
            };
            var result = await saveService.DialogService.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Workspace",
                SuggestedFileName = fullFileName,
                SuggestedFileType = filePickerFileType,
                DefaultExtension = ".pwaatws",
                FileTypeChoices = 
                [
                    filePickerFileType
                ],
                ShowOverwritePrompt = true
            });
            if (result.File == null)
            {
                Console.WriteLine("Save workspace cancelled.");
                return false;
            }
            var path = result.File.Path.LocalPath;
            var success = await saveService.TrySaveWorkspaceDataAsync(path, saveService.CurrentWorkspaceData);
            return success;
        }
        
        public async Task<bool> SaveCurrentWorkspaceAsync()
        {
            if (saveService.CurrentWorkspaceData == null || string.IsNullOrWhiteSpace(saveService.CurrentWorkspacePath))
            {
                Console.WriteLine("No current workspace data or path to save.");
                return false;
            }
            var success = await saveService.TrySaveWorkspaceDataAsync(saveService.CurrentWorkspacePath, saveService.CurrentWorkspaceData);
            return success;
        }
    }
}
