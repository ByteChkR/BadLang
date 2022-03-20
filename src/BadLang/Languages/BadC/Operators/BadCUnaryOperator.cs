using BadAssembler;

using BadC.Expressions;
using BadC.Operators.Math.Atomic;
using BadC.Operators.Pointer;

using BadVM.Shared;

namespace BadC.Operators;

public abstract class BadCUnaryOperator
{

    public readonly int Precedence;
    public readonly string Key;

    private static readonly List < BadCUnaryOperator > s_Operators =
        new List < BadCUnaryOperator >
        {
            new BadCAddressOfOperator(),
            new BadCValueOfOperator(),
            new BadCPreFixIncrementOperator(),
            new BadCPreFixDecrementOperator(),
        };

    #region Public

    public static BadCUnaryOperator? Get( int p, string key )
    {
        foreach ( BadCUnaryOperator op in s_Operators )
        {
            if ( op.Precedence <= p && op.Key == key )
            {
                return op;
            }
        }

        return null;
    }

    public abstract BadCExpression Parse(
        BadCEmitContext context,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token );

    #endregion

    #region Protected

    protected BadCUnaryOperator( int precedence, string key )
    {
        Precedence = precedence;
        Key = key;
    }

    #endregion

}
