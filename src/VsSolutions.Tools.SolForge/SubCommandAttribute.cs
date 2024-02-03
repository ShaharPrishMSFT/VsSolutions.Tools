namespace VsSolutions.Tools.SolForge;

using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class SubCommandAttribute(string commandName, string description, object state) : Attribute
{
    public string CommandName { get; } = commandName;

    public string Description { get; } = description;

    public object State { get; } = state;
}
