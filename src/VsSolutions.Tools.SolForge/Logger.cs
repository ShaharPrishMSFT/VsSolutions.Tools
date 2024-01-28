// <copyright file="Logger.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge;
using System;

internal class Logger
{
    public static void LogImportant(string message)
        => Log(message);

    public static void LogInfo(string message)
        => Log(message);

    public static void LogError(string message)
        => WriteColorLine(ConsoleColor.Red, () => Log($"Error: {message}"));

    public static void LogWarning(string message)
        => WriteColorLine(ConsoleColor.Yellow, () => Log($"Warning: {message}"));

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }

    private static void WriteColorLine(ConsoleColor color, Action write)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        write();
        Console.ForegroundColor = oldColor;
    }
}
