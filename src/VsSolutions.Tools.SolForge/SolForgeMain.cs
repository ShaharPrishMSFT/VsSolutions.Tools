// <copyright file="SolForgeMain.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using VsSolutions.Tools.SolForge;
using VsSolutions.Tools.SolForge.Commands.NewFile;
using VsSolutions.Tools.SolForgeSolForge.Commands.CentralManagement;

internal class SolForgeMain
{
    public static readonly Option<bool> OverwriteOption = new Option<bool>(
        ["--overwrite", "-o"],
        "Overwrite the file if it exists");

    private static readonly Option<string> _directoryOption = new Option<string>(
        ["--directory", "-d"],
        "The directory to work in");

    private static readonly Option<bool> _applyOption = new Option<bool>(
        ["--apply", "-a"],
        "Make changes in the file system");

    private static readonly Option<string> _locationOption = new Option<string>(
        ["--location", "-l"],
        "Name and optional location of a file");

    private RootCommand _rootCommand;

    public SolForgeMain()
    {
        var rootCommand = new RootCommand("SolForge analyzes and modifies Visual Studio solution files to make them more sound");

        rootCommand.AddGlobalOption(_directoryOption);

        // Central management
        var centralManagement = new Command("centralmgmt", "Modify the solution to support NuGet central package management");
        centralManagement.AddOption(_applyOption);
        centralManagement.SetHandler(CentralNuGetPackageManagement, _directoryOption, _applyOption);
        rootCommand.AddCommand(centralManagement);

        // New command for templates.
        var newCommand = new Command("new", "Create a new file from template");
        newCommand.AddCommands(FileTemplate.GetTemplates().Select(CreateTemplateSubCommand));
        newCommand.SetHandler(() => newCommand.Invoke("-h"));
        rootCommand.AddCommand(newCommand);

        _rootCommand = rootCommand;
    }

    private Command CreateTemplateSubCommand(FileTemplate t)
    {
        var command = new Command(t.Command, t.Description);
        command.AddOption(OverwriteOption);
        command.AddOption(_locationOption);
        command.SetHandler((d, l, o) => CreateNewFile(t, d, l, o), _directoryOption, _locationOption, OverwriteOption);
        return command;
    }

    private void CreateNewFile(FileTemplate template, string? directoryOption, string? locationOption, bool overwriteOption)
    {
        directoryOption ??= Environment.CurrentDirectory;
        var file = new NewFileCommandHandler(template, directoryOption, locationOption, overwriteOption);
        file.Analyze();
    }

    public void Run(string[] args)
    {
        _rootCommand.Invoke(args);
    }

    private void CentralNuGetPackageManagement(string? directoryOption, bool applyOption)
    {
        directoryOption ??= Environment.CurrentDirectory;
        var analyzer = new NuGetManagementCommandHandler(directoryOption, applyOption);
        analyzer.Analyze();
    }
}
