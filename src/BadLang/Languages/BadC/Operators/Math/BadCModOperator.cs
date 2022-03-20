using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math;

using BadVM.Shared;

namespace BadC.Operators.Math;

public class BadCModOperator : BadCBinaryOperator
{

    #region Public

    public BadCModOperator() : base( 5, "%" )
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

        return new BadCModExpression( left, right, token );
    }

    #endregion

}
