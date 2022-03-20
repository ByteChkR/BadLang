using BadVM.Shared.Exceptions;

namespace BadVM.Shared.Memory.Exceptions;

public class MemoryUnmappedException : RuntimeException
{

    #region Public

    public MemoryUnmappedException( string message, long address ) : base( message )
    {
    }

    #endregion

}
