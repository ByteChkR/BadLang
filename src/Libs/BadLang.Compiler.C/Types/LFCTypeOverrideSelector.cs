using LF.Compiler.C.Functions;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public class LFCTypeOverrideSelector
{
    //If false, the order of the types does not matter
    public readonly bool IsPositionDependent;
    public readonly LFCType LeftType;
    public readonly LFCType LeftTypeOverride;
    public readonly string OperatorName;
    public readonly LFCType RightType;

    public LFCTypeOverrideSelector(LFCType leftType, LFCType rightType, bool isPositionDependent, string operatorName, LFCType? leftTypeOverride = null)
    {
        LeftType = leftType;
        LeftTypeOverride = leftTypeOverride ?? leftType;
        RightType = rightType;
        IsPositionDependent = isPositionDependent;
        OperatorName = operatorName;
    }

    public LFCFunctionType GetOverride(LFSourcePosition position, out bool isReverseOrder)
    {
        isReverseOrder = false;
        if (LeftTypeOverride.HasOverride(OperatorName, LeftType, RightType))
        {
            return LeftTypeOverride.GetOverride(position, OperatorName, LeftType, RightType);
        }

        if (RightType.HasOverride(OperatorName, LeftType, RightType))
        {
            return RightType.GetOverride(position, OperatorName, LeftType, RightType);
        }

        if (!IsPositionDependent)
        {
            isReverseOrder = true;
            if (LeftTypeOverride.HasOverride(OperatorName, RightType, LeftType))
            {
                return LeftTypeOverride.GetOverride(position, OperatorName, RightType, LeftType);
            }

            if (RightType.HasOverride(OperatorName, RightType, LeftType))
            {
                return RightType.GetOverride(position, OperatorName, RightType, LeftType);
            }
        }

        throw new LFCParserException($"No override found for operator {OperatorName} on types {LeftType.Name} and {RightType.Name}", position);
    }
}