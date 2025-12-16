using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PWAATExtractorSuite.ViewModels.Dialogs;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace PWAATExtractorSuite;

public class AppSettings(IDialogService dialogService)
{
    private static readonly string SettingsFileName = "app-settings.json";
    public AppSettingsData Data { get; private set; }
    
    private static string GetSettingsFilePath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var appFolderPath = Path.Combine(appDirectory, "AppData");
        if (!Directory.Exists(appFolderPath))
        {
            Directory.CreateDirectory(appFolderPath);
        }
        return Path.Combine(appFolderPath, SettingsFileName);
    }
    
    public async Task Load()
    {
        var filePath = GetSettingsFilePath();
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Settings file not found, creating new one.");
            Data = new AppSettingsData();
            var json = MessagePackSerializer.SerializeToJson(Data);
            var indentedJson = JToken.Parse(json).ToString(Formatting.Indented);
            await File.WriteAllTextAsync(filePath, indentedJson);
            return;
        }
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var data = MessagePackSerializer.ConvertFromJson(json);
            var settings = MessagePackSerializer.Deserialize<AppSettingsData>(data);
            Data = settings;
        }
        catch (Exception ex)
        {
            await dialogService.ShowNotificationDialog("Error loading settings", $"An error occurred while loading settings: {ex.Message}");
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }
    
    public async Task Save()
    {
        var filePath = GetSettingsFilePath();
        try
        {
            var json = MessagePackSerializer.SerializeToJson(Data);
            var indentedJson = JToken.Parse(json).ToString(Formatting.Indented);
            await File.WriteAllTextAsync(filePath, indentedJson);
        }
        catch (Exception ex)
        {
            await dialogService.ShowNotificationDialog("Error saving settings", $"An error occurred while saving settings: {ex.Message}");
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}

[MessagePackObject(keyAsPropertyName: true)]
public class AppSettingsData
{
    public bool OpenLastWorkspaceOnStartup { get; set; } = true;
    public string LastOpenedWorkspacePath { get; set; } = string.Empty;
}