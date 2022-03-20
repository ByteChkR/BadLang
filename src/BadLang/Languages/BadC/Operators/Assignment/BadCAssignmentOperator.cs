using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Assignment;

using BadVM.Shared;

namespace BadC.Operators.Assignment;

public class BadCAssignmentOperator : BadCBinaryOperator
{

    #region Public

    public BadCAssignmentOperator() : base( 16, "=" )
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

        return new BadCAssignmentExpression( left, right, token );
    }

    #endregion

}
