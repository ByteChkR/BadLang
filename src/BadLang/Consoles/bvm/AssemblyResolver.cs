using BadVM.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.Logging;

namespace bvm
{

    public class AssemblyResolver
    {

        public static readonly LogMask LogMask = new LogMask( nameof( AssemblyResolver ) );
        private readonly string m_AssemblyPath;

        #region Public

        public AssemblyResolver( string assemblyPath )
        {
            m_AssemblyPath = Path.GetFullPath( assemblyPath );
        }

        public void Resolve( BadAssemblyResolveEventArgs args )
        {
            string[] libs = Directory.GetFiles( m_AssemblyPath, "*.bvm" );

            foreach ( string lib in libs )
            {
                byte[] data = File.ReadAllBytes( lib );

                if ( AssemblyReader.TryReadAssemblyName( data, out string name ) && name == args.AssemblyName )
                {
                    LogMask.LogMessage( $"Resolved assembly '{args.AssemblyName}' from '{lib}'" );
                    args.Resolve( data, lib );

                    return;
                }
            }
        }

        public override string ToString()
        {
            return $"Directory Resolver: {m_AssemblyPath}";
        }

        #endregion

    }

}
