using BadAssembler;
using BadAssembler.Exceptions;
using BadAssembler.Segments;

using BadC.DebugSymbols;
using BadC.Expressions;
using BadC.Expressions.Access;
using BadC.Expressions.Branch;
using BadC.Expressions.Flow;
using BadC.Expressions.Flow.Invocation;
using BadC.Expressions.Pointer;
using BadC.Expressions.Type;
using BadC.Expressions.Values;
using BadC.Expressions.Values.Symbols;
using BadC.Functions;
using BadC.Operators;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;
using BadVM.Shared.Logging;

namespace BadC
{

    public class BadCCodeParser : CodeSyntaxParser
    {

        private static readonly LogMask s_LogMask = new LogMask( "BadC" );

        #region Public

        public BadCCodeParser() : base( "BadC" )
        {
        }

        public override void Parse(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList )
        {
            taskList.AddTask(
                             $"Nested Type Recursion Check for Section {sectionWriter.AssemblyName}::{sectionWriter.SectionName}",
                             () => NestedTaskRecursionCheck( sectionWriter )
                            );

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                bool isExtern = false;
                bool isTemplate = false;

                //Read Function Header
                //Read Return Value
                SourceReaderToken rVal = reader.ParseWord();

                if ( rVal.StringValue == "template" )
                {
                    isTemplate = true;
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    rVal = reader.ParseWord();
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( rVal.StringValue == "extern" )
                {
                    if ( isTemplate )
                    {
                        throw new ParseException( "Extern template functions are not supported", rVal.SourceToken );
                    }

                    isExtern = true;
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    rVal = reader.ParseWord();
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                if ( rVal.StringValue == "struct" )
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( isExtern )
                    {
                        ParseExternStructure( reader, sectionWriter, taskList );
                    }
                    else
                    {
                        ParseStructure( reader, sectionWriter, taskList, isTemplate );
                    }

                    continue;
                }

                //Find size of Type
                BadCType rType = ParseType( reader, rVal, sectionWriter, isTemplate, out BadCType[] _ );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                AssemblySymbol sym;
                SourceToken token;

                if ( isExtern )
                {
                    sym = reader.ParseSymbol( sectionWriter.AssemblyName, sectionWriter.SectionName, out token );
                }
                else
                {
                    SourceReaderToken name = reader.ParseWord();

                    token = name.SourceToken;

                    sym = new AssemblySymbol(
                                             sectionWriter.AssemblyName,
                                             sectionWriter.SectionName,
                                             name.StringValue
                                            );
                }

                ParseFunction( reader, sym, sectionWriter, isExtern, isTemplate, rType, token, taskList );
            }
        }

        internal BadCExpression ParseExpression(
            SourceReader reader,
            BadCEmitContext context,
            BadCExpression? left,
            int precedence = int.MaxValue )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            left ??= ParseValue( reader, context, precedence );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            while ( !reader.Is( ';' ) )
            {
                if ( reader.Is( '(' ) )
                {
                    SourceToken token = reader.SourceFile.GetToken( reader.CurrentIndex );
                    reader.Eat( '(' );
                    left = ParseInvocation( reader, context, left, token );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    continue;
                }

                if ( reader.Is( '[' ) )
                {
                    SourceToken token = reader.SourceFile.GetToken( reader.CurrentIndex );
                    reader.Eat( '[' );
                    left = ParseArrayAccess( reader, context, left, token );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    continue;
                }

                if ( reader.Is( '<' ) )
                {
                    int resetIdx = reader.CurrentIndex;
                    reader.Eat( '<' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    List < BadCType > templateArgs = new List < BadCType >();
                    bool abort = false;

                    while ( !reader.Is( '>' ) )
                    {
                        try
                        {
                            SourceReaderToken typeName = reader.ParseWord();

                            BadCType rType = ParseType(
                                                       reader,
                                                       typeName,
                                                       context.Writer,
                                                       context.AllowTemplateTypes,
                                                       out BadCType[] _
                                                      );

                            templateArgs.Add( rType );
                        }
                        catch ( Exception )
                        {
                            //Ugly
                            abort = true;

                            break;
                        }

                        reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    }

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( !abort )
                    {
                        reader.Eat( '>' );
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );
                        SourceToken token = reader.SourceFile.GetToken( reader.CurrentIndex );
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );

                        if ( reader.Is( '(' ) )
                        {
                            reader.Eat( '(' );
                            left = ParseTemplateInvocation( reader, context, left, token, templateArgs );

                            reader.SkipWhitespaceAndNewlineAndComments( "//" );

                            continue;
                        }

                        reader.ResetTo( resetIdx );
                    }
                    else
                    {
                        reader.ResetTo( resetIdx );
                    }
                }

                SourceReaderToken op = reader.ParseUntilWhiteSpaceOrWordOrDigitOrSeparator();
                BadCBinaryOperator? opr = BadCBinaryOperator.Get( precedence, op.StringValue );

                if ( opr == null )
                {
                    reader.ResetTo( op );

                    return left;
                }

                left = opr.Parse( context, left, reader, this, op.SourceToken );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            return left;
        }

        #endregion

        #region Private

        private void NestedTaskRecursionCheck( CodeSectionWriter writer )
        {
            BadCElementExporter elements = writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

            foreach ( BadCType type in elements.Types )
            {
                try
                {
                    NestedTaskRecursionCheck( type, new List < BadCType >() );
                }
                catch ( Exception e )
                {
                    throw new ParseException( e.Message, type.SourceToken );
                }
            }
        }

        private void NestedTaskRecursionCheck(
            BadCType currentType,
            List < BadCType > visited )
        {
            visited.Add( currentType );
            List < BadCType > v = new List < BadCType >( visited );

            foreach ( BadCTypeMember member in currentType.Members )
            {
                if ( member is BadCTypeField field )
                {
                    if ( visited.Contains( field.FieldType ) )
                    {
                        throw new Exception( "Recursive type definition detected" );
                    }

                    NestedTaskRecursionCheck( field.FieldType, v );
                }
            }
        }

        private BadCExpression ParseArrayAccess(
            SourceReader reader,
            BadCEmitContext context,
            BadCExpression callSite,
            SourceToken token )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            BadCArrayAccessExpression expr =
                new BadCArrayAccessExpression(
                                              token,
                                              callSite,
                                              ParseExpression( reader, context, null, int.MaxValue - 1 )
                                             );

            reader.Eat( ']' );

            return expr;
        }

        private void ParseExternStructure(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList )
        {
            SourceReaderToken structName = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblyElementVisibility visibility = AssemblyElementVisibility.Local;

            if ( reader.Is( ':' ) )
            {
                reader.Eat( ':' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                SourceReaderToken visibilityToken = reader.ParseWord();
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                visibility = ( AssemblyElementVisibility )Enum.Parse(
                                                                     typeof( AssemblyElementVisibility ),
                                                                     visibilityToken.StringValue,
                                                                     true
                                                                    );
            }

            BadCElementExporter elementExporter =
                sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

            int asmSepIndex = sectionWriter.SectionName.IndexOf( '_' );
            string asmName = sectionWriter.SectionName.Substring( 0, asmSepIndex );
            string sectionName = sectionWriter.SectionName.Substring( asmSepIndex + 1 );

            AssemblySymbol structureSymbol = new AssemblySymbol(
                                                                asmName,
                                                                sectionName,
                                                                structName.StringValue
                                                               );

            elementExporter.AddType( structureSymbol, visibility, structName.SourceToken, true, false );
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            reader.Eat( '{' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                ParseExternStructureMember( reader, sectionWriter, taskList, structureSymbol );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            reader.Eat( '}' );
            reader.SkipWhitespaceAndNewlineAndComments( "//" );
        }

        private void ParseExternStructureMember(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList,
            AssemblySymbol typeName )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            SourceReaderToken baseType = reader.ParseWord();

            BadCType type = ParseType( reader, baseType, sectionWriter, false, out BadCType[] _ );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            SourceReaderToken name = reader.ParseWord();

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblyElementVisibility visibility = AssemblyElementVisibility.Local;

            if ( reader.Is( '(' ) )
            {
                int asmSepIndex = sectionWriter.SectionName.IndexOf( '_' );
                string asmName = sectionWriter.SectionName.Substring( 0, asmSepIndex );
                string sectionName = sectionWriter.SectionName.Substring( asmSepIndex + 1 ).Split( ':' )[0];

                AssemblySymbol sym = new AssemblySymbol(
                                                        asmName,
                                                        sectionName,
                                                        typeName.SymbolName + "_" + name.StringValue
                                                       );

                BadCElementExporter elementExporter =
                    sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

                BadCType? instanceType = elementExporter.GetType( typeName );

                if ( instanceType == null )
                {
                    throw new ParseException(
                                             "Could not find type '" + typeName + "'",
                                             type.SourceToken
                                            );
                }

                ParseFunction(
                              reader,
                              sym,
                              sectionWriter,
                              true,
                              false,
                              type,
                              baseType.SourceToken,
                              taskList,
                              true,
                              instanceType,
                              name.StringValue
                             );
            }
            else
            {
                //Variable

                int offset = ( int )( decimal )reader.ParseNumber().ParsedValue;

                reader.Eat( ';' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                BadCElementExporter elementExporter =
                    sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

                BadCType? instanceType = elementExporter.GetType( typeName );

                if ( instanceType == null )
                {
                    throw new ParseException(
                                             "Could not find type '" + typeName + "'",
                                             type.SourceToken
                                            );
                }

                instanceType.AddMember( new BadCTypeField( name.StringValue, visibility, type, offset ) );
            }
        }

        private void ParseFunction(
            SourceReader reader,
            AssemblySymbol sym,
            CodeSectionWriter sectionWriter,
            bool isExtern,
            bool isTemplate,
            BadCType rType,
            SourceToken token,
            PostSegmentParseTasks taskList,
            bool isInstance = false,
            BadCType? instanceType = null,
            string? originalName = null )
        {

            if ( isTemplate && isInstance )
            {
                throw new ParseException("Cannot have template functions inside templates", token);
            }
            
            if ( isTemplate )
            {
                s_LogMask.LogMessage( $"Parsing Template{( isInstance ? " Instance" : "" )} Function: {sym}" );
            }
            else if(!isExtern)
            {
                s_LogMask.LogMessage( $"Parsing{( isInstance ? " Instance" : "" )} Function: {sym}" );
            }

            List < BadCVariableDeclaration > args = new List < BadCVariableDeclaration >();

            if ( isInstance )
            {
                AssemblySymbol argName = new AssemblySymbol( sym.AssemblyName, sym.SectionName, "this" );
                args.Add( new BadCVariableDeclaration( argName, instanceType!.GetPointerType(), new SourceToken() ) );
            }

            BadCTemplateType[] templateTypes = Array.Empty < BadCTemplateType >();

            if ( isTemplate )
            {
                templateTypes = ParseTemplateTypes( reader, sectionWriter );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '(' );
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            if ( !reader.Is( ')' ) )
            {
                do
                {
                    if ( reader.Is( ',' ) )
                    {
                        reader.Eat( ',' );
                    }

                    BadCVariableDeclaration decl =
                        ParseVariableDeclaration( reader, sym, sectionWriter, taskList, isTemplate || ( instanceType?.IsTemplate ?? false ) );

                    args.Add( decl );
                }
                while ( reader.Is( ',' ) );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( ')' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblyElementVisibility vis = AssemblyElementVisibility.Local;

            if ( reader.Is( ':' ) )
            {
                reader.Eat( ':' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                SourceReaderToken visibilityToken = reader.ParseWord();

                vis = ( AssemblyElementVisibility )Enum.Parse(
                                                              typeof( AssemblyElementVisibility ),
                                                              visibilityToken.StringValue,
                                                              true
                                                             );
            }

            if ( isTemplate && vis == AssemblyElementVisibility.Export )
            {
                throw new ParseException( $"Can not export Template Function {sym}", token );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            BadCElementExporter exporter =
                sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

            FunctionInfo info = exporter.AddFunction(
                                                     sym,
                                                     rType,
                                                     args.ToArray(),
                                                     vis,
                                                     isExtern,
                                                     isTemplate,
                                                     isInstance,
                                                     token,
                                                     sectionWriter
                                                    );

            if ( isTemplate )
            {
                info.AddTemplateTypes( templateTypes );
            }

            if ( isInstance )
            {
                instanceType!.AddMember( new BadCTypeMethod( originalName!, info ) );
            }

            if ( !isExtern )
            {
                ParseFunction(
                              reader,
                              info,
                              sectionWriter,
                              taskList,
                              isTemplate || ( instanceType?.IsTemplate ?? false )
                             );
            }
            else
            {
                reader.Eat( ';' );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
        }

        private void ParseFunction(
            SourceReader reader,
            FunctionInfo info,
            CodeSectionWriter writer,
            PostSegmentParseTasks taskList,
            bool allowTemplateTypes )
        {
            reader.Eat( '{' );

            BadCEmitContext context = new BadCEmitContext( writer, info, taskList, allowTemplateTypes );
            List < BadCExpression > exprs = new List < BadCExpression >();

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                BadCExpression expr = ParseExpression( reader, context, null );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                exprs.Add( expr );

                if ( !reader.Is( ';' ) )
                {
                    throw new ParseException( "Expected ;", reader.SourceFile.GetToken( reader.CurrentIndex ) );
                }

                reader.Eat( ';' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            if ( !allowTemplateTypes )
            {
                taskList.AddTask(
                                 $"Emit Function {info}",
                                 () =>
                                 {
                                     info.WriteProlouge( writer );

                                     foreach ( BadCExpression expr in exprs )
                                     {
                                         expr.Emit(
                                                   context,
                                                   BadCType.GetPrimitive( BadCPrimitiveTypes.Void ),
                                                   false
                                                  );
                                     }
                                 }
                                );
            }
            else
            {
                info.SetTemplateExpressions( exprs.ToArray() );
            }

            reader.Eat( '}' );
        }

        private BadCExpression ParseIfBranch(
            SourceReader reader,
            BadCEmitContext context,
            SourceToken token )
        {
            Dictionary < BadCExpression, BadCExpression[] > conditionBlocks =
                new Dictionary < BadCExpression, BadCExpression[] >();

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '(' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            BadCExpression cond = ParseExpression( reader, context, null, int.MaxValue - 1 );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( ')' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '{' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            List < BadCExpression > body = new List < BadCExpression >();

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                body.Add( ParseExpression( reader, context, null ) );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( !reader.Is( ';' ) )
                {
                    throw new ParseException( "Expected ;", reader.SourceFile.GetToken( reader.CurrentIndex ) );
                }

                reader.Eat( ';' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            reader.Eat( '}' );

            conditionBlocks.Add( cond, body.ToArray() );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            while ( reader.Is( "else if" ) )
            {
                reader.Eat( "else if" );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                reader.Eat( '(' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                BadCExpression eCond = ParseExpression( reader, context, null, int.MaxValue - 1 );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                reader.Eat( ')' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                reader.Eat( '{' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                List < BadCExpression > eBody = new List < BadCExpression >();

                while ( !reader.Is( '}' ) )
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    eBody.Add( ParseExpression( reader, context, null ) );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( !reader.Is( ';' ) )
                    {
                        throw new ParseException( "Expected ;", reader.SourceFile.GetToken( reader.CurrentIndex ) );
                    }

                    reader.Eat( ';' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                reader.Eat( '}' );
                conditionBlocks.Add( eCond, eBody.ToArray() );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            List < BadCExpression > elseBody = new List < BadCExpression >();

            if ( reader.Is( "else" ) )
            {
                reader.Eat( "else" );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                reader.Eat( '{' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                while ( !reader.Is( '}' ) )
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    elseBody.Add( ParseExpression( reader, context, null ) );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( !reader.Is( ';' ) )
                    {
                        throw new ParseException( "Expected ;", reader.SourceFile.GetToken( reader.CurrentIndex ) );
                    }

                    reader.Eat( ';' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                reader.Eat( '}' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            return new BadCIfExpression( conditionBlocks, elseBody.ToArray(), token );
        }

        private BadCExpression ParseInvocation(
            SourceReader reader,
            BadCEmitContext context,
            BadCExpression callSite,
            SourceToken token )
        {
            List < BadCExpression > args = new List < BadCExpression >();

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            if ( !reader.Is( ')' ) )
            {
                do
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( reader.Is( ',' ) )
                    {
                        reader.Eat( ',' );
                    }

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    args.Add( ParseExpression( reader, context, null, int.MaxValue - 1 ) );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }
                while ( reader.Is( ',' ) );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( ')' );

            return new BadCInvocationExpression( callSite, args.ToArray(), token );
        }

        private void ParseStructure(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList,
            bool isTemplate )
        {
            SourceReaderToken structName = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            BadCTemplateType[] templateTypes = Array.Empty < BadCTemplateType >();

            if ( isTemplate )
            {
                templateTypes = ParseTemplateTypes( reader, sectionWriter );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblyElementVisibility visibility = AssemblyElementVisibility.Local;

            if ( reader.Is( ':' ) )
            {
                reader.Eat( ':' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                SourceReaderToken visibilityToken = reader.ParseWord();
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                visibility = ( AssemblyElementVisibility )Enum.Parse(
                                                                     typeof( AssemblyElementVisibility ),
                                                                     visibilityToken.StringValue,
                                                                     true
                                                                    );
            }

            BadCElementExporter elementExporter =
                sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

            AssemblySymbol structureSymbol = new AssemblySymbol(
                                                                sectionWriter.AssemblyName,
                                                                sectionWriter.SectionName,
                                                                structName.StringValue
                                                               );

            BadCType structureType = elementExporter.AddType(
                                                             structureSymbol,
                                                             visibility,
                                                             structName.SourceToken,
                                                             false,
                                                             isTemplate
                                                            );

            if ( isTemplate )
            {
                structureType.SetTemplateTypes( templateTypes.ToArray() );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            reader.Eat( '{' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                ParseStructureMember( reader, sectionWriter, taskList, structureSymbol );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            reader.Eat( '}' );
            reader.SkipWhitespaceAndNewlineAndComments( "//" );
        }

        private void ParseStructureMember(
            SourceReader reader,
            CodeSectionWriter sectionWriter,
            PostSegmentParseTasks taskList,
            AssemblySymbol typeName )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            SourceReaderToken baseType = reader.ParseWord();
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            bool isTemplate = false;

            if ( baseType.StringValue == "template" )
            {
                isTemplate = true;
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                baseType = reader.ParseWord();
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            BadCElementExporter elementExporter =
                sectionWriter.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

            BadCType? instanceType = elementExporter.GetType( typeName );

            BadCType type = ParseType(
                                      reader,
                                      baseType,
                                      sectionWriter,
                                      isTemplate || ( instanceType?.IsTemplate ?? false ),
                                      out BadCType[] _
                                     );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            SourceReaderToken name = reader.ParseWord();

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblyElementVisibility visibility = AssemblyElementVisibility.Local;

            if ( reader.Is( '(' ) || reader.Is( '<' ) )
            {
                AssemblySymbol sym = new AssemblySymbol(
                                                        sectionWriter.AssemblyName,
                                                        sectionWriter.SectionName,
                                                        typeName.SymbolName + "_" + name.StringValue
                                                       );

                if ( instanceType == null )
                {
                    throw new ParseException(
                                             "Could not find type '" + typeName + "'",
                                             type.SourceToken
                                            );
                }

                ParseFunction(
                              reader,
                              sym,
                              sectionWriter,
                              false,
                              isTemplate,
                              type,
                              baseType.SourceToken,
                              taskList,
                              true,
                              instanceType,
                              name.StringValue
                             );
            }
            else
            {
                //Variable
                if ( reader.Is( ':' ) )
                {
                    reader.Eat( ':' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    SourceReaderToken visibilityToken = reader.ParseWord();

                    visibility = ( AssemblyElementVisibility )Enum.Parse(
                                                                         typeof( AssemblyElementVisibility ),
                                                                         visibilityToken.StringValue,
                                                                         true
                                                                        );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                reader.Eat( ';' );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( instanceType == null )
                {
                    throw new ParseException(
                                             "Could not find type '" + typeName + "'",
                                             type.SourceToken
                                            );
                }

                instanceType.AddMember( new BadCTypeField( name.StringValue, visibility, type ) );
            }
        }

        private BadCExpression ParseTemplateInvocation(
            SourceReader reader,
            BadCEmitContext context,
            BadCExpression callSite,
            SourceToken token,
            List < BadCType > templateArgs )
        {
            List < BadCExpression > args = new List < BadCExpression >();

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            if ( !reader.Is( ')' ) )
            {
                do
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( reader.Is( ',' ) )
                    {
                        reader.Eat( ',' );
                    }

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    args.Add( ParseExpression( reader, context, null, int.MaxValue - 1 ) );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }
                while ( reader.Is( ',' ) );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( ')' );

            return new BadCTemplateInvocation(
                                              callSite,
                                              templateArgs.ToArray(),
                                              args.ToArray(),
                                              token
                                             );
        }

        private BadCTemplateType[] ParseTemplateTypes( SourceReader reader, CodeSectionWriter sectionWriter )
        {
            List < BadCTemplateType > types = new List < BadCTemplateType >();
            reader.Eat( '<' );

            while ( true )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                SourceReaderToken typeName = reader.ParseWord();
                BadCType type = ParseType( reader, typeName, sectionWriter, true, out BadCType[] _ );

                if ( type is not BadCTemplateType templateType )
                {
                    throw new ParseException( "Expected template type", typeName.SourceToken );
                }

                types.Add( templateType );

                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( !reader.Is( ',' ) )
                {
                    break;
                }

                reader.Eat( ',' );
            }

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '>' );

            return types.ToArray();
        }

        private BadCType ParseType(
            SourceReader reader,
            SourceReaderToken baseType,
            CodeSectionWriter writer,
            bool isTemplate,
            out BadCType[] templateArgs )
        {
            BadCType? ttype;
            templateArgs = Array.Empty < BadCType >();

            if ( Enum.TryParse( baseType.StringValue, true, out BadCPrimitiveTypes type ) )
            {
                ttype = BadCType.GetPrimitive( type );
            }
            else
            {
                BadCElementExporter elementExporter =
                    writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

                reader.ResetTo( baseType );
                baseType = reader.ParseWord();
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( reader.Is( "::" ) )
                {
                    reader.ResetTo( baseType );

                    AssemblySymbol sym = reader.ParseSymbol(
                                                            writer.AssemblyName,
                                                            writer.SectionName,
                                                            out SourceToken _
                                                           );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    if ( reader.Is( '<' ) )
                    {
                        reader.Eat( '<' );

                        List < BadCType > genericTypes = new List < BadCType >();

                        while ( !reader.Is( '>' ) )
                        {
                            reader.SkipWhitespaceAndNewlineAndComments( "//" );

                            genericTypes.Add(
                                             ParseType(
                                                       reader,
                                                       reader.ParseWord(),
                                                       writer,
                                                       isTemplate,
                                                       out BadCType[] _
                                                      )
                                            );

                            reader.SkipWhitespaceAndNewlineAndComments( "//" );

                            if ( !reader.Is( '>' ) )
                            {
                                reader.Eat( ',' );
                            }

                            reader.SkipWhitespaceAndNewlineAndComments( "//" );
                        }

                        reader.Eat( '>' );
                        templateArgs = genericTypes.ToArray();
                    }

                    ttype = elementExporter.GetType( sym );
                }
                else if ( reader.Is( '<' ) )
                {
                    reader.Eat( '<' );

                    List < BadCType > genericTypes = new List < BadCType >();

                    while ( !reader.Is( '>' ) )
                    {
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );

                        genericTypes.Add(
                                         ParseType( reader, reader.ParseWord(), writer, isTemplate, out BadCType[] _ )
                                        );

                        reader.SkipWhitespaceAndNewlineAndComments( "//" );

                        if ( !reader.Is( '>' ) )
                        {
                            reader.Eat( ',' );
                        }

                        reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    }

                    reader.Eat( '>' );

                    if ( baseType.StringValue == "Action" )
                    {
                        ttype = new BadCFunctionType( null, genericTypes.ToArray() );
                    }
                    else if ( baseType.StringValue == "Func" )
                    {
                        ttype = new BadCFunctionType( genericTypes[0], genericTypes.Skip( 1 ).ToArray() );
                    }
                    else
                    {
                        ttype = elementExporter.GetType(
                                                        new AssemblySymbol(
                                                                           writer.AssemblyName,
                                                                           writer.SectionName,
                                                                           baseType.StringValue
                                                                          )
                                                       );
                    }

                    templateArgs = genericTypes.ToArray();
                }
                else
                {
                    if ( baseType.StringValue == "Action" )
                    {
                        ttype = new BadCFunctionType();
                    }
                    else
                    {
                        ttype = elementExporter.
                            GetType(
                                    new AssemblySymbol( writer.AssemblyName, writer.SectionName, baseType.StringValue )
                                   );
                    }

                    if ( ttype == null && isTemplate )
                    {
                        ttype = elementExporter.GetTemplateType(
                                                                new AssemblySymbol(
                                                                     writer.AssemblyName,
                                                                     writer.SectionName,
                                                                     baseType.StringValue
                                                                    )
                                                               );
                    }
                }
            }

            int ptr = 0;

            if ( ttype == null )
            {
                throw new ParseException(
                                         "Could not find type '" + baseType.StringValue + "'",
                                         baseType.SourceToken
                                        );
            }

            while ( reader.Is( '*' ) )
            {
                reader.Eat( '*' );
                ptr++;
            }

            if ( ptr > 0 )
            {
                return ttype.GetPointerType( ptr );
            }

            return ttype;
        }

        private BadCExpression ParseValue(
            SourceReader reader,
            BadCEmitContext context,
            int precedence )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            if ( reader.Is( '(' ) )
            {
                reader.Eat( '(' );
                BadCExpression e = ParseExpression( reader, context, null );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                reader.Eat( ')' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                return e;
            }

            SourceReaderToken op = reader.ParseUntilWhiteSpaceOrWordOrDigitOrSeparator();
            BadCUnaryOperator? opr = BadCUnaryOperator.Get( precedence, op.StringValue );

            if ( opr == null )
            {
                reader.ResetTo( op );
            }
            else
            {
                return opr.Parse( context, reader, this, op.SourceToken );
            }

            if ( reader.IsWordStart() )
            {
                SourceReaderToken word = reader.ParseWord();
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( word.StringValue == "return" )
                {
                    if ( context.Function.ReturnType == BadCType.GetPrimitive( BadCPrimitiveTypes.Void ) )
                    {
                        return new BadCReturnExpression( null, word.SourceToken );
                    }

                    return new BadCReturnExpression(
                                                    ParseExpression( reader, context, null, int.MaxValue - 1 ),
                                                    word.SourceToken
                                                   );
                }

                if ( word.StringValue == "cast" )
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( '<' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    SourceReaderToken baseType = reader.ParseWord();

                    BadCType type = ParseType(
                                              reader,
                                              baseType,
                                              context.Writer,
                                              context.AllowTemplateTypes,
                                              out BadCType[] _
                                             );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( '>' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( '(' );
                    BadCExpression e = ParseExpression( reader, context, null );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( ')' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    return new BadCCastExpression( word.SourceToken, e, type );
                }

                if ( word.StringValue == "sizeof" )
                {
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( '(' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    SourceReaderToken baseType = reader.ParseWord();

                    BadCType type = ParseType(
                                              reader,
                                              baseType,
                                              context.Writer,
                                              context.AllowTemplateTypes,
                                              out BadCType[] _
                                             );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                    reader.Eat( ')' );
                    reader.SkipWhitespaceAndNewlineAndComments( "//" );

                    return new BadCSizeOfExpression( word.SourceToken, type );
                }

                if ( word.StringValue == "halt" )
                {
                    return new BadCHaltExpression( word.SourceToken );
                }

                if ( word.StringValue == "if" )
                {
                    return ParseIfBranch( reader, context, word.SourceToken );
                }

                if ( word.StringValue == "while" )
                {
                    return ParseWhileLoop( reader, context, word.SourceToken );
                }

                if ( reader.Is( "::" ) )
                {
                    reader.ResetTo( word );

                    reader.ParseSymbol(
                                       context.Writer.AssemblyName,
                                       context.Writer.SectionName,
                                       out SourceToken _
                                      );

                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                }

                if ( reader.IsWordStart() || reader.Is( '*' ) || reader.Is( '<' ) )
                {
                    bool continueTypeParsing = precedence == int.MaxValue;

                    if ( continueTypeParsing &&
                         reader.Is( '*' ) ) // Account for edge case: i *= value; (i is not a type in this case)
                    {
                        int resetIdx = reader.CurrentIndex;
                        reader.Eat( '*' );
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );
                        continueTypeParsing = !reader.IsSymbol || reader.Is( '*' );
                        reader.ResetTo( resetIdx );
                    }

                    if ( continueTypeParsing && reader.Is( '<' ) )
                    {
                        int resetIdx = reader.CurrentIndex;
                        reader.Eat( '<' );
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );
                        continueTypeParsing = !reader.IsSymbol;

                        if ( continueTypeParsing )
                        {
                            while ( !reader.Is( '>' ) )
                            {
                                SourceReaderToken token = reader.ParseWord();

                                try
                                {
                                    ParseType( reader, token, context.Writer, context.AllowTemplateTypes, out BadCType[] _ );
                                }
                                catch ( Exception )
                                {
                                    continueTypeParsing = false;

                                    break;
                                }

                                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                                if ( reader.Is( '>' ) )
                                {
                                    break;
                                }

                                reader.Eat( ',' );
                                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                            }

                            if ( continueTypeParsing )
                            {
                                if ( reader.Is( '>' ) )
                                {
                                    reader.Eat( '>' );
                                    reader.SkipWhitespaceAndNewlineAndComments( "//" );
                                    continueTypeParsing = !reader.IsSymbol;
                                }
                                else
                                {
                                    continueTypeParsing = false;
                                }
                            }
                        }

                        reader.ResetTo( resetIdx );
                    }

                    if ( continueTypeParsing )
                    {
                        BadCType type = ParseType( reader, word, context.Writer, context.AllowTemplateTypes, out BadCType[] templateArgs );
                        reader.SkipWhitespaceAndNewlineAndComments( "//" );

                        AssemblySymbol sym = reader.ParseSymbol(
                                                                context.Function.Symbol.AssemblyName,
                                                                context.Function.Symbol.SectionName,
                                                                out SourceToken token
                                                               );

                        if ( sym.AssemblyName != context.Function.Symbol.AssemblyName ||
                             sym.SectionName != context.Function.Symbol.SectionName )
                        {
                            throw new ParseException(
                                                     $"Declared Variable {sym} is not in the same section as the function {context.Function}",
                                                     token
                                                    );
                        }

                        BadCVariableDeclaration decl =
                            new BadCVariableDeclaration( sym, type, word.SourceToken );

                        decl = decl.ResolveTemplateTypes( templateArgs, context.TaskList, context.Writer );

                        context.Function.AddLocal( decl );

                        return decl;
                    }
                }

                reader.ResetTo( word );

                AssemblySymbol symbol = reader.ParseSymbol(
                                                           context.Function.Symbol.AssemblyName,
                                                           context.Function.Symbol.SectionName,
                                                           out SourceToken t
                                                          );

                return new BadCVariable( symbol, t );
            }

            if ( reader.IsHexNumberStart() )
            {
                SourceReaderToken token = reader.ParseHexNumber();
                decimal d = ( decimal )token.ParsedValue;

                return new BadCNumber( d, token.SourceToken );
            }

            if ( reader.IsNumberStart() )
            {
                SourceReaderToken token = reader.ParseNumber();
                decimal d = ( decimal )token.ParsedValue;

                return new BadCNumber( d, token.SourceToken );
            }

            if (reader.Is('\''))
            {
                SourceReaderToken token = reader.ParseChar();
                char c = (char)token.ParsedValue;

                return new BadCNumber(c, token.SourceToken);
            }
            
            if (reader.Is('"'))
            {
                SourceReaderToken token = reader.ParseString();
                DataSectionWriter dataWriter =
                    context.Writer.AssemblyWriter.GetDataWriter(context.Writer.SectionName + "_DATA", "data_raw",
                        true);

                AssemblySymbol sym = new AssemblySymbol(dataWriter.AssemblyName, dataWriter.SectionName,
                    "str_" + dataWriter.CurrentSize);
                
                string s = token.ParsedValue.ToString() + '\0';
                byte[] data = s.ToCharArray().SelectMany( BitConverter.GetBytes ).ToArray();
                dataWriter.AddData(sym.SymbolName, AssemblyElementVisibility.Assembly, data);

                return new BadCAddressOfExpression(new BadCVariable(sym, token.SourceToken), token.SourceToken);

            }

            throw new ParseException( "Expected value", reader.SourceFile.GetToken( reader.CurrentIndex ) );
        }

        private BadCVariableDeclaration ParseVariableDeclaration(
            SourceReader reader,
            AssemblySymbol funcSymbol,
            CodeSectionWriter writer,
            PostSegmentParseTasks taskList,
            bool isTemplate = false )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            SourceReaderToken argType = reader.ParseWord();
            BadCType type = ParseType( reader, argType, writer, isTemplate, out BadCType[] templateArgs );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            AssemblySymbol argName = reader.ParseSymbol(
                                                        funcSymbol.AssemblyName,
                                                        funcSymbol.SectionName,
                                                        out SourceToken _
                                                       );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );

            BadCVariableDeclaration decl = new BadCVariableDeclaration( argName, type, argType.SourceToken );

            return decl.ResolveTemplateTypes( templateArgs, taskList, writer );
        }

        private BadCExpression ParseWhileLoop(
            SourceReader reader,
            BadCEmitContext context,
            SourceToken token )
        {
            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '(' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            BadCExpression cond = ParseExpression( reader, context, null, int.MaxValue - 1 );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( ')' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            reader.Eat( '{' );

            reader.SkipWhitespaceAndNewlineAndComments( "//" );
            List < BadCExpression > body = new List < BadCExpression >();

            while ( !reader.Is( '}' ) )
            {
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
                body.Add( ParseExpression( reader, context, null ) );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );

                if ( !reader.Is( ';' ) )
                {
                    throw new ParseException( "Expected ;", reader.SourceFile.GetToken( reader.CurrentIndex ) );
                }

                reader.Eat( ';' );
                reader.SkipWhitespaceAndNewlineAndComments( "//" );
            }

            reader.Eat( '}' );

            return new BadCWhileExpression( cond, body.ToArray(), token );
        }

        #endregion

    }

}
