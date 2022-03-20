using BadC.Expressions;
using BadC.Types.Primitives;

using BadVM.Shared;

namespace BadC.Types;

public static class BadCTypeChecker
{

    #region Public

    public static BadCType? CheckTypes(
        SourceToken token,
        BadCType? left,
        BadCType? right,
        BadCType? baseTypeHint )
    {
        if ( left != null )
        {
            if ( right != null &&
                 left != right )
            {
                throw new TypeMismatchException(
                                                right!,
                                                left!,
                                                token
                                               );
            }
            else
            {
                baseTypeHint = left!;
            }
        }
        else if ( right != null )
        {
            baseTypeHint = right!;
        }

        return baseTypeHint;
    }

    public static BadCType? CheckTypes(
        this BadCEmitContext context,
        SourceToken token,
        BadCExpression left,
        BadCExpression right,
        BadCType? baseTypeHint )
    {
        return CheckTypes( token, left.GetFixedType( context ), right.GetFixedType( context ), baseTypeHint );
    }

    public static BadCType CheckTypes(
        this BadCEmitContext context,
        SourceToken token,
        BadCExpression left,
        BadCExpression right )
    {
        BadCType baseTypeHint = BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

        return context.CheckTypes( token, left, right, baseTypeHint )!;
    }

    #endregion

}
