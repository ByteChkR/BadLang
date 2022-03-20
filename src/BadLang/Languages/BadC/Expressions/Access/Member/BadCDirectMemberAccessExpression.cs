using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Exceptions;

namespace BadC.Expressions.Access.Member;

public class BadCDirectMemberAccessExpression : BadCMemberAccessExpression
{

    #region Public

    public BadCDirectMemberAccessExpression( BadCExpression left, string right, SourceToken token ) :
        base( left, right, token )
    {
    }

    public override void Emit( BadCEmitContext context, BadCType baseTypeHint, bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );
        BadCType? leftType = Left.GetFixedType( context );

        if ( leftType == null )
        {
            throw new ParseException( "Left side of direct member access must be a fixed type", SourceToken );
        }

        long memberOffset = leftType.GetMemberOffset( Right );

        if ( memberOffset == -1 )
        {
            throw new ParseException( $"Type {leftType} does not have a member named {Right}", SourceToken );
        }

        Left.Emit( context, leftType, true );
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
        BadCType? leftType = Left.GetFixedType( context );

        if ( leftType == null || leftType.IsPointer )
        {
            throw new ParseException( "Left side of direct member access is not fixed", SourceToken );
        }

        BadCTypeMember? member = leftType.Members.FirstOrDefault( x => x.Name == Right );

        if ( member == null || member is not BadCTypeField field )
        {
            throw new SymbolNotFoundException( $"Type {leftType} does not have a member named {Right}" );
        }

        return field.FieldType;
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        return new BadCDirectMemberAccessExpression(
                                                    Left.ResolveTemplateTypes(
                                                                              templateContext
                                                                             ),
                                                    Right,
                                                    SourceToken
                                                   );
    }

    #endregion

}
