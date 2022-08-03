using LF.Compiler.C.Operators.Access;
using LF.Compiler.C.Operators.Bitwise;
using LF.Compiler.C.Operators.Bitwise.Self;
using LF.Compiler.C.Operators.Branches;
using LF.Compiler.C.Operators.Comparison;
using LF.Compiler.C.Operators.Logic;
using LF.Compiler.C.Operators.Math;
using LF.Compiler.C.Operators.Math.Self;
using LF.Compiler.C.Operators.Math.Self.Atomic;
using LF.Compiler.C.Operators.Pointers;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Operators;

public static class LFCOperatorTable
{
    private static readonly List<LFCOperator> s_Operators = new List<LFCOperator>
    {
        new LFCAssignOperator(),
        new LFCAddOperator(),
        new LFCSubtractOperator(),
        new LFCMultiplyOperator(),
        new LFCDivideOperator(),
        new LFCModuloOperator(),
        new LFCPointerDereferenceOperator(),
        new LFCPointerReferenceOperator(),
        new LFCAndOperator(),
        new LFCOrOperator(),
        new LFCXorOperator(),
        new LFCShiftLeftOperator(),
        new LFCShiftRightOperator(),
        new LFCEqualOperator(),
        new LFCNotEqualOperator(),
        new LFCLessThanOperator(),
        new LFCLessOrEqualOperator(),
        new LFCGreaterThanOperator(),
        new LFCGreaterOrEqualOperator(),
        new LFCNotOperator(),
        new LFCInvertOperator(),
        new LFCMemberAccessOperator(),
        new LFCPointerMemberAccessOperator(),
        new LFCAddSelfOperator(),
        new LFCSubtractSelfOperator(),
        new LFCMultiplySelfOperator(),
        new LFCDivideSelfOperator(),
        new LFCModuloSelfOperator(),
        new LFCAndSelfOperator(),
        new LFCOrSelfOperator(),
        new LFCXorSelfOperator(),
        new LFCShiftLeftSelfOperator(),
        new LFCShiftRightSelfOperator(),
        new LFCLogicAndOperator(),
        new LFCLogicOrOperator(),
        new LFCPreValueIncrementOperator(),
        new LFCPostValueIncrementOperator(),
        new LFCPreValueDecrementOperator(),
        new LFCPostValueDecrementOperator(),
        new LFCTernaryOperator(),
    };

    public static bool HasPreValueOperator(string symbol)
    {
        return s_Operators.Any(o => o is LFCPreValueOperator && o.Symbol == symbol);
    }

    public static bool HasPreValueOperator(string symbol, int precedence)
    {
        return s_Operators.Any(o => o is LFCPreValueOperator && o.Symbol == symbol && o.Precedence <= precedence);
    }

    public static bool HasPostValueOperator(string symbol)
    {
        return s_Operators.Any(o => o is LFCPostValueOperator && o.Symbol == symbol);
    }

    public static bool HasPostValueOperator(string symbol, int precedence)
    {
        return s_Operators.Any(o => o is LFCPostValueOperator && o.Symbol == symbol && o.Precedence <= precedence);
    }

    public static bool HasOperator(string symbol)
    {
        return s_Operators.Any(o => o.Symbol == symbol);
    }

    public static bool HasOperator(string symbol, int precedence)
    {
        return s_Operators.Any(o => o.Symbol == symbol && o.Precedence <= precedence);
    }

    public static LFCPostValueOperator GetPostValueOperator(string name, LFSourcePosition position)
    {
        foreach (LFCOperator op in s_Operators)
        {
            if (op is LFCPostValueOperator postOp && postOp.Symbol == name)
            {
                return postOp;
            }
        }

        throw new LFCParserException($"No post value operator with name {name}", position);
    }

    public static LFCPreValueOperator GetPreValueOperator(string name, int precedence, LFSourcePosition position)
    {
        foreach (LFCOperator op in s_Operators)
        {
            if (op is LFCPreValueOperator preOp && preOp.Symbol == name && preOp.Precedence <= precedence)
            {
                return preOp;
            }
        }

        throw new LFCParserException($"No pre value operator with name {name} and precedence {precedence}", position);
    }
}