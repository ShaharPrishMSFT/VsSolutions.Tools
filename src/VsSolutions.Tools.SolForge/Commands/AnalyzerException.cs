// <copyright file="AnalyzerException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Commands;
using System;
using VsSolutions.Tools.SolForgeSolForge.Commands.CentralManagement;

internal class AnalyzerException : Exception
{
    public AnalyzerException(AnalysisItem analysisItem)
        : base(analysisItem.Description)
    {
        AnalysisItem = analysisItem;
    }

    public AnalysisItem AnalysisItem { get; }
}
