using BadC.Templates;

using BadVM.Shared.AssemblyFormat;

namespace BadC.Types.Members;

public abstract class BadCTypeMember
{

    public string Name { get; }

    public AssemblyElementVisibility Visibility { get; }

    public abstract int Size { get; }

    public abstract bool IsResolved { get; }

    #region Public

    public abstract BadCTypeMember ResolveTemplate( BadCTemplateTypeContext templateContext );

    #endregion

    #region Protected

    protected BadCTypeMember( string name, AssemblyElementVisibility visibility )
    {
        Name = name;
        Visibility = visibility;
    }

    #endregion

}
