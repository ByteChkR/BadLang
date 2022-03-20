using BadVM.Shared.Exceptions;

namespace bvm.Exceptions;

public class LoadDirectoryNotFoundException : RuntimeException
{

    #region Public

    public LoadDirectoryNotFoundException( string message ) : base( message )
    {
    }

    #endregion

}
