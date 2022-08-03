using LF.Compiler.C.Expressions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCParser : LFCodeParser
{
    private static readonly Dictionary<string, Func<LFCExpressionCompileInput, LFCType[], LFCExpression[], LFSourcePosition, LFCType>>
        s_CompilerFunctions =
            new Dictionary<string, Func<LFCExpressionCompileInput, LFCType[], LFCExpression[], LFSourcePosition, LFCType>>();

    static LFCParser()
    {
        RegisterCompilerFunction("unsafe_cast", LFCCompilerFunctions.UnsafeCast);
        RegisterCompilerFunction("sizeof", LFCCompilerFunctions.SizeOf);
        RegisterCompilerFunction("const_ctor", LFCCompilerFunctions.ConstConstructorCall);
        RegisterCompilerFunction("nameof", LFCCompilerFunctions.NameOf);
    }

    public static bool HasCompilerFunction(string name)
    {
        return s_CompilerFunctions.ContainsKey(name);
    }

    public static void RegisterCompilerFunction(
        string name,
        Func<LFCExpressionCompileInput, LFCType[], LFCExpression[], LFSourcePosition, LFCType> func)
    {
        s_CompilerFunctions.Add(name, func);
    }

    public static LFCType EmitCompilerFunction(
        string name,
        LFCExpressionCompileInput input,
        LFCType[] typeArgs,
        LFCExpression[] arguments,
        LFSourcePosition position)
    {
        return s_CompilerFunctions[name](input, typeArgs, arguments, position);
    }

    public static void AddParser()
    {
        AddParser<LFCParser>(".lfc");
    }

    protected override LFCodeReader CreateReader(string src, string workingDir, string fileName)
    {
        return new LFCReader(src, workingDir, fileName);
    }
}