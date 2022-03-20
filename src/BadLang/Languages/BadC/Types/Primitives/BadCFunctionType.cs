using BadC.Templates;
using BadC.Types.Members;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Types.Primitives;

public class BadCFunctionType : BadCType
{

    public BadCType[] Signature { get; }

    public BadCType ReturnType { get; }

    public override IEnumerable < BadCTypeMember > Members => Enumerable.Empty < BadCTypeMember >();

    public override AssemblySymbol TypeName => new AssemblySymbol( "BadC", "Primitives", "Action" );

    public override int Size => ( int )BadCPrimitiveTypes.I64;

    #region Public

    public BadCFunctionType( BadCType? returnType = null, params BadCType[] signature ) : base(
         AssemblyElementVisibility.Assembly,
         new SourceToken( "INTERNAL", 0 ),
         false,
         false
        )
    {
        Signature = signature;
        ReturnType = returnType ?? GetPrimitive( BadCPrimitiveTypes.Void );
    }

    public override void AddMember( BadCTypeMember member )
    {
        throw new InvalidOperationException( "Can not add member to primitive type" );
    }

    public override BadCType ResolveTemplate( BadCTemplateTypeContext templateContext )
    {
        BadCType[] signature = new BadCType[Signature.Length];

        for ( int i = 0; i < signature.Length; i++ )
        {
            signature[i] = Signature[i].ResolveTemplate( templateContext );
        }

        return new BadCFunctionType( ReturnType?.ResolveTemplate( templateContext ), signature );
    }

    public override string ToString()
    {
        if ( ReturnType == GetPrimitive( BadCPrimitiveTypes.Void ) )
        {
            if ( Signature.Length == 0 )
            {
                return "Action";
            }

            return $"Action<{string.Join( ", ", Signature.Select( x => x.ToString() ) )}>";
        }
        else
        {
            if ( Signature.Length == 0 )
            {
                return $"Func<{ReturnType}>";
            }

            return $"Func<{ReturnType}, {string.Join( ", ", Signature.Select( x => x.ToString() ) )}>";
        }
    }

    public override bool TryGetBaseType( out BadCType baseType )
    {
        throw new InvalidOperationException( "Can not get base type of non-pointer type" );
    }

    #endregion

    #region Protected

    protected override bool Equals( BadCType other )
    {
        if ( other is BadCFunctionType o )
        {
            if ( ReturnType != o.ReturnType || Signature.Length != o.Signature.Length )
            {
                return false;
            }

            for ( int i = 0; i < Signature.Length; i++ )
            {
                if ( Signature[i] != o.Signature[i] )
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    #endregion

}
