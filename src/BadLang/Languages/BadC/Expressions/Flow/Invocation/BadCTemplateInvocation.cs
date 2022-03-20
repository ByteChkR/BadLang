using BadAssembler.Exceptions;

using BadC.Expressions.Access.Member;
using BadC.Expressions.Values.Symbols;
using BadC.Functions;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Flow.Invocation;

public class BadCTemplateInvocation : BadCInvocationExpression
{

    public BadCType[] TemplateArguments { get; }

    #region Public

    public BadCTemplateInvocation(
        BadCExpression callSite,
        BadCType[] templateArgs,
        BadCExpression[] arguments,
        SourceToken token ) : base( callSite, arguments, token )
    {
        TemplateArguments = templateArgs;
    }

    public override void Emit( BadCEmitContext context, BadCType baseTypeHint, bool isLval )
    {
        if ( CallSite is BadCVariable var )
        {
            BadCVariableDeclaration vDecl = context.GetNamedVar( var.Name );

            if ( vDecl is BadCFunctionDeclaration fDecl )
            {
                if ( !fDecl.Signature.IsTemplate )
                {
                    throw new ParseException(
                                             "Cannot call non-template function with template arguments",
                                             SourceToken
                                            );
                }

                CallSite = Resolve( fDecl.Signature, context );

                base.Emit( context, baseTypeHint, isLval );

                return;
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
                if ( member is BadCTypeMethod method && method.Name == directMember.Right )
                {
                    callTarget = method.FunctionInfo;

                    break;
                }
            }

            if ( callTarget == null )
            {
                throw new ParseException(
                                         $"Cannot find function {directMember.Right} in type {callSiteType}",
                                         SourceToken
                                        );
            }

            if ( !callTarget.IsTemplate )
            {
                throw new ParseException( "Cannot call non-template function with template arguments", SourceToken );
            }

            Resolve( callTarget, context );

            base.Emit( context, baseTypeHint, isLval );

            return;
        }
        else if ( CallSite is BadCIndirectMemberAccessExpression indirectMember )
        {
            BadCType? callSitePtr = indirectMember.Left.GetFixedType( context );

            if ( callSitePtr == null || !callSitePtr.IsPointer )
            {
                throw new ParseException( "Left side of indirect member access is not a pointer type", SourceToken );
            }

            if ( !callSitePtr.TryGetBaseType( out BadCType callSiteType ) )
            {
                throw new ParseException( "Left side of indirect member access is not a pointer type", SourceToken );
            }

            FunctionInfo? callTarget = null;

            foreach ( BadCTypeMember member in callSiteType.Members )
            {
                if ( member is BadCTypeMethod method && method.Name == indirectMember.Right )
                {
                    callTarget = method.FunctionInfo;

                    break;
                }
            }

            if ( callTarget == null )
            {
                throw new ParseException(
                                         $"Cannot find function {indirectMember.Right} in type {callSiteType}",
                                         SourceToken
                                        );
            }

            if ( !callTarget.IsTemplate )
            {
                throw new ParseException( "Cannot call non-template function with template arguments", SourceToken );
            }

            Resolve( callTarget, context );

            base.Emit( context, baseTypeHint, isLval );

            return;
        }

        throw new ParseException( "Cannot call non-template function with template arguments", SourceToken );
    }

    public override string GetNameExtension()
    {
        uint hash = 0;

        foreach ( BadCType type in TemplateArguments )
        {
            hash ^= ( uint )type.TypeName.ToString().GetHashCode();
        }

        return $"_{hash}";
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        BadCExpression[] newArgs = new BadCExpression[Arguments.Length];

        for ( int i = 0; i < Arguments.Length; i++ )
        {
            newArgs[i] = Arguments[i].ResolveTemplateTypes( templateContext );
        }

        return new BadCTemplateInvocation(
                                          CallSite.ResolveTemplateTypes( templateContext ),
                                          TemplateArguments,
                                          newArgs,
                                          SourceToken
                                         );
    }

    #endregion

    #region Private

    private BadCExpression Resolve( FunctionInfo info, BadCEmitContext context )
    {
        Dictionary < BadCTemplateType, BadCType > typeMap = new Dictionary < BadCTemplateType, BadCType >();

        if ( TemplateArguments.Length != info.TemplateTypes.Length )
        {
            throw new ParseException(
                                     "Template argument count mismatch",
                                     SourceToken
                                    );
        }

        for ( int i = 0; i < TemplateArguments.Length; i++ )
        {
            if ( info.TemplateTypes[i] is BadCTemplateType template )
            {
                typeMap[template] = TemplateArguments[i];
            }
        }

        BadCTemplateTypeContext ctx = new BadCTemplateTypeContext(
                                                                  typeMap,
                                                                  context.TaskList,
                                                                  info.CodeSectionWriter
                                                                 );

        AssemblySymbol newInfoName = new AssemblySymbol(
                                                        info.Symbol.AssemblyName,
                                                        info.Symbol.SectionName,
                                                        info.Symbol.SymbolName + "_" + ctx.GetTemplateHash()
                                                       );

        FunctionInfo? newInfo = context.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >().
                                        GetInfo( newInfoName );

        if ( newInfo != null )
        {
            return newInfo.EmitTemplate( ctx );
        }

        return info.ResolveTemplate( ctx ).EmitTemplate( ctx );
    }

    #endregion

}
