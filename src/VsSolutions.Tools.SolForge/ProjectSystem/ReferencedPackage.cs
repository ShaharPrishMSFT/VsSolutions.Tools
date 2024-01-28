// <copyright file="ReferencedPackage.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.ProjectSystem;
using System;
using System.Linq;
using System.Xml;

internal class ReferencedPackage(ProjectFile project, XmlNode sourceNode)
{
    public ProjectFile Project { get; } = project;

    public XmlNode Node { get; private set; } = sourceNode;

    public PackageName PackageName => new(Include ?? Update ?? throw new InvalidOperationException(Node.MessageWithLocation("No package name found here")));

    public string? Include => Node.GetAttribute("Include");

    public string? Update => Node.GetAttribute("Update");

    public string? Version => Node.GetAttribute("Version");

    public ConditionAndPackage ConditionAndPackage => new(Condition, PackageName);

    public string? Condition => Node.ParentNode?.GetAttribute("Condition");

    public string? NodeContent => string.Join(Environment.NewLine, Node.ChildNodes.Cast<XmlNode>().Select(x => x.OuterXml));

    private string? ComparableCondition => Condition?.Replace(" ", "");

    public override int GetHashCode() => HashCode.Combine(PackageName, Version, ComparableCondition, NodeContent);

    public void ReplaceNode(XmlNode newNode)
    {
        if (newNode.OwnerDocument != Node.OwnerDocument)
        {
            throw new ArgumentException("New node must be from the same document as the old node", nameof(newNode));
        }

        Node.ParentNode!.ReplaceChild(newNode, Node);
        Node = newNode;
    }

    public override bool Equals(object? obj)
        => ReferenceEquals(obj, this) || (obj is ReferencedPackage other && other.PackageName == PackageName && other.Version == Version && other.ComparableCondition == ComparableCondition && other.NodeContent == NodeContent);

    public override string ToString()
        => $"{PackageName} [{Version}] ({Condition})";
}
