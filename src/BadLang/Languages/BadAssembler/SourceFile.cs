using BadVM.Shared;
using BadVM.Shared.Logging;

namespace BadAssembler;

public class SourceFile
{
    private static readonly LogMask s_LogMask = new LogMask("SourceFile");

    public string FileName { get; }

    public string Source { get; private set; }

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

    public void Replace(int index, int length, string newStr)
    {
        if(index < 0 || index + length > Source.Length)
        {
            throw new ArgumentOutOfRangeException();
        }
        
        if(length<0)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (length < newStr.Length)
        {
            s_LogMask.Warning("Potential invalid source positions because the replace operation has a length that is less than the new string length.");
        }
        else
        {
            newStr += new string(' ', length - newStr.Length); //Pad the new string with spaces to the length of the old string.
        }

        Source = Source.Remove(index, length).Insert(index, newStr);

    }

    #endregion

}
