namespace LFC.Commandline;

public class EnumOptionAttribute : OptionAttribute
{
    public readonly bool AllowMultiple;

    public EnumOptionAttribute(string name, bool allowMultiple, string? shortName = null, string? description = null) : base(name, shortName, description)
    {
        AllowMultiple = allowMultiple;
    }
}