using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Utils;
using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Branch;

public class BadCInlineNullCoalescingExpression : BadCBinaryExpression
{
    public BadCInlineNullCoalescingExpression(BadCExpression left, BadCExpression right, SourceToken token) : base(left, right, token)
    {
    }

    public override void Emit(BadCEmitContext context, BadCType baseTypeHint, bool isLval)
    {
        AssemblySymbol leftLabel = context.Writer.GetUniqueName( "INL_NC_IF_FALSE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "INL_NC_IF_END" );
        context.AddSymbol( context.Writer, SourceToken );

        Left.Emit( context, baseTypeHint, false );

        context.Writer.BranchNotZero( baseTypeHint, leftLabel );
        
        Right.Emit( context, baseTypeHint, false );
        
        context.Writer.Jump(endLabel);
        
        context.Writer.Label(leftLabel, AssemblyElementVisibility.Local);
        
        Left.Emit( context, baseTypeHint, false );

        context.Writer.Label(endLabel, AssemblyElementVisibility.Local);
    }

    public override BadCExpression ResolveTemplateTypes(BadCTemplateTypeContext templateContext)
    {
        return new BadCInlineNullCoalescingExpression(Left.ResolveTemplateTypes(templateContext),
            Right.ResolveTemplateTypes(templateContext), SourceToken);
    }
}