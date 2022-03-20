using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Exceptions;

namespace BadC.Expressions.Access.Member;

public class BadCIndirectMemberAccessExpression : BadCMemberAccessExpression
{

    #region Public

    public BadCIndirectMemberAccessExpression( BadCExpression left, string right, SourceToken token ) :
        base( left, right, token )
    {
    }

    public override void Emit( BadCEmitContext context, BadCType baseTypeHint, bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );
        BadCType? leftPtrType = Left.GetFixedType( context );

        if ( leftPtrType == null || !leftPtrType.IsPointer )
        {
            throw new ParseException( "Left side of indirect member access must be a fixed pointer type", SourceToken );
        }

        if ( !leftPtrType.TryGetBaseType( out BadCType leftType ) )
        {
            throw new ParseException( "Left side of indirect member access must be a fixed pointer type", SourceToken );
        }

        long memberOffset = leftType.GetMemberOffset( Right );

        if ( memberOffset == -1 )
        {
            throw new ParseException( $"Type {leftType} does not have a member named {Right}", SourceToken );
        }

        Left.Emit( context, leftPtrType, false );
        context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( memberOffset ) );
        context.Writer.Emit( OpCode.AddI64, Array.Empty < byte >() );

        BadCTypeField member = ( BadCTypeField )leftType.Members.First( m => m.Name == Right && m is BadCTypeField );

        if ( member.FieldType != baseTypeHint )
        {
            throw new ParseException( $"Type {leftType}.{Right} is not of type {baseTypeHint}", SourceToken );
        }

        if ( !isLval )
        {
            context.Writer.Load( baseTypeHint );
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        BadCType? leftPtrType = Left.GetFixedType( context );

        if ( leftPtrType == null || !leftPtrType.IsPointer )
        {
            throw new ParseException( "Left side of indirect member access must be a fixed pointer type", SourceToken );
        }

        if ( !leftPtrType.TryGetBaseType( out BadCType leftType ) )
        {
            throw new ParseException( "Left side of indirect member access must be a fixed pointer type", SourceToken );
        }

        BadCTypeMember? member = leftType.Members.FirstOrDefault( x => x.Name == Right );

        if ( member == null || member is not BadCTypeField field )
        {
            throw new SymbolNotFoundException( $"Member {Right} not found in type {leftType}" );
        }

        return field.FieldType;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCIndirectMemberAccessExpression(
                                                      Left.ResolveTemplateTypes(
                                                                                templateContext
                                                                               ),
                                                      Right,
                                                      SourceToken
                                                     );
    }

    #endregion

}
