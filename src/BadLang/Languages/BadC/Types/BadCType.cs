using BadC.Templates;
using BadC.Types.Members;
using BadC.Types.Primitives;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Types;

public abstract class BadCType
{

    private static readonly Dictionary < BadCPrimitiveTypes, BadCType > s_Primitives =
        new Dictionary < BadCPrimitiveTypes, BadCType >
        {
            {
                BadCPrimitiveTypes.Void,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "Void" ), BadCPrimitiveTypes.Void )
            },
            {
                BadCPrimitiveTypes.I8,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "I8" ), BadCPrimitiveTypes.I8 )
            },
            {
                BadCPrimitiveTypes.I16,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "I16" ), BadCPrimitiveTypes.I16 )
            },
            {
                BadCPrimitiveTypes.I32,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "I32" ), BadCPrimitiveTypes.I32 )
            },
            {
                BadCPrimitiveTypes.I64,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "I64" ), BadCPrimitiveTypes.I64 )
            },
            {
                BadCPrimitiveTypes.F32,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "F32" ), BadCPrimitiveTypes.F32 )
            },
            {
                BadCPrimitiveTypes.F64,
                new BadCPrimitiveType( new AssemblySymbol( "BadC", "Primitives", "F64" ), BadCPrimitiveTypes.F64 )
            },
        };

    public virtual bool IsResolved => TemplateTypes.All( x => x is not BadCTemplateType );

    public virtual bool IsPrimitivePointer => false;
    public BadCType[] TemplateTypes { get; private set; } = Array.Empty < BadCType >();

    public static bool operator ==( BadCType? left, BadCType? right )
    {
        return Equals( left, right );
    }

    public static bool operator !=( BadCType? left, BadCType? right )
    {
        return !Equals( left, right );
    }

    public abstract IEnumerable < BadCTypeMember > Members { get; }

    public AssemblyElementVisibility Visibility { get; }

    public int PointerLevel { get; }

    public bool IsExtern { get; }

    public bool IsTemplate { get; }

    public SourceToken SourceToken { get; }

    public bool IsPointer => PointerLevel > 0;

    public abstract AssemblySymbol TypeName { get; }

    public abstract int Size { get; }

    public BadCPrimitiveTypes? PrimitiveType { get; }

    #region Public

    public static BadCType GetPrimitive( BadCPrimitiveTypes type )
    {
        return s_Primitives[type];
    }

    public abstract void AddMember( BadCTypeMember member );

    public abstract BadCType ResolveTemplate( BadCTemplateTypeContext templateContext );

    public abstract bool TryGetBaseType( out BadCType baseType );

    public override bool Equals( object obj )
    {
        if ( obj is BadCType other )
        {
            return Equals( other );
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Members.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ TypeName.GetHashCode();
            hashCode = ( hashCode * 397 ) ^ Size;
            hashCode = ( hashCode * 397 ) ^ PrimitiveType.GetHashCode();

            return hashCode;
        }
    }

    public int GetMemberOffset( string memberName )
    {
        if ( IsExtern )
        {
            foreach ( BadCTypeMember member in Members )
            {
                if ( member is BadCTypeField field )
                {
                    if ( field.Name == memberName )
                    {
                        if ( field.Offset == null )
                        {
                            throw new Exception( "External Field without manual offset not supported" );
                        }

                        return field.Offset.Value;
                    }
                }
            }

            return -1;
        }

        int off = 0;

        foreach ( BadCTypeMember member in Members )
        {
            if ( member is BadCTypeField field )
            {
                if ( field.Offset != null )
                {
                    throw new Exception( "Manual offset not supported" );
                }

                if ( field.Name == memberName )
                {
                    return off;
                }

                off += field.Size;
            }
        }

        return -1;
    }

    public BadCPointerType GetPointerType( int ptrLevel = 1 )
    {
        return new BadCPointerType( this, ptrLevel );
    }

    public bool IsPrimitive( BadCPrimitiveTypes primitiveType )
    {
        return PrimitiveType == primitiveType;
    }

    public virtual void SetMembers( BadCTypeMember[] member )
    {
        throw new Exception( $"Cannot set members on type {this}" );
    }

    public void SetTemplateTypes( BadCTemplateType[] templateTypes )
    {
        TemplateTypes = templateTypes.Cast < BadCType >().ToArray();
    }

    public override string ToString()
    {
        return TypeName.ToString();
    }

    #endregion

    #region Protected

    protected BadCType(
        AssemblyElementVisibility visibility,
        SourceToken sourceToken,
        bool isExtern,
        bool isTemplate,
        int pointerLevel = 0 )
    {
        Visibility = visibility;
        SourceToken = sourceToken;
        IsExtern = isExtern;
        IsTemplate = isTemplate;
        PointerLevel = pointerLevel;
    }

    protected BadCType(
        BadCPrimitiveTypes type,
        AssemblyElementVisibility visibility,
        SourceToken sourceToken,
        bool isExtern,
        int pointerLevel = 0 )
    {
        PrimitiveType = type;
        Visibility = visibility;
        SourceToken = sourceToken;
        IsExtern = isExtern;
        IsTemplate = false;
        PointerLevel = pointerLevel;
    }

    protected abstract bool Equals( BadCType other );

    protected void SetTemplateTypes( BadCType[] templateTypes )
    {
        TemplateTypes = templateTypes;
    }

    #endregion

}
