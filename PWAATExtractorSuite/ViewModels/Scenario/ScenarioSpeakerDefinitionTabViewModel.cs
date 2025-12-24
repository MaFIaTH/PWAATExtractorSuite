using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using ObservableCollections;
using PWAAT_scenario_extractor.Simplifier;
using PWAATExtractorSuite.Models;
using PWAATExtractorSuite.ViewModels.Dialogs;
using PWAATExtractorSuite.ViewModels.Shared;
using R3;
using ReactiveUI;
using ReactiveCommand = R3.ReactiveCommand;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;

namespace PWAATExtractorSuite.ViewModels.Scenario;

public class ScenarioSpeakerDefinitionTabViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    public WorkspacePathHandler SpeakerDefinition { get; } = new("Speaker Definition", string.Empty);
    
    public ReactiveCommand BrowseSpeakerDefinitionPathCommand { get; } = new();
    public ReactiveCommand OpenSpeakerDefinitionPathCommand { get; } = new();
    public ReactiveCommand RefreshSpeakerDefinitionPathCommand { get; } = new();
    public ReactiveCommand SpeakerDefinitionTextBoxLostFocusCommand { get; } = new();
    public ReactiveCommand AddNewRowCommand { get; } = new();
    public ReactiveCommand<SpeakerDefinitionHandler> SpeakerIdTextBoxLostFocusCommand { get; } = new();
    public ReactiveCommand<SpeakerDefinitionHandler> SpeakerNameTextBoxLostFocusCommand { get; } = new();
    public ReactiveCommand<SpeakerDefinitionHandler> SpeakerDeleteCommand { get; } = new();
    
    public NotifyCollectionChangedSynchronizedViewList<SpeakerDefinitionHandler> SpeakerDefinitionViews { get; private set; }
    
    private readonly ObservableList<SpeakerDefinitionHandler> _speakerDefinitions = new();

    private readonly ScenarioExtractorModel _model;
    private readonly IDialogService _dialogService;
    private readonly ILauncher _launcher;

    public ScenarioSpeakerDefinitionTabViewModel()
    {
        SpeakerDefinitionViews = _speakerDefinitions.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
    }
    
    public ScenarioSpeakerDefinitionTabViewModel(
        ScenarioExtractorModel model,
        IDialogService dialogService,
        ILauncher launcher) 
        : this()
    {
        _model = model;
        _dialogService = dialogService;
        _launcher = launcher;
        this.WhenActivated(BindWhenSelfActivate);
    }
    
    private void BindWhenSelfActivate(CompositeDisposable disposables)
    {
        SpeakerDefinition.PreviousPath = SpeakerDefinition.Path.Value;
        foreach (var speakerDefinition in _speakerDefinitions)
        {
            speakerDefinition.PreviousValue = speakerDefinition.ToSpeakerId();
        }
    }

    public void BindWhenParentActivate(CompositeDisposable disposables)
    {
        SpeakerDefinitionViews = _speakerDefinitions.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        BrowseSpeakerDefinitionPathCommand
            .SubscribeAwait((_, _) => GetFilePath(SpeakerDefinition), AwaitOperation.Drop)
            .AddTo(disposables);
        OpenSpeakerDefinitionPathCommand
            .SubscribeAwait((_, _) => OpenFile(SpeakerDefinition), AwaitOperation.Drop)
            .AddTo(disposables);
        RefreshSpeakerDefinitionPathCommand
            .SubscribeAwait((_, _) => OnRefreshFile(SpeakerDefinition), AwaitOperation.Drop)
            .AddTo(disposables);
        SpeakerDefinitionTextBoxLostFocusCommand
            .SubscribeAwait((_, _) => OnPathTextBoxLostFocus(SpeakerDefinition), AwaitOperation.Drop)
            .AddTo(disposables);
        SpeakerIdTextBoxLostFocusCommand
            .SubscribeAwait((x, _)  => OnIdTextBoxLostFocus(x), AwaitOperation.Drop)
            .AddTo(disposables);
        SpeakerNameTextBoxLostFocusCommand
            .SubscribeAwait((x, _)  => OnNameTextBoxLostFocus(x), AwaitOperation.Drop)
            .AddTo(disposables);
        SpeakerDeleteCommand
            .Subscribe(OnDeleteSpeakerDefinition)
            .AddTo(disposables);
        AddNewRowCommand
            .Subscribe(_ => OnAddNewRow())
            .AddTo(disposables);
        Disposable.Create(() =>
        {
            SpeakerDefinitionViews.Dispose();
        }).AddTo(disposables);
    }
    
    private async ValueTask GetFilePath(WorkspacePathHandler handler)
    {
        await handler.GetFilePath(_dialogService, [
            new FilePickerFileType("JSON Files")
            {
                Patterns = ["*.json"],
                MimeTypes = ["application/json"]
            }
        ]);
        ApplyFilePath();
        await LoadSpeakerDefinitions(handler.Path.Value);
    }

    private async ValueTask OpenFile(WorkspacePathHandler handler)
    {
        await handler.OpenFile(_dialogService, _launcher);
    }
    
    private async ValueTask OnRefreshFile(WorkspacePathHandler handler)
    {
        await LoadSpeakerDefinitions(handler.Path.Value);
    }

    private async ValueTask OnPathTextBoxLostFocus(WorkspacePathHandler handler)
    {
        await handler.OnFileTextBoxLostFocus(_dialogService);
        ApplyFilePath();
    }
    
    private async ValueTask OnIdTextBoxLostFocus(SpeakerDefinitionHandler handler)
    {
        var newId = handler.Id.Value;
        if (handler.PreviousValue.Id.Equals(newId)) return;
        handler.PreviousValue = handler.ToSpeakerId();
        ApplySpeakerDefinitions();
    }
    
    private async ValueTask OnNameTextBoxLostFocus(SpeakerDefinitionHandler handler)
    {
        var newName = handler.Name.Value;
        if (handler.PreviousValue.Name.Equals(newName)) return;
        handler.PreviousValue = handler.ToSpeakerId();
        ApplySpeakerDefinitions();
    }
    
    private void OnDeleteSpeakerDefinition(SpeakerDefinitionHandler handler)
    {
        _speakerDefinitions.Remove(handler);
        ApplySpeakerDefinitions();
    }
    
    private void OnAddNewRow()
    {
        var newHandler = new SpeakerDefinitionHandler(new SpeakerId
        {
            Id = 0,
            Name = string.Empty
        });
        _speakerDefinitions.Add(newHandler);
        ApplySpeakerDefinitions();
    }
    
    private void ApplyFilePath()
    {
        _model.WorkspaceData.Value.SpeakerDefinitionPath = SpeakerDefinition.Path.Value;
        _model.WorkspaceData.OnNext(_model.WorkspaceData.Value);
    }

    private void ApplySpeakerDefinitions()
    {
        _model.WorkspaceData.Value.SpeakerDefinitions.Clear();
        foreach (var handler in _speakerDefinitions)
        {
            _model.WorkspaceData.Value.SpeakerDefinitions.Add(handler);
        }
        _model.WorkspaceData.OnNext(_model.WorkspaceData.Value);
    }

    public async Task LoadSpeakerDefinitions(string? speakerDefinitionPath)
    {
        if (string.IsNullOrWhiteSpace(speakerDefinitionPath) || !File.Exists(speakerDefinitionPath))
        {
            _speakerDefinitions.Clear();
            return;
        }
        var speakerIdData = JsonConvert.DeserializeObject<SpeakerIdData>(await File.ReadAllTextAsync(speakerDefinitionPath));
        if (speakerIdData == null)
        {
            _speakerDefinitions.Clear();
            return;
        }
        _speakerDefinitions.Clear();
        foreach (var speaker in speakerIdData.Speakers)
        {
            var handler = new SpeakerDefinitionHandler(speaker);
            _speakerDefinitions.Add(handler);
        }
        ApplySpeakerDefinitions();
    }
}

public record SpeakerDefinitionHandler
{
    public BindableReactiveProperty<uint> Id { get; } = new(0);
    public BindableReactiveProperty<string> Name { get; } = new(string.Empty);
    
    public SpeakerId PreviousValue { get; set; } = new() { Id = 0, Name = string.Empty };

    public SpeakerDefinitionHandler(){}
    public SpeakerDefinitionHandler(SpeakerId speakerId)
    {
        Id.Value = speakerId.Id;
        Name.Value = speakerId.Name;
        PreviousValue = speakerId;
    }
    public bool CompareMemberwise(SpeakerDefinitionHandler? other)
    {
        if (other == null) return false;
        return Id.Value.Equals(other.Id.Value) && Name.Value.Equals(other.Name.Value);
    }

    public SpeakerDefinitionHandler Copy()
    {
        return new SpeakerDefinitionHandler
        {
            Id = { Value = Id.Value },
            Name = { Value = Name.Value },
            PreviousValue = PreviousValue
        };
    }
    
    public SpeakerId ToSpeakerId()
    {
        return new SpeakerId
        {
            Id = Id.Value,
            Name = Name.Value
        };
    }
}