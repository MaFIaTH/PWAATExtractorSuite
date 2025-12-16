using System;
using System.IO;
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
        throw new NotImplementedException();
        // Implement the text extractor wizard logic here
    }
}

