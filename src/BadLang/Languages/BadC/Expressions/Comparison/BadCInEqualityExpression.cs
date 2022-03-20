using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Comparison;

public class BadCInEqualityExpression : BadCBinaryExpression
{

    #region Public

    public BadCInEqualityExpression( BadCExpression left, BadCExpression right, SourceToken token ) :
        base( left, right, token )
    {
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType typeHint,
        bool isLval )
    {
        if ( isLval )
        {
            throw new ParseException( "Cannot assign to a comparison expression", SourceToken );
        }

        BadCType baseTypeHint = context.CheckTypes( SourceToken, Left, Right );
        AssemblySymbol trueLabel = context.Writer.GetUniqueName( "OP_NOT_EQUAL_TRUE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "OP_NOT_EQUAL_END" );

        context.AddSymbol( context.Writer, SourceToken );
        Left.Emit( context, baseTypeHint, false );
        Right.Emit( context, baseTypeHint, false );

        context.Writer.Sub( baseTypeHint );
        context.Writer.BranchNotZero( baseTypeHint, trueLabel );
        context.Writer.Push( typeHint, 0 );
        context.Writer.Jump( endLabel );
        context.Writer.Label( trueLabel, AssemblyElementVisibility.Local );
        context.Writer.Push( typeHint, 1 ); //zero, so push 1

        context.Writer.Label( endLabel, AssemblyElementVisibility.Local ); //End Label
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCInEqualityExpression(
                                            Left.ResolveTemplateTypes( templateContext ),
                                            Right.ResolveTemplateTypes( templateContext ),
                                            SourceToken
                                           );
    }

    #endregion

}
