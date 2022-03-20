using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;

namespace BadC.Expressions.Access;

public class BadCArrayAccessExpression : BadCExpression
{

    public BadCExpression BasePointer { get; }

    public BadCExpression IndexOffset { get; }

    #region Public

    public BadCArrayAccessExpression(
        SourceToken sourceToken,
        BadCExpression basePointer,
        BadCExpression indexOffset ) : base( false, sourceToken )
    {
        BasePointer = basePointer;
        IndexOffset = indexOffset;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        BadCType? baseType = BasePointer.GetFixedType( context );

        if ( baseType == null )
        {
            throw new ParseException( "Base type of array access expression is not fixed", SourceToken );
        }

        if ( !baseType.TryGetBaseType( out BadCType elementType ) )
        {
            throw new ParseException( "Can not get base type of non-pointer type", SourceToken );
        }

        if ( elementType.IsPrimitive( BadCPrimitiveTypes.Void ) && elementType.PointerLevel <= 1 )
        {
            throw new ParseException(
                                     "Cannot access array of void type. Cast the pointer to use array access.",
                                     SourceToken
                                    );
        }

        context.AddSymbol( context.Writer, SourceToken );
        BasePointer.Emit( context, BadCType.GetPrimitive( BadCPrimitiveTypes.Void ).GetPointerType( 1 ), false );

        IndexOffset.Emit( context, BadCType.GetPrimitive( BadCPrimitiveTypes.I64 ), false );

        context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )elementType.Size ) );

        context.Writer.Emit( OpCode.MulI64, Array.Empty < byte >() );
        context.Writer.Emit( OpCode.AddI64, Array.Empty < byte >() );

        if ( !isLval )
        {
            context.Writer.Load( elementType );
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        BadCType? baseType = BasePointer.GetFixedType( context );

        if ( baseType == null )
        {
            throw new ParseException( "Base type of array access expression is not fixed", SourceToken );
        }

        if ( !baseType.TryGetBaseType( out BadCType elementType ) )
        {
            throw new ParseException( "Can not get base type of pointer type", SourceToken );
        }

        return elementType;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCArrayAccessExpression(
                                             SourceToken,
                                             BasePointer.ResolveTemplateTypes(
                                                                              templateContext
                                                                             ),
                                             IndexOffset.ResolveTemplateTypes(
                                                                              templateContext
                                                                             )
                                            );
    }

    #endregion

}
