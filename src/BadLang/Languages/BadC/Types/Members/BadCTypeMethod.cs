using BadC.Functions;
using BadC.Templates;

namespace BadC.Types.Members;

public class BadCTypeMethod : BadCTypeMember
{

    public FunctionInfo FunctionInfo { get; }

    public override int Size => 0;

    public override bool IsResolved => FunctionInfo.IsResolved;

    #region Public

    public BadCTypeMethod( string originalName, FunctionInfo info ) : base( originalName, info.Visibility )
    {
        FunctionInfo = info;
    }

    public override BadCTypeMember ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        return new BadCTypeMethod( Name, FunctionInfo.ResolveTemplate( templateContext ) );
    }

    #endregion

}
