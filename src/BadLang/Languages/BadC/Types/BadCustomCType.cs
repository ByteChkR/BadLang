using BadC.Templates;
using BadC.Types.Members;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Types;

public class BadCustomCType : BadCType
{

    private readonly List < BadCTypeMember > m_Members = new List < BadCTypeMember >();

    public override IEnumerable < BadCTypeMember > Members => m_Members;

    public override AssemblySymbol TypeName { get; }

    public override int Size => m_Members.Sum( x => x.Size );

    #region Public

    public BadCustomCType(
        AssemblySymbol assemblySymbol,
        AssemblyElementVisibility visibility,
        SourceToken token,
        bool isExtern,
        bool isTemplate ) : base( visibility, token, isExtern, isTemplate )
    {
        TypeName = assemblySymbol;
    }

    public override void AddMember( BadCTypeMember member )
    {
        m_Members.Add( member );
    }

    public override BadCType ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        BadCType[] templateArgTypes = new BadCType[TemplateTypes.Length];

        for ( int i = 0; i < TemplateTypes.Length; i++ )
        {
            templateArgTypes[i] = templateContext.ResolveType( TemplateTypes[i] ).ResolveTemplate( templateContext );
        }

        BadCustomCType t = new BadCustomCType( TypeName, Visibility, SourceToken, IsExtern, IsTemplate );
        t.SetTemplateTypes( templateArgTypes );

        t.m_Members.AddRange( m_Members );

        BadCElementExporter exporter =
            templateContext.Writer.AssemblyWriter.CompilationData.GetData < BadCElementExporter >();

        exporter.UpdateType( t );

        return t;
    }

    public override void SetMembers( BadCTypeMember[] member )
    {
        m_Members.Clear();
        m_Members.AddRange( member );
    }

    public override bool TryGetBaseType( out BadCType baseType )
    {
        throw new InvalidOperationException( "Can not get base type of non-pointer type" );
    }

    #endregion

    #region Protected

    protected override bool Equals( BadCType other )
    {
        return other is BadCustomCType ct && ct.TypeName == TypeName;
    }

    #endregion

}
