namespace LFC.Commandline;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public abstract class CommandlineAttribute : Attribute
{
    public readonly string? Description;
    public readonly string Name;
    public readonly string? ShortName;

    public CommandlineAttribute(string name, string? shortName = null, string? description = null)
    {
        Name = name;
        ShortName = shortName;
        Description = description;
    }
}