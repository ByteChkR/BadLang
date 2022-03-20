using BadC.Templates;
using BadC.Types.Members;

using BadVM.Shared.AssemblyFormat;

namespace BadC.Types.Primitives;

public class BadCPointerType : BadCType
{

    public BadCType BaseType { get; }

    public override bool IsResolved => base.IsResolved && BaseType.IsResolved;

    public override IEnumerable < BadCTypeMember > Members => Enumerable.Empty < BadCTypeMember >();

    public override AssemblySymbol TypeName => BaseType.TypeName;

    public override bool IsPrimitivePointer => BaseType.PrimitiveType != null;

    public override int Size => ( int )BadCPrimitiveTypes.I64;

    #region Public

    public BadCPointerType( BadCType baseType, int pointerLevel ) : base(
                                                                         baseType.Visibility,
                                                                         baseType.SourceToken,
                                                                         false,
                                                                         false,
                                                                         pointerLevel
                                                                        )
    {
        BaseType = baseType;
        SetTemplateTypes(baseType.TemplateTypes);
    }

    public override void AddMember( BadCTypeMember member )
    {
        throw new InvalidOperationException( "Can not add member to pointer type" );
    }

    public override BadCType ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        return new BadCPointerType( BaseType.ResolveTemplate( templateContext ), PointerLevel );
    }

    public override void SetMembers( BadCTypeMember[] member )
    {
        BaseType.SetMembers(member);
    }

    public override string ToString()
    {
        return BaseType + new string( '*', PointerLevel );
    }

    public override bool TryGetBaseType( out BadCType baseType )
    {
        if ( PointerLevel == 1 )
        {
            baseType = BaseType;

            return true;
        }

        baseType = new BadCPointerType( BaseType, PointerLevel - 1 );

        return true;
    }

    #endregion

    #region Protected

    protected override bool Equals( BadCType other )
    {
        return other is BadCPointerType ct &&
               ( PointerLevel != 0 && ct.PointerLevel != 0 ||
                 BaseType == ct.BaseType && PointerLevel == ct.PointerLevel );
    }

    #endregion

}
