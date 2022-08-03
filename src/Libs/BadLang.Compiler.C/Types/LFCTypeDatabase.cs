using LF.Compiler.C.Functions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public class LFCTypeDatabase
{
    private static readonly Dictionary<LFReaderResult, LFCTypeDatabase> s_TypeSystems =
        new Dictionary<LFReaderResult, LFCTypeDatabase>();

    private readonly List<LFCReaderStructureType> m_Structures = new List<LFCReaderStructureType>();

    private readonly List<LFCType> m_Types = new List<LFCType>();

    public LFCTypeDatabase()
    {
        m_Types.AddRange(LFCBaseType.BaseTypes);
    }

    public IEnumerable<LFCType> Types => m_Types;

    public static LFCTypeDatabase GetTypeSystem(LFReaderResult result)
    {
        if (!s_TypeSystems.ContainsKey(result))
        {
            s_TypeSystems[result] = new LFCTypeDatabase();
            result.AfterFinalize(result.EmitAssemblyManifest);
        }

        return s_TypeSystems[result];
    }

    public void AddType(LFCReaderStructureType type)
    {
        m_Structures.Add(type);
    }

    public void AddType(LFCType type)
    {
        m_Types.Add(type);
    }

    public static string Parse(LFCTypeToken typeToken, out int pointerLevel)
    {
        string type = typeToken.Name;
        pointerLevel = 0;
        while (type.EndsWith("*"))
        {
            type = type.Substring(0, type.Length - 1);
            pointerLevel++;
        }

        return type;
    }

    public LFCType GetType(string name, LFSourcePosition position)
    {
        return GetType(new LFCTypeToken(name, Array.Empty<LFCTypeToken>()), position);
    }

    public LFCType GetType(LFCTypeToken name, LFSourcePosition position)
    {
        string typeName = Parse(name, out int pointerLevel);
        if (typeName == "Function")
        {
            LFCTypeToken returnType = name.TypeArgs.First();
            LFCTypeToken[] argTypes = name.TypeArgs.Skip(1).ToArray();

            return new LFCFunctionType(returnType, argTypes, Array.Empty<LFCTypeToken>());
        }

        LFCTypeToken typeToken = new LFCTypeToken(typeName, name.TypeArgs);

        LFCType? type = m_Types.FirstOrDefault(t => t.Name == typeToken);
        if (type == null)
        {
            LFCReaderStructureType? structType = m_Structures.FirstOrDefault(x => x.Name.Name == typeName);
            if (structType == null)
            {
                throw new LFCParserException($"Type {typeName} not found", position);
            }

            if (structType.Name.TypeArgs.Length != name.TypeArgs.Length)
            {
                throw new LFCParserException("Type args mismatch", position);
            }

            Dictionary<LFCTypeToken, LFCTypeToken> typeMap = new Dictionary<LFCTypeToken, LFCTypeToken>();
            for (int i = 0; i < name.TypeArgs.Length; i++)
            {
                typeMap.Add(structType.Name.TypeArgs[i], name.TypeArgs[i]);
            }

            type = structType.CreateType(this, typeMap);
        }

        for (int i = 0; i < pointerLevel; i++)
        {
            type = LFCPointerType.Create(type);
        }

        return type;
    }
}