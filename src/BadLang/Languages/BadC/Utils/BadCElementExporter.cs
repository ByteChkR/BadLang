using System.Text;

using BadC.Expressions.Values.Symbols;
using BadC.Functions;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Members;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Utils;

public class BadCElementExporter : ICompilationExporter
{

    private Dictionary < AssemblySymbol, BadCType > m_Types = new Dictionary < AssemblySymbol, BadCType >();
    private readonly List < FunctionInfo > m_Functions = new List < FunctionInfo >();

    public IEnumerable < BadCType > Types => m_Types.Values;

    #region Public

    public FunctionInfo AddFunction(
        AssemblySymbol symbol,
        BadCType returnType,
        BadCVariableDeclaration[] parameterSize,
        AssemblyElementVisibility visibility,
        bool isExtern,
        bool isTemplate,
        bool isInstance,
        SourceToken token,
        CodeSectionWriter writer,
        params uint[] hash )
    {
        FunctionInfo info = new FunctionInfo(
                                             symbol,
                                             returnType,
                                             parameterSize,
                                             visibility,
                                             isExtern,
                                             isInstance,
                                             isTemplate,
                                             token,
                                             writer
                                            );

        foreach ( uint h in hash )
        {
            info.SetHash( h );
        }

        if ( m_Functions.Any( x => x.Symbol == info.Symbol ) )
        {
            if ( !isExtern )
            {
                throw new ArgumentException( "Function already exists" );
            }
        }

        m_Functions.Add( info );

        return info;
    }

    public BadCustomCType AddType(
        AssemblySymbol typeName,
        AssemblyElementVisibility visibility,
        SourceToken token,
        bool isExtern,
        bool isTemplate )
    {
        if ( m_Types.ContainsKey( typeName ) )
        {
            throw new Exception( "Type already exists" );
        }

        BadCustomCType type = new BadCustomCType( typeName, visibility, token, isExtern, isTemplate );
        m_Types.Add( typeName, type );

        return type;
    }

    public void Export( string outputBinary )
    {
        string outputHeader = Path.ChangeExtension( outputBinary, "header.basm" );

        StringBuilder sb = new StringBuilder();

        StringBuilder funcSb = new StringBuilder();
        bool saveExport = ExportFunctions( funcSb, Path.GetFileNameWithoutExtension( outputBinary ) );

        if ( saveExport )
        {
            sb.AppendLine( funcSb.ToString() );
        }

        Dictionary < string, Dictionary < string, List < BadCType > > > typeMap =
            new Dictionary < string, Dictionary < string, List < BadCType > > >();

        foreach ( KeyValuePair < AssemblySymbol, BadCType > type in m_Types )
        {
            if ( !typeMap.ContainsKey( type.Key.AssemblyName ) )
            {
                typeMap[type.Key.AssemblyName] = new Dictionary < string, List < BadCType > >();
            }

            if ( !typeMap[type.Key.AssemblyName].ContainsKey( type.Key.SectionName ) )
            {
                typeMap[type.Key.AssemblyName][type.Key.SectionName] = new List < BadCType >();
            }

            typeMap[type.Key.AssemblyName][type.Key.SectionName].Add( type.Value );
        }

        foreach ( KeyValuePair < string, Dictionary < string, List < BadCType > > > asmTypes in typeMap )
        {
            foreach ( KeyValuePair < string, List < BadCType > > sectionTypes in asmTypes.Value )
            {
                int memberExports = 0;
                StringBuilder sb2 = new StringBuilder();
                sb2.AppendLine();

                sb2.AppendLine(
                               $".code {Path.GetFileNameWithoutExtension( outputBinary )}_{sectionTypes.Key}:TypeHeaders code_raw BadC"
                              );

                sb2.AppendLine( "{" );

                foreach ( BadCType type in sectionTypes.Value )
                {
                    if ( type.Visibility == AssemblyElementVisibility.Export && !type.IsExtern )
                    {
                        sb2.AppendLine( "\textern struct " + type.TypeName.SymbolName + " : assembly" );
                        sb2.AppendLine( "\t{" );

                        foreach ( BadCTypeMember member in type.Members )
                        {
                            if ( member.Visibility != AssemblyElementVisibility.Export )
                            {
                                continue;
                            }

                            memberExports++;

                            if ( member is BadCTypeField field )
                            {
                                sb2.AppendLine(
                                               $"\t\t{field.FieldType} {field.Name} {type.GetMemberOffset( field.Name )};"
                                              );
                            }
                            else if ( member is BadCTypeMethod method )
                            {
                                sb2.AppendLine(
                                               $"\t\t{method.FunctionInfo.ToString( method.Name )};"
                                              );
                            }
                            else
                            {
                                throw new Exception( "Unknown member type" );
                            }
                        }

                        sb2.AppendLine( "\t}" );
                    }
                }

                sb2.AppendLine( "}" );

                if ( memberExports != 0 )
                {
                    saveExport = true;
                    sb.Append( sb2 );
                }
            }
        }

        if ( saveExport )
        {
            File.WriteAllText( outputHeader, sb.ToString() );
        }
    }


    public FunctionInfo? GetInfo( AssemblySymbol symbol )
    {
        return m_Functions.FirstOrDefault( f => f.Symbol == symbol );
    }

    public BadCType GetTemplateType( AssemblySymbol typeName )
    {
        return new BadCTemplateType(
                                    typeName,
                                    AssemblyElementVisibility.Local,
                                    new SourceToken( "UNKNOWN", 0 ),
                                    false
                                   );
    }

    public BadCType? GetType( AssemblySymbol sym )
    {
        BadCType type = m_Types.FirstOrDefault( t => t.Key == sym ).Value;

        return type;
    }

    public void UpdateType( BadCType type )
    {
        m_Types[type.TypeName] = type;
    }

    #endregion

    #region Private

    private static void EmitHeader( FunctionInfo func, StringBuilder sb )
    {
        sb.AppendLine( $"\textern {func} : export;" );
    }

    private bool ExportFunctions( StringBuilder sb, string name )
    {
        int functionExports = 0;
        sb.AppendLine();
        sb.AppendLine( $".code {name}:Headers code_raw BadC\n{{" );

        foreach ( FunctionInfo function in m_Functions )
        {
            if ( function.Visibility == AssemblyElementVisibility.Export && !function.IsExtern && !function.IsInstance )
            {
                functionExports++;
                EmitHeader( function, sb );
            }
        }

        sb.AppendLine( "\n}" );
        sb.AppendLine();

        return functionExports != 0;
    }

    #endregion

}
