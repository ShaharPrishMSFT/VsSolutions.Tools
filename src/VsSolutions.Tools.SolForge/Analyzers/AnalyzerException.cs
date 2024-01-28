// <copyright file="AnalyzerException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Analyzers;
using System;
using VsSolutions.Tools.SolForgeSolForge.Analyzers.NuGetPackages;

internal class AnalyzerException : Exception
{
    public AnalyzerException(AnalysisItem analysisItem)
        : base(analysisItem.Description)
    {
        AnalysisItem = analysisItem;
    }

    public AnalysisItem AnalysisItem { get; }
}
