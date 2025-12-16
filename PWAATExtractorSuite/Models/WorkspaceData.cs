using System.IO;
using MessagePack;

namespace PWAATExtractorSuite.Models;

[Union(0, typeof(BinaryWorkspaceData))]
public interface IWorkspaceData
{
    ExtractorType Type { get; }
    string RootWorkspacePath { get;set; }
    public bool IsValid();
}

[MessagePackObject(keyAsPropertyName: true)]
public class BinaryWorkspaceData : IWorkspaceData
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
}