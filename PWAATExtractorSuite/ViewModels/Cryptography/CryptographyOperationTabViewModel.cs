using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using R3;
using ReactiveUI;
using FileMode = PWAATExtractorSuite.Models.FileMode;
using ReactiveCommand = R3.ReactiveCommand;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;

namespace PWAATExtractorSuite.ViewModels.Cryptography;

public class CryptographyOperationTabViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    
    public ReactiveCommand<FileMode> SetFileModeCommand { get; } = new();
    public ReactiveCommand<CryptographyOperationMode> SetOperationModeCommand { get; } = new();
    public ReactiveCommand StartOperationCommand { get; } = new();
    public BindableReactiveProperty<bool> OpenOutputWhenDone { get; } = new(true);
    public BindableReactiveProperty<string> FileModeDescription { get; } = new("Default Description");
    public BindableReactiveProperty<string> OperationDescription { get; } = new("Default Description");
    public BindableReactiveProperty<bool> WorkspaceDataValid { get; } = new(true);
    
    private readonly IDialogService _dialogService;
    private readonly ILauncher _launcher;
    private FileMode _currentFileMode = FileMode.Single;
    private CryptographyOperationMode _currentOperationMode = CryptographyOperationMode.Decrypt;

    private readonly CryptographyExtractorModel _model;
    
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
    
    private readonly Dictionary<CryptographyOperationMode, string> _operationModeDescriptions = new()
    {
        { 
            CryptographyOperationMode.Decrypt, 
            "Decrypt the any game file into its respective format.\n" +
            "Steps:\n" +
            """1. Put the original game file(s) inside the "{0}" folder.""" +
            "\n" +
            "2. Start the operation.\n" +
            """3. See the output in "{1}" folder.""" 
        },
        { 
            CryptographyOperationMode.Encrypt, 
            "Decryption data into any game files, then exporting it as their respective format.\n" +
            "Steps:\n" +
            """1. Put the modified game file(s) inside the "{2}" folder.""" +
            "\n" +
            "2. Start the operation.\n" +
            """3. See the output in "{3}" folder.""" 
        }
    };
    #endregion
    
    public CryptographyOperationTabViewModel()
    {
        this.WhenActivated(BindWhenSelfActivate);
    }

    public CryptographyOperationTabViewModel(
        CryptographyExtractorModel model,
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
    
    private void OnOperationModeChanged(CryptographyOperationMode mode)
    {
        _currentOperationMode = mode;
        var description = _operationModeDescriptions[mode];
        description = string.Format(
            description,
            _model.WorkspaceData.Value.DecryptionInputPath,
            _model.WorkspaceData.Value.DecryptionOutputPath,
            _model.WorkspaceData.Value.EncryptionInputPath,
            _model.WorkspaceData.Value.EncryptionOutputPath);
        OperationDescription.Value = description;
    }
    
    private void OnWorkspaceDataChanged(CryptographyWorkspaceData workspaceData)
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
                case CryptographyOperationMode.Decrypt:
                    if (!await StartDecryption(_currentFileMode)) return;
                    break;
                case CryptographyOperationMode.Encrypt:
                    if (!await StartEncryption(_currentFileMode)) return;
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
            CryptographyOperationMode.Decrypt => _model.WorkspaceData.Value.DecryptionOutputPath,
            CryptographyOperationMode.Encrypt => _model.WorkspaceData.Value.EncryptionOutputPath,
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


    private async Task<bool> StartDecryption(FileMode fileMode)
    {
        switch (fileMode)
        {
            case FileMode.Single:
                var singleExtractFile = await _dialogService.PickSingleFile(null, _model.WorkspaceData.Value.DecryptionInputPath);
                if (singleExtractFile == null)
                    return false;
                var outputJsonFilePath = Path.Combine(_model.WorkspaceData.Value.DecryptionOutputPath,
                    Path.GetFileName(singleExtractFile));
                await CryptographyExtension.DecryptionSingle(singleExtractFile, outputJsonFilePath);
                break;
            case FileMode.Batch:
                await CryptographyExtension.DecryptionBatch(
                    _model.WorkspaceData.Value.DecryptionInputPath,
                    _model.WorkspaceData.Value.DecryptionOutputPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileMode), fileMode, null);
        }
        return true;
    }

    private async Task<bool> StartEncryption(FileMode fileMode)
    {
        switch (fileMode)
        {
            case FileMode.Single:
                var singleInsertFile = await _dialogService.PickSingleFile(null, _model.WorkspaceData.Value.EncryptionInputPath);
                if (singleInsertFile == null)
                    return false;
                var outputBinFilePath = Path.Combine(_model.WorkspaceData.Value.EncryptionOutputPath,
                    Path.GetFileName(singleInsertFile));
                await CryptographyExtension.EncryptionSingle(singleInsertFile, outputBinFilePath);
                break;
            case FileMode.Batch:
                await CryptographyExtension.EncryptionBatch(
                    _model.WorkspaceData.Value.EncryptionInputPath,
                    _model.WorkspaceData.Value.EncryptionOutputPath);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileMode), fileMode, null);
        }
        return true;
    }
}