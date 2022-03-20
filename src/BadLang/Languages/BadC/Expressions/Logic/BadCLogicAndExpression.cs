using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Logic;

public class BadCLogicAndExpression : BadCBinaryExpression
{

    #region Public

    public BadCLogicAndExpression( BadCExpression left, BadCExpression right, SourceToken token ) : base(
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

        AssemblySymbol falseLabel = context.Writer.GetUniqueName( "LOGIC_AND_FALSE" );
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "LOGIC_AND_END" );

        BadCType lType = Left.GetFixedType( context ) ??
                         BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

        BadCType rType = Right.GetFixedType( context ) ??
                         BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

        context.AddSymbol( context.Writer, SourceToken );
        Left.Emit( context, lType, false );

        context.Writer.BranchZero( lType, falseLabel );

        Right.Emit( context, rType, false );

        context.Writer.BranchZero( rType, falseLabel );

        context.Writer.Push( baseTypeHint, 1 );
        context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );
        context.Writer.AddPatchSite( endLabel, context.Writer.CurrentSize - sizeof( long ), sizeof( long ) );
        context.Writer.Emit( OpCode.Jump, Array.Empty < byte >() );

        context.Writer.Label( falseLabel, AssemblyElementVisibility.Local );
        context.Writer.Push( baseTypeHint, 0 );

        context.Writer.Label( endLabel, AssemblyElementVisibility.Local );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return null;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCLogicAndExpression(
                                          Left.ResolveTemplateTypes( templateContext ),
                                          Right.ResolveTemplateTypes( templateContext ),
                                          SourceToken
                                         );
    }

    #endregion

}
