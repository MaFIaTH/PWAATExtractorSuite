using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;
using PWAAT_scenario_extractor.Simplifier;
using PWAATExtractorSuite.ViewModels.Scenario;

namespace PWAATExtractorSuite.Models;

[Union(0, typeof(BinaryWorkspaceData))]
[Union(1, typeof(ScenarioWorkspaceData))]
[Union(2, typeof(CryptographyWorkspaceData))]
public interface IWorkspaceData
{
    ExtractorType Type { get; }
    string RootWorkspacePath { get; set; }
    bool IsValid();
    IWorkspaceData Copy();
    bool CompareMemberwise(IWorkspaceData? other);
    Task Save();
    Task Load();
}

[MessagePackObject(keyAsPropertyName: true)]
public record BinaryWorkspaceData : IWorkspaceData
{
    public ExtractorType Type => ExtractorType.Binary;
    public string RootWorkspacePath { get; set; }
    public string ExtractionInputPath { get; set; }
    public string ExtractionOutputPath { get; set; }
    public string InsertionInputPath { get; set; }
    public string InsertionOutputPath { get; set; }
    
    public bool IsValid()
    {
        return Directory.Exists(RootWorkspacePath) &&
               Directory.Exists(ExtractionInputPath) &&
               Directory.Exists(ExtractionOutputPath) &&
               Directory.Exists(InsertionInputPath) &&
               Directory.Exists(InsertionOutputPath);
    }
    
    public IWorkspaceData Copy()
    {
        return this with { };
    }

    public bool CompareMemberwise(IWorkspaceData? other)
    {
        if (other is not BinaryWorkspaceData otherBinary) return false;
        return Type.Equals(other.Type) &&
               WorkspaceDataUtils.IsPathEqual(RootWorkspacePath, otherBinary.RootWorkspacePath) &&
               WorkspaceDataUtils.IsPathEqual(ExtractionInputPath, otherBinary.ExtractionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(ExtractionOutputPath, otherBinary.ExtractionOutputPath) &&
               WorkspaceDataUtils.IsPathEqual(InsertionInputPath, otherBinary.InsertionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(InsertionOutputPath, otherBinary.InsertionOutputPath);
    }
    
    public Task Save() => Task.CompletedTask;
    public Task Load() => Task.CompletedTask;
}

[MessagePackObject(keyAsPropertyName: true)]
public record ScenarioWorkspaceData : IWorkspaceData
{
    public ExtractorType Type => ExtractorType.Scenario;
    public string RootWorkspacePath { get; set; }
    public string ExtractionInputPath { get; set; }
    public string ExtractionOutputPath { get; set; }
    public string InsertionInputPath { get; set; }
    public string InsertionOutputPath { get; set; }
    public string SpeakerDefinitionPath { get; set; }
    public string SimplificationInputPath { get; set; }
    public string SimplificationOutputPath { get; set; }
    public string DesimplificationOriginalPath { get; set; }
    public string DesimplificationInputPath { get; set; }
    public string DesimplificationOutputPath { get; set; }
    
    [IgnoreMember]
    public List<SpeakerDefinitionHandler> SpeakerDefinitions = new();
    
    public SpeakerIdData GetSpeakerIdData()
    {
        return new SpeakerIdData
        {
            Speakers = SpeakerDefinitions
                .Select(sd => sd.ToSpeakerId())
                .OrderBy(x => x.Id)
                .ToList()
        };
    }

    public bool IsValid()
    {
        return Directory.Exists(RootWorkspacePath) &&
               Directory.Exists(ExtractionInputPath) &&
               Directory.Exists(ExtractionOutputPath) &&
               Directory.Exists(InsertionInputPath) &&
               Directory.Exists(InsertionOutputPath) &&
               Directory.Exists(SimplificationInputPath) &&
               Directory.Exists(SimplificationOutputPath) &&
               Directory.Exists(DesimplificationOriginalPath) &&
               Directory.Exists(DesimplificationInputPath) &&
               Directory.Exists(DesimplificationOutputPath);
    }
    
    public IWorkspaceData Copy()
    {
        return this with
        {
            SpeakerDefinitions = SpeakerDefinitions.Select(sd => sd.Copy()).ToList()
        };
    }

    public bool CompareMemberwise(IWorkspaceData? other)
    {
        if (other is not ScenarioWorkspaceData otherScenario) return false;
        if (!SpeakerDefinitions.Count.Equals(otherScenario.SpeakerDefinitions.Count)) return false;
        var speakerDefinitionEqual = true;
        var sortedThis = SpeakerDefinitions.OrderBy(sd => sd.Id.Value).ToList();
        var sortedOther = otherScenario.SpeakerDefinitions.OrderBy(sd => sd.Id.Value).ToList();
        for (var i = 0; i < sortedThis.Count; i++)
        {
            if (sortedThis[i].CompareMemberwise(sortedOther[i])) continue;
            speakerDefinitionEqual = false;
            break;
        }
        return Type.Equals(otherScenario.Type) && 
               speakerDefinitionEqual &&
               WorkspaceDataUtils.IsPathEqual(RootWorkspacePath, otherScenario.RootWorkspacePath) &&
               WorkspaceDataUtils.IsPathEqual(ExtractionInputPath, otherScenario.ExtractionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(ExtractionOutputPath, otherScenario.ExtractionOutputPath) &&
               WorkspaceDataUtils.IsPathEqual(InsertionInputPath, otherScenario.InsertionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(InsertionOutputPath, otherScenario.InsertionOutputPath) &&
               WorkspaceDataUtils.IsPathEqual(SpeakerDefinitionPath, otherScenario.SpeakerDefinitionPath) &&
               WorkspaceDataUtils.IsPathEqual(SimplificationInputPath, otherScenario.SimplificationInputPath) &&
               WorkspaceDataUtils.IsPathEqual(SimplificationOutputPath, otherScenario.SimplificationOutputPath) &&
               WorkspaceDataUtils.IsPathEqual(DesimplificationOriginalPath, otherScenario.DesimplificationOriginalPath) &&
               WorkspaceDataUtils.IsPathEqual(DesimplificationInputPath, otherScenario.DesimplificationInputPath) &&
               WorkspaceDataUtils.IsPathEqual(DesimplificationOutputPath, otherScenario.DesimplificationOutputPath);
    }
    
    public async Task Save()
    { 
        if (string.IsNullOrEmpty(SpeakerDefinitionPath))
        {
            return;
        }
        var speakerIdData = GetSpeakerIdData();
        try
        {
            var json = JsonConvert.SerializeObject(speakerIdData, Formatting.Indented);
            await File.WriteAllTextAsync(SpeakerDefinitionPath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving speaker definitions: {e.Message}");
        }
    }
    public async Task Load()
    { 
        if (string.IsNullOrEmpty(SpeakerDefinitionPath) || !File.Exists(SpeakerDefinitionPath))
        {
            return;
        }
        SpeakerIdData? speakerIdData;
        try
        {
            speakerIdData = JsonConvert.DeserializeObject<SpeakerIdData>(await File.ReadAllTextAsync(SpeakerDefinitionPath));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading speaker definitions: {e.Message}");
            return;
        }
        if (speakerIdData == null)
        {
            return;
        }
        SpeakerDefinitions.Clear();
        foreach (var speaker in speakerIdData.Speakers.OrderBy(x => x.Id))
        {
            var handler = new SpeakerDefinitionHandler(speaker);
            SpeakerDefinitions.Add(handler);
        }
    }
}

[MessagePackObject(keyAsPropertyName: true)]
public record CryptographyWorkspaceData : IWorkspaceData
{
    public ExtractorType Type => ExtractorType.Cryptography;
    public string RootWorkspacePath { get; set; }
    public string DecryptionInputPath { get; set; }
    public string DecryptionOutputPath { get; set; }
    public string EncryptionInputPath { get; set; }
    public string EncryptionOutputPath { get; set; }
    
    public bool IsValid()
    {
        return Directory.Exists(RootWorkspacePath) &&
               Directory.Exists(DecryptionInputPath) &&
               Directory.Exists(DecryptionOutputPath) &&
               Directory.Exists(EncryptionInputPath) &&
               Directory.Exists(EncryptionOutputPath);
    }
    
    public IWorkspaceData Copy()
    {
        return this with { };
    }

    public bool CompareMemberwise(IWorkspaceData? other)
    {
        if (other is not CryptographyWorkspaceData otherCrypto) return false;
        return Type.Equals(other.Type) &&
               WorkspaceDataUtils.IsPathEqual(RootWorkspacePath, otherCrypto.RootWorkspacePath) &&
               WorkspaceDataUtils.IsPathEqual(DecryptionInputPath, otherCrypto.DecryptionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(DecryptionOutputPath, otherCrypto.DecryptionOutputPath) &&
               WorkspaceDataUtils.IsPathEqual(EncryptionInputPath, otherCrypto.EncryptionInputPath) &&
               WorkspaceDataUtils.IsPathEqual(EncryptionOutputPath, otherCrypto.EncryptionOutputPath);
    }
    
    public Task Save() => Task.CompletedTask;
    public Task Load() => Task.CompletedTask;
}

public static class WorkspaceDataUtils
{
    public static bool IsPathEqual(string path1, string path2)
    {
        if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
        {
            return string.Equals(path1, path2);
        }
        return string.Equals(
            Path.GetFullPath(Path.TrimEndingDirectorySeparator(path1)),
            Path.GetFullPath(Path.TrimEndingDirectorySeparator(path2)),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
