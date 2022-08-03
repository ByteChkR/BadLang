using LF.Compiler.C.Expressions.Loops;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Branches;

public class LFCSwitchExpression : LFCExpression
{
    private readonly Dictionary<LFCExpression, LFCExpression[]> m_CaseLabels;

    private readonly LFCExpression m_CaseValue;
    private readonly LFCExpression[]? m_DefaultCase;

    public LFCSwitchExpression(
        LFSourcePosition position,
        LFCExpression caseValue,
        Dictionary<LFCExpression, LFCExpression[]> caseLabels,
        LFCExpression[]? defaultCase) : base(position)
    {
        m_CaseLabels = caseLabels;
        m_DefaultCase = defaultCase;
        m_CaseValue = caseValue;
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCExpressionCompileResult valueResult = m_CaseValue.Compile(input.CreateInput(isLValue: false, typeHint: null));

        string endLabel = input.Section.MakeUniqueLabel("__LFC__~SWITCH__END~__LABEL__");
        string codeLabel = input.Section.MakeUniqueLabel("__LFC__~SWITCH__CODE~__LABEL__");
        int i = 0;
        foreach (KeyValuePair<LFCExpression, LFCExpression[]> branch in m_CaseLabels)
        {
            string nextLabel = input.Section.MakeUniqueLabel("__LFC__~SWITCH__NEXT~__LABEL__");

            input.Section.Dup(valueResult.ReturnType.GetSize(), Position);

            //Condition
            LFCExpressionCompileResult conditionResult = branch.Key.Compile(
                new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, valueResult.ReturnType)
            );

            input.Section.TestEq(valueResult.ReturnType.GetSize(), Position);

            //IfZero => Jump Next
            input.Section.Emit(Position, OpCodes.Push64, nextLabel);
            switch (conditionResult.ReturnType.GetSize())
            {
                case 1:

                    input.Section.Emit(Position, OpCodes.JumpZero8);

                    break;
                case 2:

                    input.Section.Emit(Position, OpCodes.JumpZero16);

                    break;
                case 4:

                    input.Section.Emit(Position, OpCodes.JumpZero32);

                    break;
                case 8:

                    input.Section.Emit(Position, OpCodes.JumpZero64);

                    break;
                default:
                    throw new LFCParserException($"Invalid size {conditionResult.ReturnType.GetSize()}", Position);
            }

            LFCChildScope scope = input.Scope.CreateChildScope(Position, null, (i,p) => LFCControlFlow.Break(endLabel, i, p));

            input.Section.CreateLabel(codeLabel + "__BLOCK__" + i, Position);
            
            //Block
            foreach (LFCExpression expression in branch.Value)
            {
                expression.Compile(new LFCExpressionCompileInput(input.Result, input.Section, scope, false))
                    .DiscardResult();
            }

            scope.Release(input.Section);

            if (i == m_CaseLabels.Count - 1)
            {
                //Jump End
                input.Section.Emit(Position, OpCodes.Push64, endLabel);
                input.Section.Emit(Position, OpCodes.Jump);
            }
            else
            {
                input.Section.Emit(Position, OpCodes.Push64, codeLabel + "__BLOCK__" + (i + 1));
                input.Section.Emit(Position, OpCodes.Jump);
            }
            //Next
            input.Section.CreateLabel(nextLabel, Position);
            i++;
        }

        if (m_DefaultCase != null)
        {
            LFCChildScope scope = input.Scope.CreateChildScope(Position, null,(i,p) => LFCControlFlow.Break(endLabel, i, p));
            foreach (LFCExpression expression in m_DefaultCase)
            {
                expression.Compile(new LFCExpressionCompileInput(input.Result, input.Section, scope, false))
                    .DiscardResult();
            }
            scope.Release(input.Section);
        }

        input.Section.CreateLabel(endLabel, Position);

        return input.CreateResult(LFCBaseType.GetType("void"), Position);
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        Dictionary<LFCExpression, LFCExpression[]> caseLabels = new Dictionary<LFCExpression, LFCExpression[]>();
        foreach (KeyValuePair<LFCExpression, LFCExpression[]> pair in m_CaseLabels)
        {
            caseLabels.Add(rebuilder(pair.Key), pair.Value.Select(rebuilder).ToArray());
        }

        return new LFCSwitchExpression(Position, rebuilder(m_CaseValue), caseLabels, m_DefaultCase?.Select(rebuilder).ToArray());
    }
}

public class LFCIfBranchExpression : LFCExpression
{
    private readonly Dictionary<LFCExpression, LFCExpression[]> m_Branches;
    private readonly LFCExpression[]? m_ElseBranch;


    public LFCIfBranchExpression(LFSourcePosition position, Dictionary<LFCExpression, LFCExpression[]> branches, LFCExpression[]? elseBranch) : base(position)
    {
        m_Branches = branches;
        m_ElseBranch = elseBranch;
    }

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        Dictionary<LFCExpression, LFCExpression[]> branches = new Dictionary<LFCExpression, LFCExpression[]>();
        foreach (KeyValuePair<LFCExpression, LFCExpression[]> pair in m_Branches)
        {
            branches.Add(rebuilder(pair.Key), pair.Value.Select(rebuilder).ToArray());
        }

        LFCExpression[]? elseBranch = m_ElseBranch?.Select(rebuilder).ToArray();

        return new LFCIfBranchExpression(Position, branches, elseBranch);
    }


    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        string endLabel = input.Section.MakeUniqueLabel("__LFC__~IF__END~__BRANCH__");
        foreach (KeyValuePair<LFCExpression, LFCExpression[]> branch in m_Branches)
        {
            string nextLabel = input.Section.MakeUniqueLabel("__LFC__~IF__NEXT~__BRANCH__");

            //Condition
            LFCExpressionCompileResult conditionResult = branch.Key.Compile(
                new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)
            );

            //IfZero => Jump Next
            input.Section.Emit(Position, OpCodes.Push64, nextLabel);
            switch (conditionResult.ReturnType.GetSize())
            {
                case 1:

                    input.Section.Emit(Position, OpCodes.JumpZero8);

                    break;
                case 2:

                    input.Section.Emit(Position, OpCodes.JumpZero16);

                    break;
                case 4:

                    input.Section.Emit(Position, OpCodes.JumpZero32);

                    break;
                case 8:

                    input.Section.Emit(Position, OpCodes.JumpZero64);

                    break;
                default:
                    throw new LFCParserException($"Invalid size {conditionResult.ReturnType.GetSize()}", Position);
            }

            LFCChildScope scope = input.Scope.CreateChildScope(Position);

            //Block
            foreach (LFCExpression expression in branch.Value)
            {
                expression.Compile(new LFCExpressionCompileInput(input.Result, input.Section, scope, false))
                    .DiscardResult();
            }

            scope.Release(input.Section);

            //Jump End
            input.Section.Emit(Position, OpCodes.Push64, endLabel);
            input.Section.Emit(Position, OpCodes.Jump);

            //Next
            input.Section.CreateLabel(nextLabel, Position);
        }

        if (m_ElseBranch != null)
        {
            LFCChildScope scope = input.Scope.CreateChildScope(Position);
            foreach (LFCExpression expression in m_ElseBranch)
            {
                expression.Compile(new LFCExpressionCompileInput(input.Result, input.Section, scope, false))
                    .DiscardResult();
            }
            scope.Release(input.Section);
        }

        input.Section.CreateLabel(endLabel, Position);

        return input.CreateResult(LFCBaseType.GetType("void"), Position);
    }
}