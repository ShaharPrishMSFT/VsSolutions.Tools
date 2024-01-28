// <copyright file="AnalysisResult.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Analyzers.NuGetPackages;

internal record AnalysisResult
{
    private AnalysisResult(string? error)
    {
        Error = error;
    }

    public bool WasSuccessful => Error == null;

    public string? Error { get; }

    public IList<AnalysisItem> Items { get; init; } = new List<AnalysisItem>();

    public static AnalysisResult Failed(string error, params AnalysisItem[] analysisItems)
        => new(error) { Items = analysisItems.ToList() };

    public static AnalysisResult Success()
        => new AnalysisResult((string?)null);
}
