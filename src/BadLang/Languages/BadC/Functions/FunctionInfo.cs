using BadC.Expressions;
using BadC.Expressions.Values.Symbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;
using BadVM.Shared.Logging;

namespace BadC.Functions
{

    public class FunctionInfo
    {

        public CodeSectionWriter CodeSectionWriter { get; }
        private static LogMask s_LogMask = new LogMask( "Template Resolver" );

        private List < BadCVariableDeclaration > m_Locals = new List < BadCVariableDeclaration >();
        private List < uint > m_HashList = new List < uint >();

        private readonly AssemblySymbol m_Symbol;

        public bool IsResolved =>
            TemplateTypes.All( x => x is not BadCTemplateType ) &&
            ParameterSize.All( x => x.Type is not BadCTemplateType ) &&
            m_Locals.All( x => x.Type is not BadCTemplateType ) &&
            ReturnType is not BadCTemplateType;

        public BadCType[] TemplateTypes { get; private set; } = Array.Empty < BadCType >();

        public BadCExpression[] TemplateExpressions { get; private set; } = Array.Empty < BadCExpression >();

        public AssemblySymbol Symbol
        {
            get
            {
                if ( m_HashList.Count == 0 )
                {
                    return m_Symbol;
                }

                return new AssemblySymbol(
                                          m_Symbol.AssemblyName,
                                          m_Symbol.SectionName,
                                          m_Symbol.SymbolName + "_" + string.Join( "_", m_HashList )
                                         );
            }
        }

        public bool IsExtern { get; }

        public bool IsInstance { get; }

        public bool IsTemplate { get; }

        public bool IsEmitted { get; private set; }

        public AssemblyElementVisibility Visibility { get; }

        public BadCType ReturnType { get; }

        public SourceToken Token { get; }

        public BadCVariableDeclaration[] ParameterSize { get; }

        public BadCVariableDeclaration[] LocalSize => m_Locals.ToArray();

        public int TotalLocalSize => LocalSize.Sum( x => ( int )x.Type.Size );

        public int TotalParameterSize => ParameterSize.Sum( x => ( int )x.Type.Size );

        #region Public

        public FunctionInfo(
            AssemblySymbol symbol,
            BadCType returnType,
            BadCVariableDeclaration[] parameterSize,
            AssemblyElementVisibility visibility,
            bool isExtern,
            bool isInstance,
            bool isTemplate,
            SourceToken token,
            CodeSectionWriter codeSectionWriter )
        {
            m_Symbol = symbol;
            ReturnType = returnType;
            ParameterSize = parameterSize;
            Visibility = visibility;
            IsExtern = isExtern;
            IsInstance = isInstance;
            IsTemplate = isTemplate;
            Token = token;
            CodeSectionWriter = codeSectionWriter;
        }

        public void AddLocal( BadCVariableDeclaration decl )
        {
            m_Locals.Add( decl );
        }

        public void AddTemplateTypes( BadCType[] templateTypes )
        {
            TemplateTypes = templateTypes;
        }

        public IEnumerable < uint > GetHashes()
        {
            return m_HashList;
        }

        public BadCVariableDeclaration GetNamedVar( AssemblySymbol name, AssemblyWriter writer )
        {
            BadCVariableDeclaration? pDecl = ParameterSize.FirstOrDefault( x => x.Name == name );

            if ( pDecl != null )
            {
                return pDecl;
            }

            BadCVariableDeclaration? lDecl = m_Locals.FirstOrDefault( x => x.Name == name );

            if ( lDecl != null )
            {
                return lDecl;
            }

            BadCElementExporter exporter = writer.CompilationData.GetData < BadCElementExporter >();
            FunctionInfo? info = exporter.GetInfo( name );

            if ( info != null )
            {
                if ( !info.IsExtern && !info.IsVisibleFrom( Symbol ) )
                {
                    throw new SymbolNotFoundException( $"Symbol {name} is not visible from {this}" );
                }

                return new BadCFunctionDeclaration( info, info.Token );
            }

            AssemblyElement elem = writer.GetElement( name );

            if ( !elem.IsVisibleFrom( Symbol ) )
            {
                throw new SymbolNotFoundException( $"Symbol {name} is not visible from {this}" );
            }

            BadCType type;

            switch ( elem.Size )
            {
                case 1:
                    type = BadCType.GetPrimitive( BadCPrimitiveTypes.I8 );

                    break;

                case 2:
                    type = BadCType.GetPrimitive( BadCPrimitiveTypes.I16 );

                    break;

                case 4:
                    type = BadCType.GetPrimitive( BadCPrimitiveTypes.I32 );

                    break;

                case 8:
                    type = BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );

                    break;

                default:
                    type = BadCType.GetPrimitive( BadCPrimitiveTypes.Void ).GetPointerType();

                    break;
            }

            return new BadCVariableDeclaration( elem.Name, type, new SourceToken() );
        }

        public AssemblySymbol GetUnhashedSymbol()
        {
            return m_Symbol;
        }

        public bool IsVisibleFrom( AssemblySymbol symbol )
        {
            return Visibility == AssemblyElementVisibility.Export ||
                   Visibility == AssemblyElementVisibility.Assembly && symbol.AssemblyName == Symbol.AssemblyName ||
                   Visibility == AssemblyElementVisibility.Local &&
                   symbol.AssemblyName == Symbol.AssemblyName &&
                   symbol.SectionName == Symbol.SectionName;
        }

        public void SetEmitted()
        {
            IsEmitted = true;
        }

        public void SetHash( uint hash )
        {
            m_HashList.Add( hash );
        }

        public void SetTemplateExpressions( BadCExpression[] templateExpressions )
        {
            TemplateExpressions = templateExpressions;
        }

        public override string ToString()
        {
            return ToString( Symbol.ToString() );
        }

        public string ToString( string name )
        {
            if ( IsInstance )
            {
                return
                    $"{ReturnType} {name}({string.Join( ", ", ParameterSize.Skip( 1 ).Select( x => x.ToString() ) )})";
            }

            return $"{ReturnType} {name}({string.Join( ", ", ParameterSize.Select( x => x.ToString() ) )})";
        }

        public bool TryGetStackFrameOffset( AssemblySymbol name, out int offset )
        {
            int start = TotalParameterSize;

            foreach ( BadCVariableDeclaration param in ParameterSize )
            {
                start -= param.Type.Size;

                if ( param.Name == name )
                {
                    offset = start;

                    return true;
                }
            }

            start = -TotalLocalSize;

            for ( int i = m_Locals.Count - 1; i >= 0; i-- )
            {
                BadCVariableDeclaration local = m_Locals[i];

                if ( local.Name == name )
                {
                    offset = start;

                    return true;
                }

                start += local.Type.Size;
            }

            offset = 0;

            return false;
        }

        #endregion

    }

}
