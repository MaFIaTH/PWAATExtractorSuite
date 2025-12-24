using System;
using ObservableCollections;
using PWAATExtractorSuite.ViewModels.Shared;

namespace PWAATExtractorSuite.Models;

public class BinaryExtractorModel
{
    public readonly R3.ReactiveProperty<BinaryWorkspaceData> WorkspaceData = new(new());
}