namespace LF.Compiler.DataObjects.Code;

public class LFAssemblyPatchSite : IEquatable<LFAssemblyPatchSite>
{
    public readonly int InstructionOffset;
    public readonly int PatchSize;

    public LFAssemblyPatchSite(int instructionOffset, int patchSize)
    {
        InstructionOffset = instructionOffset;
        PatchSize = patchSize;
    }

    public bool Equals(LFAssemblyPatchSite? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return InstructionOffset == other.InstructionOffset && PatchSize == other.PatchSize;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((LFAssemblyPatchSite)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InstructionOffset, PatchSize);
    }

    public static bool operator ==(LFAssemblyPatchSite? left, LFAssemblyPatchSite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LFAssemblyPatchSite? left, LFAssemblyPatchSite? right)
    {
        return !Equals(left, right);
    }
}