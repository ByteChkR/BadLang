using BadC.Templates;
using BadC.Types.Members;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Types.Primitives;

public class BadCPrimitiveType : BadCType
{

    public static int GetPrimitiveSize(BadCPrimitiveTypes type)
    {
        if (type == BadCPrimitiveTypes.F32) return sizeof(float);
        if (type == BadCPrimitiveTypes.F64) return sizeof(double);
        return (int) type;
    }
    
    public override IEnumerable < BadCTypeMember > Members => Enumerable.Empty < BadCTypeMember >();

    public override AssemblySymbol TypeName { get; }

    public override int Size => GetPrimitiveSize(PrimitiveType!.Value);

    #region Public

    public BadCPrimitiveType( AssemblySymbol typeName, BadCPrimitiveTypes primitiveType ) : base(
         primitiveType,
         AssemblyElementVisibility.Assembly,
         new SourceToken( "INTERNAL", 0 ),
         false
        )
    {
        TypeName = typeName;
    }

    public override void AddMember( BadCTypeMember member )
    {
        throw new InvalidOperationException( "Can not add member to primitive type" );
    }

    public override BadCType ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        return this;
    }

    public override string ToString()
    {
        return PrimitiveType.ToString();
    }

    public override bool TryGetBaseType( out BadCType baseType )
    {
        throw new InvalidOperationException( "Can not get base type of non-pointer type" );
    }

    #endregion

    #region Protected

    protected override bool Equals( BadCType other )
    {
        return other is BadCPrimitiveType && TypeName == other.TypeName && PrimitiveType == other.PrimitiveType;
    }

    #endregion

}
