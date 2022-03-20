using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math;

using BadVM.Shared;

namespace BadC.Operators.Math;

public class BadCAddOperator : BadCBinaryOperator
{

    #region Public

    public BadCAddOperator() : base( 6, "+" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        BadCExpression right = parser.ParseExpression( reader, context, null, Precedence );

        return new BadCAddExpression( left, right, token );
    }

    #endregion

}
