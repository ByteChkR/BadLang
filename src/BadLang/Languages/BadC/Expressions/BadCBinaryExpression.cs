using BadC.Types;

using BadVM.Shared;

namespace BadC.Expressions;

public abstract class BadCBinaryExpression : BadCExpression
{

    protected BadCExpression Left { get; }

    protected BadCExpression Right { get; }

    #region Public

    protected BadCBinaryExpression( BadCExpression left, BadCExpression right, SourceToken token ) : base( false, token )
    {
        Left = left;
        Right = right;
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return context.CheckTypes( SourceToken, Left, Right, null );
    }

    #endregion

}
