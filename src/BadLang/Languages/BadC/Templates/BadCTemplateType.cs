using BadC.Types;
using BadC.Types.Members;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Templates;

public class BadCTemplateType : BadCType
{

    public override IEnumerable < BadCTypeMember > Members => Enumerable.Empty < BadCTypeMember >();

    public override AssemblySymbol TypeName { get; }

    public override bool IsResolved => false;

    public override int Size => throw new Exception( "Can not get size of template type" );

    #region Public

    public BadCTemplateType(
        AssemblySymbol typeName,
        AssemblyElementVisibility visibility,
        SourceToken sourceToken,
        bool isExtern,
        int pointerLevel = 0 ) : base(
                                      visibility,
                                      sourceToken,
                                      isExtern,
                                      false,
                                      pointerLevel
                                     )
    {
        TypeName = typeName;
    }

    public override void AddMember( BadCTypeMember member )
    {
        throw new Exception( "Can not get add member to template type" );
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Members.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ TypeName.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ PrimitiveType.GetHashCode();

            return hashCode;
        }
    }

    public override BadCType ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    public override bool TryGetBaseType( out BadCType baseType )
    {
        throw new InvalidOperationException( "Can not get base type of non-pointer type" );
    }

    #endregion

    #region Protected

    protected override bool Equals( BadCType other )
    {
        return other is BadCTemplateType otherType && TypeName == otherType.TypeName;
    }

    #endregion

}
