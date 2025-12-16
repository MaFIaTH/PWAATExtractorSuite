using System;

namespace PWAATExtractorSuite.Models;

public class BinaryExtractorModel
{
    public readonly R3.ReactiveProperty<BinaryWorkspaceData> WorkspaceData = new(new());
}