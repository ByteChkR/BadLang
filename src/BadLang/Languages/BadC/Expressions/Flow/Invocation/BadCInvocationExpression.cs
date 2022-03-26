using BadAssembler.Exceptions;

using BadC.DebugSymbols;
using BadC.Expressions.Access.Member;
using BadC.Expressions.Values.Symbols;
using BadC.Functions;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Flow.Invocation;

public class BadCInvocationExpression : BadCExpression
{

    public BadCExpression CallSite { get; protected set; }

    public BadCExpression[] Arguments { get; protected set; }

    #region Public

    public BadCInvocationExpression( BadCExpression callSite, BadCExpression[] arguments, SourceToken token ) :
        base( false, token )
    {
        CallSite = callSite;
        Arguments = arguments;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType baseTypeHint,
        bool isLval )
    {
        context.AddSymbol( context.Writer, SourceToken );

        //Get Callsite info
        if ( CallSite is BadCVariable var )
        {
            BadCVariableDeclaration vDecl = context.GetNamedVar( var.Name );

            if ( vDecl is BadCFunctionDeclaration fDecl )
            {
                //Check Argument Count
                if ( Arguments.Length != fDecl.Signature.ParameterSize.Length )
                {
                    throw new ParseException(
                                             $"Function '{fDecl.Name}' expects {fDecl.Signature.ParameterSize.Length} arguments, but {Arguments.Length} were provided.",
                                             SourceToken
                                            );
                }

               

                //Emit Arguments & Check Types

                for ( int i = 0; i < Arguments.Length; i++ )
                {
                    BadCVariableDeclaration pDecl = fDecl.Signature.ParameterSize[i];
                    Arguments[i].Emit( context, pDecl.Type, false );
                }

                //Emit Call Instruction

                context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );
                context.Writer.AddPatchSite( fDecl.Name, context.Writer.CurrentSize - sizeof( long ), sizeof( long ) );
                context.Writer.Emit( OpCode.Call, Array.Empty < byte >() );

                if ( baseTypeHint == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
                {
                    context.Writer.Pop( fDecl.Signature.ReturnType.Size );
                }
                else if ( baseTypeHint != fDecl.Signature.ReturnType )
                {
                    throw new ParseException(
                                             $"Function '{fDecl.Name}' expects a left handside type of {fDecl.Signature.ReturnType}, but {baseTypeHint} was provided.",
                                             SourceToken
                                            );
                }
            }
            else if ( vDecl.Type is BadCFunctionType funcType )
            {
                EmitFunc( funcType, context, baseTypeHint );
            }
            else
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }
        }
        else if ( CallSite is BadCDirectMemberAccessExpression directMember )
        {
            BadCType? callSiteType = directMember.Left.GetFixedType( context );

            if ( callSiteType == null || callSiteType.IsPointer )
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }

            FunctionInfo? callTarget = null;

            foreach ( BadCTypeMember member in callSiteType.Members )
            {
                if ( member.Name == directMember.Right && member is BadCTypeMethod method )
                {
                    callTarget = method.FunctionInfo;

                    break;
                }
            }

            if ( callTarget == null )
            {
                throw new ParseException(
                                         $"Cannot call non-function '{directMember.Right}' on type '{callSiteType}'",
                                         SourceToken
                                        );
            }

            AssemblySymbol sym = new AssemblySymbol(
                                                    callSiteType.TypeName.AssemblyName,
                                                    callSiteType.TypeName.SectionName,
                                                    callTarget.Symbol.SymbolName +
                                                    GetNameExtension()
                                                   );

            BadCVariableDeclaration decl = context.GetNamedVar( sym );

            if ( decl is BadCFunctionDeclaration fDecl )
            {
                callTarget = fDecl.Signature;

                if ( !callTarget.IsResolved )
                {
                    throw new ParseException( "Something went wrong", SourceToken );
                }

                //Check Argument Count
                if ( Arguments.Length != callTarget.ParameterSize.Length - 1 )
                {
                    throw new ParseException(
                                             $"Function '{callTarget.Symbol}' expects {callTarget.ParameterSize.Length - 1} arguments, but {Arguments.Length} were provided.",
                                             SourceToken
                                            );
                }

                //Emit Arguments & Check Types

                directMember.Left.Emit( context, callSiteType, true );

                for ( int i = 0; i < Arguments.Length; i++ )
                {
                    BadCVariableDeclaration pDecl = callTarget.ParameterSize[i + 1];
                    Arguments[i].Emit( context, pDecl.Type, false );
                }

                //Emit Call Instruction

                context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );

                context.Writer.AddPatchSite(
                                            callTarget.Symbol,
                                            context.Writer.CurrentSize - sizeof( long ),
                                            sizeof( long )
                                           );

                context.Writer.Emit( OpCode.Call, Array.Empty < byte >() );

                if ( baseTypeHint == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
                {
                    context.Writer.Pop( callTarget.ReturnType.Size );
                }
                else if ( baseTypeHint != callTarget.ReturnType )
                {
                    throw new ParseException(
                                             $"Function '{callTarget.Symbol}' expects a left handside type of {callTarget.ReturnType}, but {baseTypeHint} was provided.",
                                             SourceToken
                                            );
                }
            }
            else if ( decl.Type is BadCFunctionType funcType )
            {
                EmitFunc( funcType, context, baseTypeHint );
            }
            else
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }
        }
        else if ( CallSite is BadCIndirectMemberAccessExpression indirectMember )
        {
            BadCType? callSiteHint = indirectMember.Left.GetFixedType( context );

            if ( callSiteHint == null || !callSiteHint.IsPointer )
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }

            callSiteHint.TryGetBaseType( out BadCType callSiteType );

            FunctionInfo? callTarget = null;

            foreach ( BadCTypeMember member in callSiteType.Members )
            {
                if ( member.Name == indirectMember.Right && member is BadCTypeMethod method )
                {
                    callTarget = method.FunctionInfo;

                    break;
                }
            }

            if ( callTarget == null )
            {
                throw new ParseException(
                                         $"Cannot call non-function '{indirectMember.Right}' on type '{callSiteType}'",
                                         SourceToken
                                        );
            }

            AssemblySymbol sym = new AssemblySymbol(
                                                    callSiteType.TypeName.AssemblyName,
                                                    callSiteType.TypeName.SectionName,
                                                    callTarget.Symbol.SymbolName +
                                                    GetNameExtension()
                                                   );

            BadCVariableDeclaration decl = context.GetNamedVar( sym );

            if ( decl is BadCFunctionDeclaration fDecl )
            {
                callTarget = fDecl.Signature;

                if ( !callTarget.IsResolved )
                {
                    throw new ParseException( "Something went wrong", SourceToken );
                }

                //Check Argument Count
                if ( Arguments.Length != callTarget.ParameterSize.Length - 1 )
                {
                    throw new ParseException(
                                             $"Function '{callTarget.Symbol}' expects {callTarget.ParameterSize.Length - 1} arguments, but {Arguments.Length} were provided.",
                                             SourceToken
                                            );
                }

                //Emit Arguments & Check Types

                indirectMember.Left.Emit( context, callSiteHint, false );

                for ( int i = 0; i < Arguments.Length; i++ )
                {
                    BadCVariableDeclaration pDecl = callTarget.ParameterSize[i + 1];
                    Arguments[i].Emit( context, pDecl.Type, false );
                }

                //Emit Call Instruction

                context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );

                context.Writer.AddPatchSite(
                                            callTarget.Symbol,
                                            context.Writer.CurrentSize - sizeof( long ),
                                            sizeof( long )
                                           );

                context.Writer.Emit( OpCode.Call, Array.Empty < byte >() );

                if ( baseTypeHint == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
                {
                    context.Writer.Pop( callTarget.ReturnType.Size );
                }
                else if ( baseTypeHint != callTarget.ReturnType )
                {
                    throw new ParseException(
                                             $"Function '{callTarget.Symbol}' expects a left handside type of {callTarget.ReturnType}, but {baseTypeHint} was provided.",
                                             SourceToken
                                            );
                }
            }
            else if ( decl.Type is BadCFunctionType funcType )
            {
                EmitFunc( funcType, context, baseTypeHint );
            }
            else
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }
        }
        else if ( CallSite.GetFixedType( context ) is BadCFunctionType funcType )
        {
            EmitFunc( funcType, context, baseTypeHint );
        }
        else
        {
            throw new ParseException( $"Can not call {CallSite}", SourceToken );
        }
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        if ( CallSite is BadCVariable var )
        {
            BadCVariableDeclaration vDecl = context.GetNamedVar( var.Name );

            if ( vDecl is BadCFunctionDeclaration fDecl )
            {
                return fDecl.Type;
            }

            if ( vDecl.Type is BadCFunctionType funcType )
            {
                return funcType.ReturnType;
            }
        }
        else if ( CallSite is BadCMemberAccessExpression member )
        {
            BadCType? callSiteType = member.Left.GetFixedType( context );

            if ( callSiteType == null )
            {
                throw new ParseException( "Cannot call non-function", SourceToken );
            }

            if (callSiteType.IsPointer && callSiteType.TryGetBaseType( out callSiteType ) )
            {
            }

            FunctionInfo? callTarget = null;

            foreach ( BadCTypeMember targetMember in callSiteType.Members )
            {
                if ( targetMember.Name == member.Right && targetMember is BadCTypeMethod method )
                {
                    callTarget = method.FunctionInfo;

                    break;
                }
            }

            AssemblySymbol sym;
            BadCVariableDeclaration decl;

            if ( callTarget != null )
            {
                sym = new AssemblySymbol(
                                         callSiteType.TypeName.AssemblyName,
                                         callSiteType.TypeName.SectionName,
                                         callTarget.Symbol.SymbolName +
                                         GetNameExtension()
                                        );

                decl = context.GetNamedVar( sym );
            }
            else
            {
                sym = new AssemblySymbol(
                                         callSiteType.TypeName.AssemblyName,
                                         callSiteType.TypeName.SectionName,
                                         callSiteType.TypeName.SymbolName +
                                         "_" +
                                         member.Right +
                                         GetNameExtension()
                                        );

                decl = context.GetNamedVar( sym );
            }

            if ( decl is BadCFunctionDeclaration fDecl )
            {
                return fDecl.Signature.ReturnType;
            }

            if ( decl.Type is BadCFunctionType funcType )
            {
                return funcType.ReturnType;
            }
        }
        else if ( CallSite.GetFixedType( context ) is BadCFunctionType funcType )
        {
            return funcType.ReturnType;
        }

        throw new ParseException( $"Can not call {CallSite}. It has no fixed Type", SourceToken );
    }

    public virtual string GetNameExtension()
    {
        return "";
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        BadCExpression newCallSite = CallSite.ResolveTemplateTypes( templateContext );

        BadCExpression[] args = Arguments.
                                Select( a => a.ResolveTemplateTypes( templateContext ) ).
                                ToArray();

        return new BadCInvocationExpression( newCallSite, args, SourceToken );
    }

    #endregion

    #region Private

    private void EmitFunc(
        BadCFunctionType funcType,
        BadCEmitContext context,
        BadCType baseTypeHint )
    {
        //Check Argument Count
        if ( Arguments.Length != funcType.Signature.Length )
        {
            throw new ParseException(
                                     $"Function '{funcType}' expects {funcType.Signature.Length} arguments, but {Arguments.Length} were provided.",
                                     SourceToken
                                    );
        }

        //Emit Arguments & Check Types

        for ( int i = 0; i < Arguments.Length; i++ )
        {
            Arguments[i].Emit( context, funcType.Signature[i], false );
        }

        //Emit Call Instruction

        CallSite.Emit( context, funcType, false );
        context.Writer.Emit( OpCode.Call, Array.Empty < byte >() );

        if ( baseTypeHint == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
        {
            context.Writer.Pop( funcType.ReturnType.Size );
        }
        else if ( baseTypeHint != funcType.ReturnType )
        {
            throw new ParseException(
                                     $"Function '{funcType}' expects a left handside type of {funcType.ReturnType}, but {baseTypeHint} was provided.",
                                     SourceToken
                                    );
        }
    }

    #endregion

}
