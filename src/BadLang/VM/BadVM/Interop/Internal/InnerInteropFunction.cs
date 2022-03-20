using BadVM.Core;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.Memory;

namespace BadVM.Interop.Internal;

public abstract class InnerInteropFunction
{

    public AssemblySymbol Name { get; }

    #region Public

    public abstract string GetSignature();

    public abstract void Invoke( VirtualCore core, MemoryBus bus );

    #endregion

    #region Protected

    protected InnerInteropFunction( string name )
    {
        Name = AssemblySymbol.Parse( name );
    }

    protected T ReadArgumentValue < T >( VirtualCore core, MemoryBus bus )
    {
        return InteropConverters.Read < T >( core, bus );
    }

    protected object ReadArgumentValue( VirtualCore core, MemoryBus bus, Type t )
    {
        return InteropConverters.Read( core, bus, t );
    }

    protected void WriteReturnValue( VirtualCore core, MemoryBus bus, object o )
    {
        InteropConverters.Write( core, bus, o );
    }

    #endregion

}
