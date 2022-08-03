using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Math;

namespace LF.Compiler.C.Operators.Math;

public class LFCModuloOperator : LFCPostValueOperator
{
    public LFCModuloOperator() : base(5, "%") { }

    public override LFCExpression Parse(LFCPostValueOperatorParseInput input)
    {
        return new LFCModuloExpression(input.Left, input.Reader.ReadExpression(input.Result, input.CodeSection, null, Precedence));
    }
}