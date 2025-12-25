using System.IO;
using System.Threading.Tasks;
using PWAAT_bin_to_json;

namespace PWAATExtractorSuite.Models;

public static class CryptographyExtension
{
    public static async Task DecryptionSingle(string inputFilePath, string outputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException("Input file not found.", inputFilePath);
        }
        await DecryptionInternal(inputFilePath, outputFilePath);
    }
    
    public static async Task DecryptionBatch(string inputDirectoryPath, string outputDirectoryPath)
    {
        if (!FileUtils.ValidateDirectory(inputDirectoryPath))
        {
            throw new DirectoryNotFoundException("Input directory not found.");
        }
        if (!FileUtils.ValidateDirectory(outputDirectoryPath))
        {
            throw new DirectoryNotFoundException("Output directory not found.");
        }
        var inputFiles = Directory.GetFiles(inputDirectoryPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var inputFile in inputFiles)
        {
            var fileName = Path.GetFileName(inputFile);
            var outputFile = Path.Combine(outputDirectoryPath, fileName);
            await DecryptionInternal(inputFile, outputFile);
        }
    }
    
    private static async Task DecryptionInternal(string inputPath, string outputPath)
    {
        var buffer = await File.ReadAllBytesAsync(inputPath);
        var decrypted = Cryptography.Decrypt(buffer);
        await File.WriteAllBytesAsync(outputPath, decrypted);
    }
    
    public static async Task EncryptionSingle(string inputFilePath, string outputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException("Input file not found.", inputFilePath);
        }
        await EncryptionInternal(inputFilePath, outputFilePath);
    }
    
    public static async Task EncryptionBatch(string inputDirectoryPath, string outputDirectoryPath)
    {
        if (!FileUtils.ValidateDirectory(inputDirectoryPath))
        {
            throw new DirectoryNotFoundException("Input directory not found.");
        }
        if (!FileUtils.ValidateDirectory(outputDirectoryPath))
        {
            throw new DirectoryNotFoundException("Output directory not found.");
        }
        var inputFiles = Directory.GetFiles(inputDirectoryPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var inputFile in inputFiles)
        {
            var fileName = Path.GetFileName(inputFile);
            var outputFile = Path.Combine(outputDirectoryPath, fileName);
            await EncryptionInternal(inputFile, outputFile);
        }
    }
    
    private static async Task EncryptionInternal(string inputPath, string outputPath)
    {
        var buffer = await File.ReadAllBytesAsync(inputPath);
        var encrypted = Cryptography.Encrypt(buffer);
        await File.WriteAllBytesAsync(outputPath, encrypted);
    }
}