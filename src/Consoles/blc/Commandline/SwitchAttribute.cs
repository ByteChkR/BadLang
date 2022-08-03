namespace LFC.Commandline;

public class SwitchAttribute : CommandlineAttribute
{
    public SwitchAttribute(string name, string? shortName = null, string? description = null) : base(name, shortName, description) { }
}