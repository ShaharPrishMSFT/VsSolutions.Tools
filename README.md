# VsSolutions.Tools

# SolForge - Visual Studio Solution Optimizer

## Introduction

SolForge is a command-line tool designed for optimizing Visual Studio solutions. It currently only provides the ability to modify a project for Central Package Management.

## Installation

SolForge is a .NET tool and can be installed via the .NET CLI.

### Local (solutuion) installation
To install the tool locally as part of the solution you are in:
```shell
dotnet tool install vssolutions.tools.solforge
```
To use from the command line:

```shell
dotnet solforge -?
```

### Global installation
To install the tool globaly, run this command:

```shell
dotnet tool install -g vssolutions.tools.solforge
```
To use from the command line:

```shell
solforge -?
```


## Features

- **Central Package Management**: Automates the transition of Visual Studio solutions to use NuGet's central package management system. This feature enhances the maintainability and consistency of package versions across multiple projects in a solution.

## Why Central Package Management?

Migrating to a central package management system offers several benefits:
1. **Consistency**: Ensures consistent use of package versions across all projects in a solution.
2. **Ease of Updates**: Simplifies the process of updating package versions, as changes are made centrally.
3. **Reduced Merge Conflicts**: Minimizes the risk of merge conflicts in project files when updating package versions.

## Suitable Repositories

Central package management is particularly beneficial for:
- Solutions with multiple projects.
- Solutions where consistency in dependencies is crucial.
- Environments where streamlined dependency management is desired.


## Usage

### Central Management Command

Modify the solution at the current directory to use central package management by scanning all projects in the solution and generating a central package management file.

```
dotnet solforge centralmgmt -a
```


Options
-a, --apply: Apply changes to the location where the solution is located (not passing -a will generate the files in a temporary location)
-d, --directory <directory>: Specify the directory to work in.
-?, -h, --help: Show help and usage information.
