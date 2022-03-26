namespace BadVM.Interop;

public class WrapperResult
{

    private readonly Dictionary < string, (string asm, string header) > m_Assemblies;

    public IEnumerable < string > AssemblyNames => m_Assemblies.Keys;

    public string GetAssemblySource( string assemblyName )
    {
        return m_Assemblies[assemblyName].asm;
    }

    public string GetAssemblyHeader( string assemblyName )
    {
        return m_Assemblies[assemblyName].header;
    }

    public WrapperResult( Dictionary < string, (string asm, string header) > assemblies )
    {
        m_Assemblies = assemblies;
    }

    public void ExportToFolder( string dir )
    {
        string asmDir = Path.Combine( dir, "Assemblies" );
        string headerDir = Path.Combine( dir, "Headers" );
        Directory.CreateDirectory( asmDir );
        Directory.CreateDirectory( headerDir );

        foreach ( string assemblyName in AssemblyNames )
        {
            string asmPath = Path.Combine( asmDir, assemblyName );
            string headerPath = Path.Combine( headerDir, assemblyName );
            string code = Path.Combine( asmPath, "code.basm" );
            string header = Path.Combine( headerPath, "header.basm" );
            Directory.CreateDirectory( asmPath );
            Directory.CreateDirectory( headerPath );
            File.WriteAllText( code, GetAssemblySource( assemblyName ) );
            File.WriteAllText( header, GetAssemblyHeader( assemblyName ) );
        }
    }

}
