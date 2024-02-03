// <copyright file="AnalysisItem.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Commands.CentralManagement;

using VsSolutions.Tools.SolForgeSolForge.ProjectSystem;

internal record AnalysisItem(ProjectFile LoadedProject, AnalysisLevel Level, string Description);
