// <copyright file="SolForgeMain.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge;

using System;
using System.CommandLine;
using VsSolutions.Tools.SolForgeSolForge.Analyzers.NuGetPackages;

internal class SolForgeMain
{
    private RootCommand _rootCommand;

    public SolForgeMain()
    {
        var r = new RootCommand("SolForge analyzes and modifies Visual Studio solution files to make them more sound");

        var directoryOption = new Option<string>(
            new[] { "--directory", "-d" },
            "The directory to work in");

        var applyOption = new Option<bool>(
            new[] { "--apply", "-a" },
            "Make changes in the file system");

        var analyzeCommand = new Command("centralmgmt", "Modify the solution to support NuGet central package management");
        analyzeCommand.AddOption(applyOption);
        analyzeCommand.SetHandler(CentralNuGetPackageManagement, directoryOption, applyOption);

        r.AddCommand(analyzeCommand);
        r.AddGlobalOption(directoryOption);

        _rootCommand = r;
    }

    public void Run(string[] args)
    {
        _rootCommand.Invoke(args);
    }

    private void CentralNuGetPackageManagement(string? directoryOption, bool applyOption)
    {
        directoryOption ??= Environment.CurrentDirectory;
        var analyzer = new NuGetManagementAnalyzer(directoryOption, applyOption);
        analyzer.Analyze();
    }
}
