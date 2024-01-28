// <copyright file="PackageName.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace VsSolutions.Tools.SolForgeSolForge.ProjectSystem;
using System;

// A string wrapper for package names - compares as case-insensitive
internal struct PackageName(string name) : IEquatable<PackageName>, IComparable<PackageName>
{
    public static PackageName Empty { get; } = new PackageName(string.Empty);

    public string Name { get; } = name;

    public static PackageName FromString(string name) => new PackageName(name);

    public bool Equals(PackageName other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is PackageName other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    public static bool operator ==(PackageName left, PackageName right) => left.Equals(right);

    public static bool operator !=(PackageName left, PackageName right) => !left.Equals(right);

    public static bool operator >(PackageName left, PackageName right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) > 0;

    public static bool operator <(PackageName left, PackageName right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) < 0;

    public static bool operator >=(PackageName left, PackageName right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) >= 0;

    public static bool operator <=(PackageName left, PackageName right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) <= 0;

    public override string ToString() => Name;

    public int CompareTo(PackageName other) => string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
}
