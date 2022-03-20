namespace BadVM.Shared;

public struct SourceToken
{

    public SourceToken( string fileName, int index )
    {
        FileName = fileName;
        Index = index;
    }

    public string FileName { get; set; }

    public int Index { get; set; }

    public override string ToString()
    {
        if ( !File.Exists( FileName ) )
        {
            return $"{FileName} at index {Index}";
        }

        string src = File.ReadAllText( FileName );
        int line = 1;

        for ( int i = 0; i < Index; i++ )
        {
            if ( src[i] == '\n' )
            {
                line++;
            }
        }

        return $"{FileName}:{line}";
    }

}
