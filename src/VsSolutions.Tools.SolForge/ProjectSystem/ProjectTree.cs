// <copyright file="ProjectTree.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.ProjectSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

internal class ProjectTree(string rootDirectory)
{
    private readonly List<ProjectFile> _projects = new();
    private readonly string _rootDirectory = rootDirectory;

    public int Count => _projects.Count;

    public IEnumerable<ProjectFile> Projects => _projects;

    public IEnumerable<ProjectFile> KnownFiles => _projects.Where(p => p.ProjectType != ProjectType.Unknown);

    public bool IsModified => _projects.Any(p => p.IsModified);

    public static ProjectTree Load(string dir)
    {
        var files = Directory
            .EnumerateFiles(dir, $"*{Consts.CsProjExtension}", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(dir, Consts.BuildProps, SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(dir, Consts.PackagesProps, SearchOption.AllDirectories));

        var tree = new ProjectTree(dir);

        foreach (var file in files)
        {
            var project = ProjectFile.Load(file, tree);
            tree._projects.Add(project);
        }

        return tree;
    }

    public ProjectFile LoadProject(string filename)
    {
        VerifyProjectLocation(filename);
        var project = ProjectFile.Load(filename, this);
        _projects.Add(project);
        return project;
    }

    public ProjectFile LoadProjectFromXml(string xml, string filename)
    {
        VerifyProjectLocation(filename);
        var project = ProjectFile.LoadFromXml(xml, filename, this);
        _projects.Add(project);
        return project;
    }

    public void SaveProjectTree(string? destination)
    {
        if (destination == null && !IsModified)
        {
            return;
        }

        foreach (var project in _projects)
        {
            project.SaveFile(GetDestination(project, destination));
        }
    }

    private string? GetDestination(ProjectFile project, string? destination)
    {
        if (destination == null)
        {
            return null;
        }

        var relativePath = Path.GetRelativePath(_rootDirectory, project.Filename);
        return Path.Combine(destination, relativePath);
    }

    private void VerifyProjectLocation(string filename)
    {
        if (!filename.StartsWith(_rootDirectory))
        {
            throw new InvalidOperationException($"Project file '{filename}' is not in the root directory '{_rootDirectory}'");
        }
    }
}