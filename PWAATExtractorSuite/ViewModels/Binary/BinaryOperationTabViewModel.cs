using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PWAAT_bin_to_json;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;
using ReactiveUI;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using FileMode = PWAATExtractorSuite.Models.FileMode;
using ReactiveCommand = R3.ReactiveCommand;

namespace PWAATExtractorSuite.ViewModels.Binary;

public class BinaryOperationTabViewModel : ViewModelBase, IActivatableViewModel
{
    
    public ViewModelActivator Activator { get; } = new();
    
    public ReactiveCommand<FileMode> SetFileModeCommand { get; } = new();
    public ReactiveCommand<BinaryOperationMode> SetOperationModeCommand { get; } = new();
    public ReactiveCommand StartOperationCommand { get; } = new();
    public BindableReactiveProperty<bool> OpenOutputWhenDone { get; } = new(true);
    public BindableReactiveProperty<string> FileModeDescription { get; } = new("Default Description");
    public BindableReactiveProperty<string> OperationDescription { get; } = new("Default Description");
    public BindableReactiveProperty<bool> WorkspaceDataValid { get; } = new(true);
    
    private readonly IDialogService _dialogService;
    private readonly ILauncher _launcher;
    private FileMode _currentFileMode = FileMode.Single;
    private BinaryOperationMode _currentOperationMode = BinaryOperationMode.Extract;

    private readonly BinaryExtractorModel _model;
    
    #region Descriptions
    private readonly Dictionary<FileMode, string> _fileModeDescriptions = new()
    {
        { 
            FileMode.Single, 
            "Operate on one file at a time.\n" +
            "Starting the operation will open a file picker corresponding to the selected operation mode." 
        },
        { 
            FileMode.Batch,
            "Operate on multiple files in a folder.\n" +
            "Operating folder will be chosen automatically according to the selected operation mode." 
        }
    };
    
    private readonly Dictionary<BinaryOperationMode, string> _operationModeDescriptions = new()
    {
        { 
            BinaryOperationMode.Extract, 
            "Extract data from binary files, then exporting it as a JSON format.\n" +
            "Steps:\n" +
            """1. Put the original .bin or .cho file(s) inside the "{0}" folder.""" +
            "\n" +
            "2. Start the operation.\n" +
            """3. See the output in "{1}" folder.""" 
        },
        { 
            BinaryOperationMode.Insert, 
            "Insert data into binary files, then exporting it as a binary format (either .bin or .cho, depending on the input.)\n" +
            "Steps:\n" +
            """1. Put the modified .json file(s) inside the "{2}" folder.""" +
            "\n" +
            "2. Start the operation.\n" +
            """3. See the output in "{3}" folder.""" 
        }
    };
    #endregion
    
    public BinaryOperationTabViewModel()
    {
        this.WhenActivated(BindWhenSelfActivate);
    }

    public BinaryOperationTabViewModel(
        BinaryExtractorModel model,
        IDialogService dialogService,
        ILauncher launcher)
        : this()
    {
        _model = model;
        _dialogService = dialogService;
        _launcher = launcher;
    }
    
    public void BindWhenParentActivate(CompositeDisposable disposables)
    {
        
        _model.WorkspaceData
            .Subscribe(OnWorkspaceDataChanged)
            .AddTo(disposables);
        
    }

    private void BindWhenSelfActivate(CompositeDisposable disposables)
    {
        SetFileModeCommand
            .Subscribe(OnFileModeChanged)
            .AddTo(disposables);
        SetOperationModeCommand
            .Subscribe(OnOperationModeChanged)
            .AddTo(disposables);
        StartOperationCommand
            .SubscribeAwait((_, _) => OnStartOperation(), AwaitOperation.Drop)
            .AddTo(disposables);
        OnWorkspaceDataChanged(_model.WorkspaceData.Value);
        OnFileModeChanged(_currentFileMode);
        OnOperationModeChanged(_currentOperationMode);
    }
    
    private void OnFileModeChanged(FileMode mode)
    {
        _currentFileMode = mode;
        FileModeDescription.Value = _fileModeDescriptions[mode];
    }
    
    private void OnOperationModeChanged(BinaryOperationMode mode)
    {
        _currentOperationMode = mode;
        var description = _operationModeDescriptions[mode];
        description = string.Format(
            description,
            _model.WorkspaceData.Value.ExtractionInputPath,
            _model.WorkspaceData.Value.ExtractionOutputPath,
            _model.WorkspaceData.Value.InsertionInputPath,
            _model.WorkspaceData.Value.InsertionOutputPath);
        OperationDescription.Value = description;
    }
    
    private void OnWorkspaceDataChanged(BinaryWorkspaceData workspaceData)
    {
        WorkspaceDataValid.Value = workspaceData.IsValid();
    }
    
    private async ValueTask OnStartOperation()
    {
        Console.WriteLine($"Starting operation {_currentOperationMode} in {_currentFileMode} mode.");
        try
        {
            switch (_currentOperationMode)
            {
                case BinaryOperationMode.Extract:
                    if (!await StartExtraction(_currentFileMode)) return;
                    break;
                case BinaryOperationMode.Insert:
                    if (!await StartInsertion(_currentFileMode)) return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowNotificationDialog("Operation Error", $"An error occurred during the operation:\n{ex.Message}");
            Console.WriteLine($"Error during operation: {ex}");
            return;
        }
        if (!OpenOutputWhenDone.Value)
        {
            await _dialogService.ShowNotificationDialog("Operation Completed", "The operation has completed successfully.");
            return;
        }
        var path = _currentOperationMode switch
        {
            BinaryOperationMode.Extract => _model.WorkspaceData.Value.ExtractionOutputPath,
            BinaryOperationMode.Insert => _model.WorkspaceData.Value.InsertionOutputPath,
            _ => throw new ArgumentOutOfRangeException()
        };
        var success = await _launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        if (!success)
        {
            await _dialogService.ShowNotificationDialog("Error", $"Failed to open path: {path}");
            Console.WriteLine($"Failed to open path: {path}");
        }
        await _dialogService.ShowNotificationDialog("Operation Completed", "The operation has completed successfully.");
    }


    private async Task<bool> StartExtraction(FileMode fileMode)
    {
        switch (fileMode)
        {
            case FileMode.Single:
                var singleExtractFile = await _dialogService.PickSingleFile([
                    new FilePickerFileType("Binary Files")
                    {
                        Patterns = ["*.bin", "*.cho"],
                        MimeTypes = ["application/octet-stream"]
                    }
                ], _model.WorkspaceData.Value.ExtractionInputPath);
                if (singleExtractFile == null)
                    return false;
                var outputJsonFilePath = Path.Combine(_model.WorkspaceData.Value.ExtractionOutputPath,
                    Path.ChangeExtension(
                        Path.GetFileName(singleExtractFile),
                        ".json"));
                await Extractor.ExtractSingle(singleExtractFile, outputJsonFilePath);
                break;
            case FileMode.Batch:
                await Extractor.ExtractBatch(
                    _model.WorkspaceData.Value.ExtractionInputPath,
                    _model.WorkspaceData.Value.ExtractionOutputPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileMode), fileMode, null);
        }
        return true;
    }

    private async Task<bool> StartInsertion(FileMode fileMode)
    {
        switch (fileMode)
        {
            case FileMode.Single:
                var singleInsertFile = await _dialogService.PickSingleFile([
                    new FilePickerFileType("JSON Files")
                    {
                        Patterns = ["*.json"],
                        MimeTypes = ["application/json"]
                    }
                ], _model.WorkspaceData.Value.InsertionInputPath);
                if (singleInsertFile == null)
                    return false;
                var outputBinFilePath = Path.Combine(_model.WorkspaceData.Value.InsertionOutputPath,
                    Path.ChangeExtension(
                        Path.GetFileName(singleInsertFile),
                        ".bin"));
                await Inserter.InsertSingle(singleInsertFile, outputBinFilePath);
                break;
            case FileMode.Batch:
                await Inserter.InsertBatch(
                    _model.WorkspaceData.Value.InsertionInputPath,
                    _model.WorkspaceData.Value.InsertionOutputPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileMode), fileMode, null);
        }
        return true;
    }
    
}