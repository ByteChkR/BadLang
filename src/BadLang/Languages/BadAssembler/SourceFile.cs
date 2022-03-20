using BadVM.Shared;

namespace BadAssembler;

public class SourceFile
{

    public string FileName { get; }

    public string Source { get; }

    #region Public

    public SourceFile( string fileName )
    {
        FileName = fileName;
        Source = File.ReadAllText( fileName );
    }

    public SourceToken GetToken( int index )
    {
        return new SourceToken( FileName, index );
    }

    #endregion

}
