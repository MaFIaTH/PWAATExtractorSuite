using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MessagePack;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;

namespace PWAATExtractorSuite;

public interface ISaveService
{
    IWorkspaceData? CurrentWorkspaceData { get; set; }
    string? CurrentWorkspacePath { get; set; }
    IDialogService DialogService { get; }
    Task<IWorkspaceData?> TryLoadWorkspaceDataAsync(string filePath);
    Task<bool> TrySaveWorkspaceDataAsync(string filePath, IWorkspaceData workspaceData);
}

public class SaveService(IDialogService dialogService, AppSettings appSettings) : ISaveService
{
    public IDialogService DialogService => dialogService;
    public IWorkspaceData? CurrentWorkspaceData { get; set; }
    public string? CurrentWorkspacePath { get; set; }
    
    public async Task<IWorkspaceData?> TryLoadWorkspaceDataAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            await dialogService.ShowNotificationDialog("File not found", $"""The file at path "{filePath}" does not exist.""");
            return null;
        }
        try
        {
            var stream = File.OpenRead(filePath);
            var workspaceData = await MessagePackSerializer.DeserializeAsync<IWorkspaceData>(stream);
            var json = MessagePackSerializer.SerializeToJson(workspaceData);
            Console.WriteLine($"Loaded workspace data from {filePath}:\n{json}");
            stream.Close();
            await stream.DisposeAsync();
            CurrentWorkspaceData = workspaceData;
            CurrentWorkspacePath = filePath;
            appSettings.Data.LastOpenedWorkspacePath = filePath;
            await appSettings.Save();
            return workspaceData;
        }
        catch (Exception ex)
        {
            await dialogService.ShowNotificationDialog("Error loading file", $"""An error occurred while loading the file: {ex.Message}""");
            return null;
        }
    }
    
    public async Task<bool> TrySaveWorkspaceDataAsync(string filePath, IWorkspaceData workspaceData)
    {
        try
        {
            var stream = File.OpenWrite(filePath);
            await MessagePackSerializer.SerializeAsync(stream, workspaceData);
            var json = MessagePackSerializer.SerializeToJson(workspaceData);
            Console.WriteLine($"Saved workspace data to {filePath}:\n{json}");
            await stream.FlushAsync();
            stream.Close();
            await stream.DisposeAsync();
            CurrentWorkspaceData = workspaceData;
            CurrentWorkspacePath = filePath;
            appSettings.Data.LastOpenedWorkspacePath = filePath;
            await appSettings.Save();
            return true;
        }
        catch (Exception ex)
        {
            await dialogService.ShowNotificationDialog("Error saving file", $"""An error occurred while saving the file: {ex.Message}""");
            return false;
        }
    }
}

public static class SaveServiceExtensions
{
    extension(ISaveService saveService)
    {
        public async Task<IWorkspaceData?> OpenWorkspaceAsync()
        {
            var filePath = await saveService.DialogService.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Workspace",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Workspace Files")
                    {
                        Patterns = ["*.pwaatws"]
                    }
                ]
            });
            if (filePath.Count == 0)
            {
                Console.WriteLine("Open workspace cancelled.");
                return null;
            }
            var path = filePath[0].Path.LocalPath;
            var workspaceData = await saveService.TryLoadWorkspaceDataAsync(path);
            return workspaceData;
        }
        
        public async Task<T?> OpenWorkspaceAsync<T>() where T : class, IWorkspaceData
        {
            var workspaceData = await saveService.OpenWorkspaceAsync();
            switch (workspaceData)
            {
                case null:
                    return null;
                case T typedWorkspaceData:
                    return typedWorkspaceData;
            }
            await saveService.DialogService.ShowNotificationDialog("Error", 
                "Loaded workspace data is of incorrect type.\n" +
                $"Expected: {typeof(T).Name} Got: {workspaceData.GetType().Name}");
            Console.WriteLine("Loaded workspace data is of incorrect type.");
            return null;
        }

        public async Task<bool> SaveWorkspaceAsync()
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
                Patterns = ["*.pwaatws"]
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
