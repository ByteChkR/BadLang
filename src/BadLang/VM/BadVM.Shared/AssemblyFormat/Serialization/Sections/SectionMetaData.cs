namespace BadVM.Shared.AssemblyFormat.Serialization.Sections
{

    public class SectionMetaData
    {

        //Element:
        //  Name
        //  Visibility
        //  Offset
        //  Size

        //Patch Site:
        //  The loaded address will be patched to the offset of the element.
        //  Name
        //  Offset

        public readonly Dictionary < int, PatchSite > PatchSites = new Dictionary < int, PatchSite >();
        public readonly List < AssemblyElement > Elements = new List < AssemblyElement >();

        #region Public

        public static SectionMetaData Deserialize( byte[] data )
        {
            int current = 0;
            int patchCount = BitConverter.ToInt32( data, current );
            current += sizeof( int );

            SectionMetaData meta = new SectionMetaData();

            for ( int i = 0; i < patchCount; i++ )
            {
                int offset = BitConverter.ToInt32( data, current );
                current += sizeof( int );
                int size = BitConverter.ToInt32( data, current );
                current += sizeof( int );

                meta.PatchSites.Add(
                                    offset,
                                    new PatchSite( AssemblySymbol.Deserialize( data, current, out int read ), size )
                                   );

                current += read;
            }

            int elementCount = BitConverter.ToInt32( data, current );
            current += sizeof( int );

            for ( int i = 0; i < elementCount; i++ )
            {
                meta.Elements.Add( AssemblyElement.Deserialize( data, current, out int read ) );
                current += read;
            }

            return meta;
        }

        public byte[] Serialize()
        {
            List < byte > data = new List < byte >();
            data.AddRange( BitConverter.GetBytes( PatchSites.Count ) );

            foreach ( KeyValuePair < int, PatchSite > patchSite in PatchSites )
            {
                data.AddRange( BitConverter.GetBytes( patchSite.Key ) );
                data.AddRange( BitConverter.GetBytes( patchSite.Value.Size ) );
                data.AddRange( patchSite.Value.Symbol.Serialize() );
            }

            data.AddRange( BitConverter.GetBytes( Elements.Count ) );

            foreach ( AssemblyElement element in Elements )
            {
                data.AddRange( element.Serialize() );
            }

            return data.ToArray();
        }

        #endregion

    }

}
