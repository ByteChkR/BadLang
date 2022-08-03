namespace LF.Compiler.DataObjects;

public class LFAssemblyPatchTarget
{
    public readonly string Name;
    public readonly int Offset;

    public LFAssemblyPatchTarget(string name, int offset)
    {
        Name = name;
        Offset = offset;
    }

    public override string ToString()
    {
        return $"{Name} at {Offset}";
    }
}