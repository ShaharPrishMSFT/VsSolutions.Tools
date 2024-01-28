// <copyright file="AnalysisItem.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Analyzers.NuGetPackages;

using VsSolutions.Tools.SolForgeSolForge.ProjectSystem;

internal record AnalysisItem(ProjectFile LoadedProject, AnalysisLevel Level, string Description);
