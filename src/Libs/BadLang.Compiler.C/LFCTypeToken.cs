using LF.Compiler.C.Types;

namespace LF.Compiler.C;

public struct LFCTypeToken : IEquatable<LFCTypeToken>
{
    public string Name;
    public LFCTypeToken[] TypeArgs;

    public override string ToString()
    {
        string name = LFCTypeDatabase.Parse(this, out int pointerLevel);

        return name + (TypeArgs.Length != 0 ? "<" + string.Join(", ", TypeArgs) + ">" : "") + new string('*', pointerLevel);
    }

    public LFCTypeToken(string name, LFCTypeToken[] typeArgs)
    {
        Name = name;
        TypeArgs = typeArgs;
    }

    public bool Equals(LFCTypeToken other)
    {
        if (Name == other.Name && TypeArgs.Length == other.TypeArgs.Length)
        {
            for (int i = 0; i < TypeArgs.Length; i++)
            {
                if (TypeArgs[i] != other.TypeArgs[i])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        return obj is LFCTypeToken other && Equals(other);
    }

    public override int GetHashCode()
    {
        int current = Name.GetHashCode();
        foreach (LFCTypeToken typeArg in TypeArgs)
        {
            current ^= typeArg.GetHashCode();
        }

        return current;
    }

    public static bool operator ==(LFCTypeToken left, LFCTypeToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LFCTypeToken left, LFCTypeToken right)
    {
        return !(left == right);
    }
}