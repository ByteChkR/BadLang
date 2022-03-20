using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;

namespace BadVM.AssemblyFormat
{

    public static class BadAssemblyLinker
    {

        public static readonly LogMask LogMask = new LogMask( "Linker" );

        #region Public

        public static void Link(
            BadAssemblyDomain domain,
            MemoryBus bus,
            Assembly assembly,
            AssemblySection section,
            byte[] data )
        {
            LogMask.LogMessage(
                               $"Linking {assembly.Name}::{section.Name}"
                              );

            foreach ( KeyValuePair < int, PatchSite > patchSite in section.MetaData.PatchSites )
            {
                int offset = patchSite.Key;
                AssemblySymbol symbol = patchSite.Value.Symbol;

                long addr = domain.GetSymbolAddress( bus, symbol );

                if ( addr == -1 )
                {
                    throw new AssemblyLinkerException( $"Unresolved symbol: {symbol}" );
                }

                byte[] bytes = BitConverter.GetBytes( addr );

                if ( patchSite.Value.Size != bytes.Length )
                {
                    throw new AssemblyLinkerException(
                                                      $"Patch site size mismatch for symbol {symbol}: {patchSite.Value.Size} != {bytes.Length}"
                                                     );
                }

                Array.Copy( bytes, 0, data, offset, bytes.Length );
            }
        }

        #endregion

    }

}
