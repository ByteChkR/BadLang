using BadAssembler;

using BadC.Expressions;
using BadC.Operators.Access;
using BadC.Operators.Assignment;
using BadC.Operators.Bitwise;
using BadC.Operators.Bitwise.Self;
using BadC.Operators.Comparison;
using BadC.Operators.Logic;
using BadC.Operators.Math;
using BadC.Operators.Math.Atomic;
using BadC.Operators.Math.Self;

using BadVM.Shared;

namespace BadC.Operators;

public abstract class BadCBinaryOperator
{

    protected readonly int Precedence;
    private readonly string m_Key;

    private static readonly List < BadCBinaryOperator > s_Operators = new List < BadCBinaryOperator >
                                                                      {
                                                                          new BadCAssignmentOperator(),
                                                                          new BadCDirectMemberAccessOperator(),
                                                                          new BadCIndirectMemberAccessOperator(),
                                                                          new BadCAddOperator(),
                                                                          new BadCSubOperator(),
                                                                          new BadCMulOperator(),
                                                                          new BadCDivOperator(),
                                                                          new BadCModOperator(),
                                                                          new BadCAddSelfOperator(),
                                                                          new BadCSubSelfOperator(),
                                                                          new BadCMulSelfOperator(),
                                                                          new BadCDivSelfOperator(),
                                                                          new BadCModSelfOperator(),
                                                                          new BadCPostFixDecrementOperator(),
                                                                          new BadCPostFixIncrementOperator(),
                                                                          new BadCShiftLeftOperator(),
                                                                          new BadCShiftRightOperator(),
                                                                          new BadCBitAndOperator(),
                                                                          new BadCBitOrOperator(),
                                                                          new BadCBitXOrOperator(),
                                                                          new BadCBitShiftLeftSelfOperator(),
                                                                          new BadCBitShiftRightSelfOperator(),
                                                                          new BadCBitAndSelfOperator(),
                                                                          new BadCBitOrSelfOperator(),
                                                                          new BadCBitXOrSelfOperator(),
                                                                          new BadCLogicAndOperator(),
                                                                          new BadCLogicOrOperator(),
                                                                          new BadCEqualityOperator(),
                                                                          new BadCInEqualityOperator(),
                                                                          new BadCLessThanOperator(),
                                                                          new BadCGreaterThanOperator(),
                                                                          new BadCLessOrEqualOperator(),
                                                                          new BadCGreaterOrEqualOperator()
                                                                      };

    #region Public

    public static BadCBinaryOperator? Get( int p, string key )
    {
        foreach ( BadCBinaryOperator op in s_Operators )
        {
            if ( op.Precedence <= p && op.m_Key == key )
            {
                return op;
            }
        }

        return null;
    }

    public abstract BadCExpression Parse(
        BadCEmitContext context,
        BadCExpression left,
        SourceReader reader,
        BadCCodeParser parser,
        SourceToken token );

    #endregion

    #region Protected

    protected BadCBinaryOperator( int precedence, string key )
    {
        Precedence = precedence;
        m_Key = key;
    }

    #endregion

}
