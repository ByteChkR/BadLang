namespace LFC.Commandline;

public class OptionAttribute : CommandlineAttribute
{
    public OptionAttribute(string name, string? shortName = null, string? description = null) : base(name, shortName, description) { }
}