using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Math.Atomic;

using BadVM.Shared;

namespace BadC.Operators.Math.Atomic;

public class BadCPreFixDecrementOperator : BadCUnaryOperator
{

    #region Public

    public BadCPreFixDecrementOperator() : base( 3, "--" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        {
            BadCExpression left = parser.ParseExpression( reader, context, null, Precedence );

            return new BadCPreFixDecrementExpression( left, token );
        }
    }

    #endregion

}
