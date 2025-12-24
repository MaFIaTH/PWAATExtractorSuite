using System;
using System.IO;
using Newtonsoft.Json;
using PWAAT_scenario_extractor.Simplifier;
using PWAATExtractorSuite.Models;
using R3;

namespace PWAATExtractorSuite;

public interface IWizardService
{
    public void StartWizard(ExtractorType extractorType, string rootPath, out IWorkspaceData workspaceData);
}
public class WizardService : IWizardService
{
    public void StartWizard(ExtractorType extractorType, string rootPath, out IWorkspaceData workspaceData)
    {
        workspaceData = null;
        switch (extractorType)
        {
            case ExtractorType.Binary:
                RunBinaryWizard(rootPath, out workspaceData);
                break;
            case ExtractorType.Scenario:
                RunScenarioWizard(rootPath, out workspaceData);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(extractorType), extractorType, null);
        }
    }
    
    private void RunBinaryWizard(string rootPath, out IWorkspaceData workspaceData)
    {
        workspaceData = new BinaryWorkspaceData();
        var extractionInputPath = Path.Combine(rootPath, "extraction", "input");
        var extractionOutputPath = Path.Combine(rootPath, "extraction", "output");
        var insertionInputPath = Path.Combine(rootPath, "insertion", "input");
        var insertionOutputPath = Path.Combine(rootPath, "insertion", "output");
        Directory.CreateDirectory(extractionInputPath);
        Directory.CreateDirectory(extractionOutputPath);    
        Directory.CreateDirectory(insertionInputPath);
        Directory.CreateDirectory(insertionOutputPath);
        var binaryWorkspaceData = (BinaryWorkspaceData)workspaceData;
        binaryWorkspaceData.RootWorkspacePath = rootPath;
        binaryWorkspaceData.ExtractionInputPath = extractionInputPath;
        binaryWorkspaceData.ExtractionOutputPath = extractionOutputPath;
        binaryWorkspaceData.InsertionInputPath = insertionInputPath;
        binaryWorkspaceData.InsertionOutputPath = insertionOutputPath;
    }
    
    private void RunScenarioWizard(string rootPath, out IWorkspaceData workspaceData)
    {
        workspaceData = new ScenarioWorkspaceData();
        var extractionInputPath = Path.Combine(rootPath, "extraction", "input");
        var extractionOutputPath = Path.Combine(rootPath, "extraction", "output");
        var insertionInputPath = Path.Combine(rootPath, "insertion", "input");
        var insertionOutputPath = Path.Combine(rootPath, "insertion", "output");
        var speakerDefinitionPath = Path.Combine(rootPath, "speaker_definitions.json");
        var simplificationInputPath = Path.Combine(rootPath, "simplification", "input");
        var simplificationOutputPath = Path.Combine(rootPath, "simplification", "output");
        var desimplificationOriginalPath = Path.Combine(rootPath, "desimplification", "original");
        var desimplificationInputPath = Path.Combine(rootPath, "desimplification", "input");
        var desimplificationOutputPath = Path.Combine(rootPath, "desimplification", "output");
        Directory.CreateDirectory(extractionInputPath);
        Directory.CreateDirectory(extractionOutputPath);    
        Directory.CreateDirectory(insertionInputPath);
        Directory.CreateDirectory(insertionOutputPath);
        if (!File.Exists(speakerDefinitionPath))
        {
            var exampleJson = new SpeakerIdData();
            exampleJson.Speakers.Add(new SpeakerId
            {
                Id = 0,
                Name = "Unknown Speaker"
            });
            var serialized = JsonConvert.SerializeObject(exampleJson, Formatting.Indented);
            File.WriteAllText(speakerDefinitionPath, serialized);
        }
        Directory.CreateDirectory(simplificationInputPath);
        Directory.CreateDirectory(simplificationOutputPath);
        Directory.CreateDirectory(desimplificationOriginalPath);
        Directory.CreateDirectory(desimplificationInputPath);
        Directory.CreateDirectory(desimplificationOutputPath);
        var scenarioWorkspaceData = (ScenarioWorkspaceData)workspaceData;
        scenarioWorkspaceData.RootWorkspacePath = rootPath;
        scenarioWorkspaceData.ExtractionInputPath = extractionInputPath;
        scenarioWorkspaceData.ExtractionOutputPath = extractionOutputPath;
        scenarioWorkspaceData.InsertionInputPath = insertionInputPath;
        scenarioWorkspaceData.InsertionOutputPath = insertionOutputPath;
        scenarioWorkspaceData.SpeakerDefinitionPath = speakerDefinitionPath;
        scenarioWorkspaceData.SimplificationInputPath = simplificationInputPath;
        scenarioWorkspaceData.SimplificationOutputPath = simplificationOutputPath;
        scenarioWorkspaceData.DesimplificationOriginalPath = desimplificationOriginalPath;
        scenarioWorkspaceData.DesimplificationInputPath = desimplificationInputPath;
        scenarioWorkspaceData.DesimplificationOutputPath = desimplificationOutputPath;
    }
}

