using BadAssembler;

using BadC.Expressions;
using BadC.Expressions.Pointer;

using BadVM.Shared;

namespace BadC.Operators.Pointer;

public class BadCAddressOfOperator : BadCUnaryOperator
{

    #region Public

    public BadCAddressOfOperator() : base( 3, "&" )
    {
    }

    public override BadCExpression Parse(
        BadCEmitContext context,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token )
    {
        return new BadCAddressOfExpression( parser.ParseExpression( reader, context, null, Precedence ), token );
    }

    #endregion

}
