using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Internal;

public class LFCCompilerFunctionExpression : LFCExpression
{
    private readonly LFCExpression[] m_Parameters;
    private readonly LFCTypeToken[] m_TypeArgs;
    public readonly string Name;


    public LFCCompilerFunctionExpression(string name, LFCExpression[] parameters, LFCTypeToken[] typeArgs, LFSourcePosition position) : base(position)
    {
        Name = name;
        m_Parameters = parameters;
        m_TypeArgs = typeArgs;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCType type = LFCParser.EmitCompilerFunction(
            Name,
            input,
            m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray(),
            m_Parameters,
            Position
        );

        return input.CreateResult(type, Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        return new LFCCompilerFunctionExpression(
            Name,
            m_Parameters.Select(rebuilder).ToArray(),
            m_TypeArgs,
            Position
        );
    }
}