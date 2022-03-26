using BadAssembler;
using BadAssembler.Exceptions;

using BadC.Expressions;
using BadC.Expressions.Values.Symbols;
using BadC.Functions;
using BadC.Types;
using BadC.Types.Members;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;
using BadVM.Shared.Logging;

namespace BadC.Templates;

public static class BadCTemplateResolver
{

    private static readonly LogMask s_LogMask = new LogMask( "Template Resolver" );

    #region Public

    public static BadCVariable EmitTemplate(
        this FunctionInfo info,
        BadCTemplateTypeContext templateContext )
    {
        BadCElementExporter exporter =
            templateContext.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

        AssemblySymbol sym = new AssemblySymbol(
                                                info.Symbol.AssemblyName,
                                                info.Symbol.SectionName,
                                                $"{info.Symbol.SymbolName}"
                                               );

        FunctionInfo? existing = exporter.GetInfo( sym );

        if ( existing != null && existing.IsEmitted )
        {
            return new BadCVariable(
                                    sym,
                                    info.Token
                                   );
        }

        if ( !info.IsResolved )
        {
            throw new Exception();
        }

        info.SetEmitted();

        templateContext.TaskList.AddTask(
                                         $"Emit Template Function {info}",
                                         () =>
                                         {

                                             BadCEmitContext context = new BadCEmitContext(
                                                  info.CodeSectionWriter,
                                                  info,
                                                  templateContext.TaskList,
                                                  false
                                                 );

                                             info.WriteProlouge( context.Writer );

                                             foreach ( BadCExpression expr in info.TemplateExpressions )
                                             {
                                                 expr.Emit(
                                                           context,
                                                           BadCType.GetPrimitive( BadCPrimitiveTypes.Void ),
                                                           false
                                                          );
                                             }
                                         }
                                        );

        return new BadCVariable(
                                sym,
                                info.Token
                               );
    }

    public static FunctionInfo ResolveTemplate( this FunctionInfo info, BadCTemplateTypeContext templateContext )
    {
        BadCType rType = templateContext.ResolveType( info.ReturnType ).ResolveTemplate( templateContext );

        BadCVariableDeclaration[] parameters = new BadCVariableDeclaration[info.ParameterSize.Length];

        for ( int i = 0; i < info.ParameterSize.Length; i++ )
        {
            parameters[i] = ( BadCVariableDeclaration )info.ParameterSize[i].ResolveTemplateTypes( templateContext );
        }

        BadCVariableDeclaration[] locals = new BadCVariableDeclaration[info.LocalSize.Length];

        for ( int i = 0; i < info.LocalSize.Length; i++ )
        {
            locals[i] = ( BadCVariableDeclaration )info.LocalSize[i].ResolveTemplateTypes( templateContext );
        }

        BadCExpression[] exprs = new BadCExpression[info.TemplateExpressions.Length];

        for ( int i = 0; i < info.TemplateExpressions.Length; i++ )
        {
            BadCExpression expr = info.TemplateExpressions[i];
            exprs[i] = expr.ResolveTemplateTypes( templateContext );
        }

        BadCType[] templateTypes = new BadCType[info.TemplateTypes.Length];

        for ( int i = 0; i < info.TemplateTypes.Length; i++ )
        {
            BadCType templateType = info.TemplateTypes[i];
            templateTypes[i] = templateContext.ResolveType( templateType ).ResolveTemplate( templateContext );
        }

        BadCElementExporter exporter =
            templateContext.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

        List < uint > hashes = new List < uint >( info.GetHashes() );
        hashes.Add( templateContext.GetTemplateHash() );

        FunctionInfo newInfo = exporter.AddFunction(
                                                    info.GetUnhashedSymbol(),
                                                    rType,
                                                    parameters,
                                                    info.Visibility,
                                                    info.IsExtern,
                                                    info.IsTemplate,
                                                    info.IsInstance,
                                                    info.Token,
                                                    info.CodeSectionWriter,
                                                    hashes.ToArray()
                                                   );

        foreach ( BadCVariableDeclaration local in locals )
        {
            newInfo.AddLocal( local );
        }

        newInfo.SetTemplateExpressions( exprs );
        newInfo.AddTemplateTypes( templateTypes );

        return newInfo;
    }

    public static BadCVariableDeclaration ResolveTemplateTypes(
        this BadCVariableDeclaration decl,
        BadCType[] templateTypes,
        PostSegmentParseTasks taskList,
        CodeSectionWriter writer )
    {
        
        if (decl.Type.IsResolved) 
            return decl;
        Dictionary < BadCTemplateType, BadCType > typeMap = new Dictionary < BadCTemplateType, BadCType >();

        if ( decl.Type.TemplateTypes.Length != templateTypes.Length )
        {
            throw new ParseException(
                                     "Template argument count mismatch",
                                     decl.SourceToken
                                    );
        }

        if ( templateTypes.Length == 0 )
        {
            return decl;
        }

        for ( int i = 0; i < decl.Type.TemplateTypes.Length; i++ )
        {
            if ( decl.Type.TemplateTypes[i] is BadCTemplateType template && templateTypes[i] is not BadCTemplateType )
            {
                typeMap[template] = templateTypes[i];
            }
        }

        BadCTemplateTypeContext context = new BadCTemplateTypeContext( typeMap, taskList, writer );

        BadCVariableDeclaration resolved = ( BadCVariableDeclaration )decl.ResolveTemplateTypes( context );

        BadCType resolvedType = resolved.Type;

        if ( resolvedType.IsResolved )
        {
            if (resolvedType.IsPointer&& resolvedType.TryGetBaseType(out BadCType ptrType) )
            {
                resolvedType = ptrType;
            }
            resolvedType.EmitTemplate( context );
        }

        return resolved;
    }

    #endregion

    #region Private

    private static void EmitTemplate( this BadCType type, BadCTemplateTypeContext context )
    {
        List < BadCTypeMember > members = new List < BadCTypeMember >();

        foreach ( BadCTypeMember member in type.Members )
        {
            if ( !member.IsResolved )
            {
                members.Add( member.ResolveTemplate( context ) );
            }
            else
            {
                members.Add( member );
            }
        }

        type.SetMembers( members.ToArray() );
        BadCElementExporter exporter = context.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();
        exporter.UpdateType( type );

        foreach ( BadCTypeMember member in type.Members )
        {
            if ( member is BadCTypeMethod method && !method.FunctionInfo.IsTemplate )
            {
                if ( !method.FunctionInfo.IsResolved )
                {
                    method = ( BadCTypeMethod )method.ResolveTemplate( context );
                }

                method.FunctionInfo.EmitTemplate( context );
            }
        }
    }

    #endregion

}
