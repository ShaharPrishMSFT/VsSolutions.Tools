namespace VsSolutions.Tools.SolForge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VsSolutions.Tools.SolForge.ProjectSystem;
using VsSolutions.Tools.SolForgeSolForge;

internal class FileTemplate(string command, string description, string template, RelativeFileLocation relativeFileLocation, string filename)
{

    public string Command { get; } = command;

    public string Description { get; } = description;

    public string Template { get; } = template;

    public string Filename { get; } = filename;

    public RelativeFileLocation RelativeFileLocation { get; } = relativeFileLocation;

    public static IEnumerable<FileTemplate> GetTemplates() => Templates.TemplateInfos;

    public static string GetTemplate(string command)
        => GetTemplates().FirstOrDefault(t => t.Command == command)?.Template ?? throw new InvalidOperationException($"No template found for command {command}");

    private static class Templates
    {
        public const string SolForgeConfig = """
{
}
""";

        public static readonly FileTemplate[] TemplateInfos = [
            new FileTemplate("solforgeconfig", "This file is the general project-wide configuration file for SolForge.", SolForgeConfig, RelativeFileLocation.SolForgeConfig, Consts.SolForgeConfig),
            ];
    }

    private record TemplateInfo(string TemplateName, RelativeFileLocation RelativeFileLocation, string Filename, string Description, string Template);
}
