// <copyright file="NuGetManagementAnalyzer.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.Analyzers.NuGetPackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VsSolutions.Tools.SolForgeSolForge.ProjectSystem;

internal partial class NuGetManagementAnalyzer(string? directory, bool applyChanges)
{
    private string _directory = directory ?? Directory.GetCurrentDirectory();
    private bool _applyChanges = applyChanges;

    public void Analyze()
    {
        try
        {
            Logger.LogImportant($"Starting to analyze directory {_directory}");

            // Find all csproj and known props files files in the directory, recursively
            var tree = ProjectTree.Load(_directory);

            Logger.LogInfo($"Found {tree.Count} project files");

            if (tree.KnownFiles.Any(x => x.ProjectType == ProjectType.PackagesProperties))
            {
                Logger.LogError($"Found {Consts.PackagesProps} file, we don't yet know how to update these yet. Aborting operation.");
            }

            AnalyzePackages(tree);
        }
        catch (AnalyzerException ex)
        {
            Logger.LogError($"""
Could not modify the projects to use Nugets central management feature. Contact the team for help.
Problem was: {ex.AnalysisItem.Description}

Project file (if any): {ex.AnalysisItem.LoadedProject?.Filename}
""");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error: {ex}");
        }
    }

    private void AnalyzePackages(ProjectTree tree)
    {
        var projects = tree.KnownFiles;

        // Build a list of ReferencedPackage instances by using xpath to find all PackageReference nodes
        var referencedPackages = projects
            .SelectMany(p => p.GetPackages())
            .Where(rp => rp.Version != null)
            .ToList();

        // Now, group packages by condition and then by package name
        var groupedByCondition = referencedPackages
            .GroupBy(rp => rp.Condition)
            .OrderBy(g => g.Key)
            .ToList()
            .Select(
                conditionGroup => new UniqueConditions(
                    conditionGroup.Key,
                    conditionGroup
                        .GroupBy(x => x.PackageName)
                        .OrderBy(pg => pg.Key) // package groups
                        .ToDictionary(pg => pg.Key, pg => new ProjectReferencedPackages(pg.ToList()))))
            .ToList();

        var packageProps = tree.LoadProjectFromXml(Consts.DirectoryPackagesPropsTemplate, Path.Combine(_directory, Consts.PackagesProps));
        var selectedPackages = FillPackages(groupedByCondition, packageProps);

        // We now have our selected packages. Now go to all files and update the PackageReference nodes to either use the one in the shared file, or if
        // different, update the one in the relevant project file.
        // Each package will be replaced by a <PackageReference Update="<name>"> node.
        // If the package has a different version from the selected one, we will specify the Version attribute.
        // If the content of the node is different, we will clone it into the new node.
        var modifiedFiles = new HashSet<ProjectFile>();
        int packagesWithOverrideVersions = 0;

        foreach (var project in projects)
        {
            // Find all PackageReference nodes in the project file
            var projectPackages = project.GetPackages().ToList();
            if (!projectPackages.Any())
            {
                continue;
            }

            // Create a new "naked" Item group with no "Condition" attribute.
            // We will strive to keep the project structure as is - replacing nodes in the xml.
            // But in cases when we have conditions, we will just place it in a new one.
            var unparentedItemGroup = project.Xml.CreateElement("ItemGroup");

            VerifySelfConsistentPackagesAndContent(project, projectPackages, selectedPackages);
            var addedPackages = new HashSet<string>();

            // Go through each node, and if it is in the selected packages, update it accordingly.
            foreach (var package in projectPackages)
            {
                modifiedFiles.Add(project);
                selectedPackages.TryGetValue(package.ConditionAndPackage, out var globalReference);

                if (!addedPackages.Add(package.PackageName.Name))
                {
                    package.Node.ParentNode!.RemoveChild(package.Node);
                    continue;
                }

                // Create the new starting node.
                var refNode = project.Xml.CreateNode(XmlNodeType.Element, "PackageReference", package.Node.NamespaceURI);
                refNode.SetAttribute("Include", package.PackageName.Name);

                // Add the version if different.
                if (package.Version != null && globalReference?.Version != package.Version)
                {
                    refNode.SetAttribute("VersionOverride", package.Version);
                    packagesWithOverrideVersions++;
                }

                // Add the content
                if (package.Node.ChildNodes.Count > 0)
                {
                    refNode.AppendChildren(package.Node.ChildNodes.Cast<XmlNode>().Select(x => project.Xml.ImportNode(x, true)));
                }

                if (package.Condition != null)
                {
                    // For elements in conditions, we will just add them to a new ItemGroup node.
                    unparentedItemGroup.AppendChild(refNode);
                    package.Node.ParentNode!.RemoveChild(package.Node);
                }
                else // Otherwise, we will replace the existing node.
                {
                    package.ReplaceNode(refNode);
                }
            }

            // If we didn't add anything into the new item group, skip doing anything with it.
            if (unparentedItemGroup.ChildNodes.Count == 0)
            {
                continue;
            }

            // Now, remove all empty ItemGroup nodes and replace the first one with the replacement we created.
            var itemGroups = project.Xml.SelectNodes("//ItemGroup")?.Cast<XmlNode>().ToArray();
            if (itemGroups == null)
            {
                project.Xml.DocumentElement!.AppendChild(unparentedItemGroup);
            }
            else
            {
                if (itemGroups.Length > 0)
                {
                    project.Xml.DocumentElement!.InsertBefore(unparentedItemGroup, itemGroups[0]);
                }
                else
                {
                    project.Xml.DocumentElement!.AppendChild(unparentedItemGroup);
                }

                foreach (var itemGroup in itemGroups)
                {
                    if (itemGroup.ChildNodes.Count == 0)
                    {
                        itemGroup.ParentNode!.RemoveChild(itemGroup);
                    }
                }
            }
        }

        if (packagesWithOverrideVersions > 0)
        {
            Logger.LogWarning($"""
Found {packagesWithOverrideVersions} packages with override versions. Comments are added to the {Consts.PackagesProps} file.
In most repos, it is better to not have overrides at all.
""");
        }

        var uniquePackageCount = selectedPackages.Select(x => x.Value).DistinctBy(x => x.PackageName).Count();
        var uniqueConditionCount = selectedPackages.Select(x => x.Key.Condition).Distinct().Count();
        Logger.LogInfo($"Found {uniquePackageCount} unique packages in {uniqueConditionCount} conditions");

        if (!_applyChanges)
        {
            Logger.LogInfo("No files will be modified. Run the command with --apply to modify the project files");
            var tempDir = Path.Combine(Path.GetTempPath(), "SolForge", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            tree.SaveProjectTree(tempDir);
            Logger.LogInfo($"Project files saved to {tempDir}");
        }
        else
        {
            tree.SaveProjectTree(null);
            Logger.LogInfo("Project files modified");
        }
    }

    private void VerifySelfConsistentPackagesAndContent(ProjectFile loadedProject, List<ReferencedPackage> projectPackages, Dictionary<ConditionAndPackage, ReferencedPackage> selectedPackages)
    {
        // There are combinations that are too hard to do here for now. So we are being lazy. We will abort in these cases.
        // The packages file contains a list of n package names. Some of the package names will appear multiple time in the same file under different conditions.
        // The case we don't know how to handle is if a package in one of the conditions has a different content than the same package in another condition and from what's in the Packages file.

        var grouped = projectPackages.GroupBy(x => x.PackageName).ToList();
        foreach (var group in grouped)
        {
            var packages = group.ToList();
            if (packages.Count == 1)
            {
                continue;
            }

            var first = packages.First();
            if (packages.All(first.Equals))
            {
                continue;
            }

            if (packages.All(x => selectedPackages.TryGetValue(x.ConditionAndPackage, out var selected) && string.Equals(selected.NodeContent, x.NodeContent)))
            {
                continue;
            }

            throw new AnalyzerException(
                new AnalysisItem(
                    loadedProject,
                    AnalysisLevel.Error,
                    $"""
Package {group.Key} has different content in different conditions. This is not supported yet. Please fix manually.
Here's the list of elements in the same file that are different:
{string.Join(Environment.NewLine, packages.Select(x => x.Node.OuterXml))}

And here's the list of elements in the packages file:
{string.Join(Environment.NewLine, selectedPackages.Where(kv => kv.Key.PackageName == group.Key).Select(x => x.Value.Node.OuterXml))}
"""));
        }
    }

    private record UniqueConditions(string? Condition, IReadOnlyDictionary<PackageName, ProjectReferencedPackages> PackageReferences);

    private record ProjectReferencedPackages(IList<ReferencedPackage> Packages);

    private Dictionary<ConditionAndPackage, ReferencedPackage> FillPackages(IList<UniqueConditions> groupedByCondition, ProjectFile packageProps)
    {
        // This stores the packages we put in the .packages file.
        // For each Condition, we have a given package + version + content we placed in the file.
        var selectedVersionPackages = new Dictionary<ConditionAndPackage, ReferencedPackage>();

        // Create a new ItemGroup node for each condition
        // Then , create a PackageReference node for each package in each condition
        // If a package appears multiple times in the same condition, emit all of them, but with an xml comment warning above.
        var xml = packageProps.Xml;
        var root = xml.DocumentElement!;
        foreach (var (condition, packageGroups) in groupedByCondition)
        {
            var itemGroupElement = xml.CreateElement("ItemGroup");
            if (condition != null)
            {
                var conditionAttribute = xml.CreateAttribute("Condition");
                conditionAttribute.Value = condition;
                itemGroupElement.Attributes.Append(conditionAttribute);
            }

            foreach (var packageGroup in packageGroups)
            {
                var packagesByVersion = packageGroup.Value.Packages.GroupBy(x => x.Version).ToList();
                var sortedVersions = packagesByVersion.OrderByDescending(x => x.Key).ToList();
                if (packagesByVersion.Count == 1)
                {
                    var selectedPackage = packagesByVersion.Single().First();
                    if (selectedPackage.Version == null)
                    {
                        continue;
                    }

                    AddPackageVersion(xml, itemGroupElement, selectedPackage); // First single is the version group, the second is on the list of packages
                    selectedVersionPackages.TryAdd(selectedPackage.ConditionAndPackage, selectedPackage);
                }
                else
                {
                    // Package will appear as many time

                    // The first one we emit without any explanation. The rest we emit inside a comment.
                    var selectedPackage = sortedVersions.First().First();
                    if (selectedPackage.Version == null)
                    {
                        Logger.LogWarning($"Package {selectedPackage.PackageName} in {selectedPackage.Project.Filename} has no version");
                        continue;
                    }

                    AddPackageVersion(xml, itemGroupElement, selectedPackage);
                    selectedVersionPackages.TryAdd(selectedPackage.ConditionAndPackage, selectedPackage);
                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine($"\t\t\tMultiple versions of {packageGroup.Key} found:");
                    foreach (var sameVersionGroup in sortedVersions.Skip(1))
                    {
                        sb.Append($"\t\t\t\t{sameVersionGroup.Key}: ");
                        var indent = "";

                        if (sameVersionGroup.Count() > 1)
                        {
                            sb.AppendLine();
                            indent = "\t\t\t\t\t";
                        }

                        foreach (var package in sameVersionGroup)
                        {
                            sb.AppendLine($"{indent}{package.Project.Filename}");
                        }
                    }

                    sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);

                    var otherVersionsComment = xml.CreateComment(sb.ToString());
                    itemGroupElement.AppendChild(otherVersionsComment);
                }
            }

            root!.AppendChild(itemGroupElement);
        }

        return selectedVersionPackages;
    }

    private static void AddPackageVersion(XmlDocument xml, XmlElement itemGroupElement, ReferencedPackage package)
    {
        var packageReference = xml.CreateElement("PackageVersion");
        packageReference.SetAttribute("Include", package.PackageName.Name);

        packageReference.SetAttribute("Version", package.Version);

        itemGroupElement.AppendChild(packageReference);
    }
}
