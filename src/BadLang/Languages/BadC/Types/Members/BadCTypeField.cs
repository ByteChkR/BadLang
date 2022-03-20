using BadC.Templates;

using BadVM.Shared.AssemblyFormat;

namespace BadC.Types.Members;

public class BadCTypeField : BadCTypeMember
{

    public BadCType FieldType { get; }

    public int? Offset { get; }

    public override bool IsResolved => FieldType.IsResolved && FieldType is not BadCTemplateType;

    public override int Size => FieldType.Size;

    #region Public

    public BadCTypeField(
        string name,
        AssemblyElementVisibility visibility,
        BadCType fieldType,
        int? offset = null ) : base( name, visibility )
    {
        FieldType = fieldType;
        Offset = offset;
    }

    public override BadCTypeMember ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        return new BadCTypeField(
                                 Name,
                                 Visibility,
                                 templateContext.ResolveType( FieldType ).ResolveTemplate( templateContext ),
                                 Offset
                                );
    }

    #endregion

}
