// <copyright file="Consts.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge;
internal class Consts
{
    public const string BuildProps = "Directory.Build.props";
    public const string PackagesProps = "Directory.Packages.props";

    public const string CsProjExtension = ".csproj";

    public const string DirectoryPackagesPropsTemplate = """
<Project>
    <!--For more info on shared project properties, see https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management-->
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
</Project>
""";

    public const string ContributeToProject = "This project was modified by VsSolutions.Tools.SolForgeSolForge (https://dev.azure.com/microsoft/WDATP/_git/TVM.Immune.Tools)";
}
