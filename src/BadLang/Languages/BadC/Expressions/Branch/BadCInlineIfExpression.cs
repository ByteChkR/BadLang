using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;
using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Branch;

public class BadCInlineIfExpression : BadCExpression
{
    public readonly BadCExpression Condition;
    public readonly BadCExpression TrueExpression;
    public readonly BadCExpression FalseExpression;
    
    public BadCInlineIfExpression(SourceToken sourceToken, BadCExpression condition, BadCExpression trueExpression, BadCExpression falseExpression) : base(false, sourceToken)
    {
        Condition = condition;
        TrueExpression = trueExpression;
        FalseExpression = falseExpression;
    }

    public override void Emit(BadCEmitContext context, BadCType typeHint, bool isLval)
    {
        AssemblySymbol falseLabel = context.Writer.GetUniqueName( "INL_IF_FALSE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "INL_IF_END" );
        context.AddSymbol( context.Writer, SourceToken );
        
        BadCType? baseTypeHint = Condition.GetFixedType( context );

        if ( baseTypeHint == null )
        {
            baseTypeHint = BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );
        }

        Condition.Emit( context, baseTypeHint, false );

        context.Writer.BranchZero( baseTypeHint, falseLabel );
        
        TrueExpression.Emit( context, typeHint, false );
        
        context.Writer.Jump(endLabel);
        
        context.Writer.Label(falseLabel, AssemblyElementVisibility.Local);
        
        FalseExpression.Emit( context, typeHint, false );

        context.Writer.Label(endLabel, AssemblyElementVisibility.Local);


    }

    public override BadCExpression ResolveTemplateTypes(BadCTemplateTypeContext templateContext)
    {
        return new BadCInlineIfExpression(SourceToken, Condition.ResolveTemplateTypes(templateContext),
            TrueExpression.ResolveTemplateTypes(templateContext),
            FalseExpression.ResolveTemplateTypes(templateContext));

    }
}