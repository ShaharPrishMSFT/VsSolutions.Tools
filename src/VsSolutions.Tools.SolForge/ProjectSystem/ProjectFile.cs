// <copyright file="ProjectFile.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

internal class ProjectFile : IEquatable<ProjectFile>
{
    private bool _isModified;

    public ProjectFile(XmlDocument xml, string filename, ProjectTree projectTree)
    {
        Xml = xml;
        Filename = filename;
        ProjectTree = projectTree;

        xml.NodeChanged += Xml_NodeChanged;
        xml.NodeInserted += Xml_NodeChanged;
        xml.NodeRemoved += Xml_NodeChanged;
    }

    private void Xml_NodeChanged(object sender, XmlNodeChangedEventArgs e)
    {
        IsModified = true;
    }

    public XmlDocument Xml { get; }

    public string Filename { get; }

    public ProjectTree ProjectTree { get; }

    public bool IsModified
    {
        get => _isModified;
        private set => _isModified = value;
    }

    public ProjectType ProjectType
        => (Path.GetFileName(Filename), Path.GetExtension(Filename)) switch
        {
            (Consts.BuildProps, _) => ProjectType.BuildProperties,
            (Consts.PackagesProps, _) => ProjectType.PackagesProperties,
            (_, Consts.CsProjExtension) => ProjectType.CsProj,
            _ => ProjectType.Unknown,
        };

    public static ProjectFile Load(string filename, ProjectTree projectTree)
    {
        var xml = new XmlDocument();
        xml.Load(filename);
        return new ProjectFile(xml, filename, projectTree);
    }

    public static ProjectFile LoadFromXml(string xml, string fileName, ProjectTree projectTree)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        return new ProjectFile(xmlDocument, fileName, projectTree) { IsModified = true };
    }

    public bool SaveFile(string? filename)
    {
        if (filename == null && !IsModified)
        {
            return false;
        }

        if (Xml.FirstChild is not XmlComment existing || (!existing.Value?.Contains("SolForge") ?? true))
        {
            var comment = Xml.CreateComment(Consts.ContributeToProject);
            Xml.InsertBefore(comment, Xml.DocumentElement);
        }

        var dir = Path.GetDirectoryName(filename ?? Filename);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir!);
        }

        Xml.WriteFormattedXml(filename ?? Filename);

        if (filename == null)
        {
            IsModified = false;
        }

        return true;
    }

    public IEnumerable<ReferencedPackage> GetPackages()
    {
        return Xml
            .SelectNodes("//PackageReference")?
            .Cast<XmlNode>()
            .Select(node => new ReferencedPackage(this, node)) ?? Array.Empty<ReferencedPackage>();
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is ProjectFile project && string.Equals(Filename, project.Filename));

    public bool Equals(ProjectFile? other) => other != null && (ReferenceEquals(this, other) || string.Equals(Filename, other.Filename));

    public override int GetHashCode() => HashCode.Combine(Filename);

    public override string ToString() => Filename;
}
