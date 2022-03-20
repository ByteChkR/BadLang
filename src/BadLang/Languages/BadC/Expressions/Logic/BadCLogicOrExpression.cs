using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Logic;

public class BadCLogicOrExpression : BadCBinaryExpression
{

    #region Public

    public BadCLogicOrExpression( BadCExpression left, BadCExpression right, SourceToken token ) : base(
         left,
         right,
         token
        )
    {
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        if ( isLval )
        {
            throw new ParseException( "Cannot assign to a number", SourceToken );
        }

        AssemblySymbol trueLabel = context.Writer.GetUniqueName( "LOGIC_OR_TRUE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "LOGIC_OR_END" );

        BadCType lType = Left.GetFixedType( context ) ??
                         BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

        BadCType rType = Right.GetFixedType( context ) ??
                         BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

        context.AddSymbol( context.Writer, SourceToken );
        Left.Emit( context, rType, false );

        context.Writer.BranchNotZero( lType, trueLabel );

        Right.Emit( context, rType, isLval );

        context.Writer.BranchNotZero( rType, trueLabel );

        context.Writer.Push( baseTypeHint, 0 );

        context.Writer.Jump( endLabel );

        context.Writer.Label( trueLabel, AssemblyElementVisibility.Local );

        context.Writer.Push( baseTypeHint, 1 );

        context.Writer.Label( endLabel, AssemblyElementVisibility.Local );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return null;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCLogicOrExpression(
                                         Left.ResolveTemplateTypes( templateContext ),
                                         Right.ResolveTemplateTypes( templateContext ),
                                         SourceToken
                                        );
    }

    #endregion

}
