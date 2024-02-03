namespace VsSolutions.Tools.SolForge.ProjectSystem;
using System;
using System.Linq;
using VsSolutions.Tools.SolForgeSolForge;

internal enum RelativeFileLocation
{
    ClosestSolutionRoot,
    SolForgeConfig
}

internal static class FileLocationExtensions
{
    public static DirectoryInfo? GetDirectory(this RelativeFileLocation location, DirectoryInfo currentDirectory)
        => location switch
        {
            RelativeFileLocation.ClosestSolutionRoot => ((DirectoryInfo?)FindFirstFileUp("*.sln", currentDirectory, true)) ?? FindFirstDirUp(".git", currentDirectory, true),
            RelativeFileLocation.SolForgeConfig => GetDirectory(RelativeFileLocation.ClosestSolutionRoot, currentDirectory) is DirectoryInfo info ? info.CombineDirectory(Consts.SolForgeDir) : null,
            _ => throw new InvalidOperationException($"Unknown location {location}"),
        };

    private static DirectoryInfo? FindFirstDirUp(string fileSpec, DirectoryInfo currentDirectory, bool returnParentDirectory)
    {
        return GoUpAndFind(currentDirectory, d => GetFirstDirectory(d, returnParentDirectory));

        DirectoryInfo? GetFirstDirectory(DirectoryInfo root, bool returnDirectory)
        {
            var dir = Directory.EnumerateDirectories(root.FullName, fileSpec).FirstOrDefault();

            if (dir == null)
            {
                return null;
            }

            if (returnDirectory)
            {
                dir = Path.GetDirectoryName(dir);
            }

            return new DirectoryInfo(dir!);
        }
    }

    private static FileSystemInfo? FindFirstFileUp(string fileSpec, DirectoryInfo currentDirectory, bool returnParentDirectory)
    {
        return GoUpAndFind(currentDirectory, d => GetFirstFileOrDirectory(d, returnParentDirectory));

        FileSystemInfo? GetFirstFileOrDirectory(DirectoryInfo root, bool returnDirectory)
        {
            var file = Directory.EnumerateFiles(root.FullName, fileSpec).FirstOrDefault();

            if (file == null)
            {
                return null;
            }

            if (returnDirectory)
            {
                return new DirectoryInfo(Path.GetDirectoryName(file)!);
            }

            return new FileInfo(file);
        }
    }

    private static T? GoUpAndFind<T>(DirectoryInfo currentDirectory, Func<DirectoryInfo, T?> getResult)
        where T : FileSystemInfo
    {
        DirectoryInfo? dir = currentDirectory;
        while (dir != null)
        {
            var result = getResult(dir);
            if (result != null)
            {
                return result;
            }

            dir = dir.Parent;
        }

        return null;
    }
}