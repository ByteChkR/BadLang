using System.Text;

using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Data
{

    public class RawDataSectionFormat : AssemblySectionFormat
    {

        private const string FORMAT_NAME = "data_raw";

        #region Public

        public RawDataSectionFormat() : base( FORMAT_NAME )
        {
        }

        public static byte[] CreateDataSection( string name, byte[] metaData, byte[] data, bool readOnly )
        {
            List < byte > bytes = new List < byte >();
            byte[] formatbuf = Encoding.UTF8.GetBytes( FORMAT_NAME );
            bytes.AddRange( BitConverter.GetBytes( formatbuf.Length ) );
            bytes.AddRange( formatbuf );
            byte[] namebuf = Encoding.UTF8.GetBytes( name );
            bytes.AddRange( BitConverter.GetBytes( readOnly ) );
            bytes.AddRange( BitConverter.GetBytes( namebuf.Length ) );
            bytes.AddRange( namebuf );
            bytes.AddRange( BitConverter.GetBytes( data.Length ) );
            bytes.AddRange( data );
            bytes.AddRange( BitConverter.GetBytes( metaData.Length ) );
            bytes.AddRange( metaData );

            return bytes.ToArray();
        }

        public override AssemblySection Read( byte[] data, int start, out int readBytes, Func < Assembly > asmLookup )
        {
            int current = start;
            bool isReadOnly = BitConverter.ToBoolean( data, current );
            current += sizeof( bool );
            int nameLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            string name = Encoding.UTF8.GetString( data, current, nameLen );
            current += nameLen;
            int dataLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            byte[] dataBytes = new byte[dataLen];
            Array.Copy( data, current, dataBytes, 0, dataLen );
            current += dataLen;

            int metaDataLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            byte[] metaDataBytes = new byte[metaDataLen];
            Array.Copy( data, current, metaDataBytes, 0, metaDataLen );
            current += metaDataLen;

            SectionMetaData metaData = SectionMetaData.Deserialize( metaDataBytes );

            readBytes = current - start;

            return new RawDataSection( asmLookup, name, metaData, dataBytes );
        }

        public override byte[] Write( ISectionWriter writer )
        {
            bool readOnly = ( writer as DataSectionWriter )?.IsReadOnly ?? false;

            return CreateDataSection(
                                     writer.SectionName,
                                     writer.GetMetaData().Serialize(),
                                     writer.GetBytes(),
                                     readOnly
                                    );
        }

        #endregion

    }

}
