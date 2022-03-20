namespace BadVM.AssemblyFormat
{

    public class BadAssemblyResolveEventArgs
    {

        public bool IsResolved => AssemblyBytes != null;

        public string AssemblyName { get; }

        public string? PathHint { get; private set; }

        internal byte[]? AssemblyBytes { get; private set; }

        #region Public

        public BadAssemblyResolveEventArgs( string assemblyName )
        {
            AssemblyName = assemblyName;
        }

        public void Resolve( byte[] assemblyBytes, string pathHint )
        {
            AssemblyBytes = assemblyBytes;
            PathHint = pathHint;
        }

        #endregion

    }

}
