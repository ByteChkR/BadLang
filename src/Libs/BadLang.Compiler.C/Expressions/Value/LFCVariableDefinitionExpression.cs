using LF.Compiler.C.Expressions.Access;
using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.Value;

public class LFCVariableDefinitionExpression : LFCVariableExpression
{
    private readonly LFCExpression[]? ConstructorArgs;
    public readonly LFCTypeToken TypeName;

    public LFCVariableDefinitionExpression(string name, LFSourcePosition position, LFCTypeToken typeName, LFCExpression[]? constructorArgs) : base(name, position)
    {
        TypeName = typeName;
        ConstructorArgs = constructorArgs;
    }

    protected override LFCType? GetTypeHint(LFCExpressionCompileInput input)
    {
        return input.GetType(TypeName, Position);
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCType type = input.GetType(TypeName, Position);
        input.Scope.DefineOnStack(Name, type, input.Section, Position);

        if (type.HasConstructor && ConstructorArgs == null)
        {
            throw new LFCParserException("Type has constructor but no constructor arguments were provided", Position);
        }

        if (!type.HasConstructor && ConstructorArgs != null)
        {
            throw new LFCParserException("Type does not have constructor but constructor arguments were provided", Position);
        }

        LFCExpressionCompileResult output = base.Compile(input);
        if (ConstructorArgs != null)
        {
            LFCInvocationExpression invoc = new LFCInvocationExpression(
                Position,
                new LFCMemberAccessExpression(
                    Position,
                    new LFCVariableExpression(Name, Position),
                    ".ctor",
                    false
                ),
                ConstructorArgs,
                Array.Empty<LFCTypeToken>()
            );
            invoc.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false)).DiscardResult();
        }

        return output;
    }
}