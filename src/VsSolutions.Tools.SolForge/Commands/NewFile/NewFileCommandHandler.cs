namespace VsSolutions.Tools.SolForge.Commands.NewFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsSolutions.Tools.SolForge.ProjectSystem;
using VsSolutions.Tools.SolForgeSolForge;

internal class NewFileCommandHandler(FileTemplate template, string directoryOption, string? locationOption, bool overwriteOption)
{
    private FileTemplate _template = template;
    private string _directoryOption = directoryOption;
    private string? _locationOption = locationOption;
    private bool _overwriteOption = overwriteOption;

    public void Analyze()
    {
        var templateDir = _locationOption.AsDirectoryInfo() ?? _template.RelativeFileLocation.GetDirectory(_directoryOption.AsDirectoryInfo()!) ?? throw new InvalidOperationException($"Could not find where to place file");
        Logger.LogInfo($"Creating template in {templateDir}");

        var file = templateDir.CombineFile(Consts.SolForgeConfig);
        if (!_overwriteOption && file.Exists)
        {
            Logger.LogError($"File {file} already exists.To overwrite it, use the {SolForgeMain.OverwriteOption.Aliases.First()} option to overwite the file.");
            return;
        }

        templateDir.Create();
        File.WriteAllText(file.FullName, _template.Template);
    }
}
