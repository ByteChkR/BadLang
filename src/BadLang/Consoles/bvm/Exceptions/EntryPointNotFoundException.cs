using BadVM.Shared.Exceptions;

namespace bvm.Exceptions;

public class EntryPointNotFoundException : RuntimeException
{

    #region Public

    public EntryPointNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
