using LF.Compiler.C.Types;

namespace LF.Compiler.C.Functions;

public class LFCFunctionParameter
{
    public readonly string Name;
    public readonly LFCTypeToken TypeName;

    public LFCFunctionParameter(string name, LFCTypeToken type)
    {
        Name = name;
        TypeName = type;
    }

    private static LFCTypeToken GetTypeName(LFCTypeToken type, List<LFCTypeToken> typeParameters, LFCType[] typeParams)
    {
        int index = typeParameters.IndexOf(type);

        if (index != -1)
        {
            return typeParams[index].Name;
        }

        LFCTypeToken[] typeArgs = new LFCTypeToken[type.TypeArgs.Length];
        for (int i = 0; i < type.TypeArgs.Length; i++)
        {
            typeArgs[i] = GetTypeName(type.TypeArgs[i], typeParameters, typeParams);
        }

        return new LFCTypeToken(type.Name, typeArgs);
    }

    public string ToString(List<LFCTypeToken> typeParameters, LFCType[] typeParams)
    {
        return $"{GetTypeName(TypeName, typeParameters, typeParams)} {Name}";
    }

    public override string ToString()
    {
        return $"{TypeName} {Name}";
    }
}