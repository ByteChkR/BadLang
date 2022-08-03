using LF.Compiler.Reader;

namespace LF.Compiler.C.Functions;

public class LFCFunctionTable
{
    private static readonly Dictionary<LFReaderCodeSection, LFCFunctionTable> s_FunctionTables =
        new Dictionary<LFReaderCodeSection, LFCFunctionTable>();

    private readonly LFReaderCodeSection m_CodeSection;
    private readonly List<LFCFunction> m_Functions = new List<LFCFunction>();
    private readonly LFReaderResult m_Result;

    public LFCFunctionTable(LFReaderResult result, LFReaderCodeSection codeSection)
    {
        m_Result = result;
        m_CodeSection = codeSection;
    }

    public static LFCFunctionTable GetFunctionTable(LFReaderCodeSection section, LFReaderResult result)
    {
        if (!s_FunctionTables.ContainsKey(section))
        {
            s_FunctionTables[section] = new LFCFunctionTable(result, section);
        }

        return s_FunctionTables[section];
    }

    private IEnumerable<(string fullName, LFCFunction func)> GetAllFunctions()
    {
        foreach (LFCFunction func in m_Functions)
        {
            yield return (func.Name, func);
        }

        foreach (LFReaderCodeSection section in m_Result.CodeSections)
        {
            if (m_CodeSection.HasImport(section.Name))
            {
                LFCFunctionTable fTable = GetFunctionTable(section, m_Result);
                foreach (LFCFunction function in fTable.m_Functions)
                {
                    if (section.HasExport(function.Name))
                    {
                        yield return ($"{section.Name}::{function.Name}", function);
                    }
                }
            }
        }
    }

    public void AddFunction(LFCFunction func)
    {
        m_Functions.Add(func);
    }

    public bool HasFunction(string name)
    {
        return GetAllFunctions().Any(x => x.fullName == name);
    }

    public LFCFunction GetFunction(string name, LFSourcePosition position)
    {
        foreach ((string fullName, LFCFunction func) in GetAllFunctions())
        {
            if (name == fullName)
            {
                return func;
            }
        }

        throw new LFCParserException($"Function not found: {name}", position);
    }
}