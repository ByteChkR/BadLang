using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Value;

namespace LF.Compiler.C;

public class LFCPreProcessorMacro
{
    private readonly string[] m_MacroArguments;
    private readonly LFCExpression m_MacroContent;

    public LFCPreProcessorMacro(string[] macroArguments, LFCExpression macroContent, bool isBatchMacro = false)
    {
        m_MacroArguments = macroArguments;
        m_MacroContent = macroContent;
        IsBatchMacro = isBatchMacro;
    }

    public bool IsBatchMacro { get; }

    public LFCExpression PrepareExpression(LFCExpression[] arguments)
    {
        return PrepareExpression(m_MacroContent, arguments);
    }

    private LFCExpression PrepareExpression(LFCExpression expr, LFCExpression[] arguments)
    {
        if (expr is LFCVariableExpression vExpr)
        {
            int i = 0;
            for (; i < m_MacroArguments.Length; i++)
            {
                string macroArgument = m_MacroArguments[i];

                if (vExpr.Name == macroArgument)
                {
                    break;
                }
            }

            if (i < m_MacroArguments.Length)
            {
                return arguments[i];
            }

            return expr;
        }

        return expr.Rebuild(e => PrepareExpression(e, arguments));
    }
}