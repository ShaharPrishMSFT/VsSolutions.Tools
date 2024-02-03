﻿// <copyright file="GeneralExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using VsSolutions.Tools.SolForge.ProjectSystem;

public static class Utils
{
    public static string GetFormattedXml(this XmlDocument doc)
    {
        using (var s = new MemoryStream())
        {
            WriteFormattedXml(doc, s);
            s.Position = 0;
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public static void WriteFormattedXml(this XmlDocument doc, string filename)
    {
        using (var stream = File.OpenWrite(filename))
        {
            stream.SetLength(0);
            doc.WriteFormattedXml(stream);
        }
    }

    public static void WriteFormattedXml(this XmlDocument doc, Stream stream)
    {
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = doc.FirstChild is not XmlDeclaration,
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            doc.Save(writer);
        }
    }

    public static void Add<TKey, TSubValue>(this Dictionary<TKey, List<TSubValue>> dict, TKey key, TSubValue value)
        where TKey : notnull
    {
        if (!dict.TryGetValue(key, out var list))
        {
            list = new List<TSubValue>();
            dict.Add(key, list);
        }

        list.Add(value);
    }

    public static void AppendChildren(this XmlNode node, IEnumerable<XmlNode> children)
    {
        foreach (var child in children)
        {
            node.AppendChild(child);
        }
    }

    public static bool HasAttribute(this XmlNode node, string name)
        => node.Attributes?[name] != null;

    public static void SetAttribute(this XmlNode node, string name, string value)
    {
        var attr = node.OwnerDocument!.CreateAttribute(name);
        attr.Value = value;
        node.Attributes!.Append(attr);
    }

    public static string? GetAttribute(this XmlNode node, string name)
    {
        var result = node.Attributes![name];
        if (result != null)
        {
            return result.Value;
        }

        // Search in a case insensitive way - VS reads the project file and does not care about case
        foreach (XmlAttribute attr in node.Attributes)
        {
            if (string.Equals(attr.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return attr.Value;
            }
        }

        return null;
    }

    public static bool RemoveAttribute(this XmlNode node, string name)
    {
        var attr = node.Attributes![name];
        if (attr != null)
        {
            node.Attributes.Remove(attr);
        }

        return attr != null;
    }

    public static string MessageWithLocation(this XmlNode node, string message)
        => $"{message} ({node.OwnerDocument!.DocumentElement!.BaseURI}: {node.GetFriendlyLocation()}";

    public static string GetFriendlyLocation(this XmlNode node)
        => node.GetLocation() switch
        {
            (-1, -1) => "[Unknown location]",
            var (line, position) => $"Location: {line}:{position}",
        };

    public static (int Line, int Position) GetLocation(this XmlNode node)
    {
        var xObject = node as IXmlLineInfo;
        if (xObject != null && xObject.HasLineInfo())
        {
            return (xObject.LineNumber, xObject.LinePosition);
        }

        return (-1, -1);
    }

    public static void AddCommands(this Command command, params Command[] subCommands)
        => command.AddCommands((IEnumerable<Command>)subCommands);

    public static void AddCommands(this Command command, IEnumerable<Command> subCommands)
    {
        foreach (var subCommand in subCommands)
        {
            command.AddCommand(subCommand);
        }
    }

    public static DirectoryInfo GetRootSolForgeConfig(string currentDirectory)
    {
        var root = RelativeFileLocation.ClosestSolutionRoot.GetDirectory(currentDirectory.AsDirectoryInfo());
        if (root == null)
        {
            Logger.LogError($"Could not find a solution root above {currentDirectory}");
            throw new InvalidOperationException();
        }

        return root.CombineDirectory(Consts.SolForgeDir);
    }

    public static DirectoryInfo CombineDirectory(this DirectoryInfo di, params string[] names)
        => new DirectoryInfo(Path.Combine(names.Prepend(di.FullName).ToArray()));

    public static FileInfo CombineFile(this DirectoryInfo di, params string[] names)
        => new FileInfo(Path.Combine(names.Prepend(di.FullName).ToArray()));

    [return: NotNullIfNotNull(nameof(dir))]
    public static DirectoryInfo? AsDirectoryInfo(this string? dir) => dir is string st ? new DirectoryInfo(st) : null;
}