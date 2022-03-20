using System.Text;

using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Code
{

    public class RawCodeSectionFormat : AssemblySectionFormat
    {

        private const string FORMAT_NAME = "code_raw";

        #region Public

        public RawCodeSectionFormat() : base( FORMAT_NAME )
        {
        }

        public static byte[] CreateCodeSection( string name, byte[] metaData, byte[] data )
        {
            List < byte > bytes = new List < byte >();
            byte[] formatbuf = Encoding.UTF8.GetBytes( FORMAT_NAME );
            bytes.AddRange( BitConverter.GetBytes( formatbuf.Length ) );
            bytes.AddRange( formatbuf );
            byte[] namebuf = Encoding.UTF8.GetBytes( name );
            bytes.AddRange( BitConverter.GetBytes( namebuf.Length ) );
            bytes.AddRange( namebuf );
            bytes.AddRange( BitConverter.GetBytes( data.Length ) );
            bytes.AddRange( data );
            bytes.AddRange( BitConverter.GetBytes( metaData.Length ) );
            bytes.AddRange( metaData );

            return bytes.ToArray();
        }

        public override AssemblySection Read( byte[] data, int start, out int read, Func < Assembly > asmResolve )
        {
            int current = start;
            int nameLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            string name = Encoding.UTF8.GetString( data, current, nameLen );
            current += nameLen;
            int dataLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            byte[] code = new byte[dataLen];
            Array.Copy( data, current, code, 0, dataLen );
            current += dataLen;

            int metaDataLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            byte[] metaData = new byte[metaDataLen];
            Array.Copy( data, current, metaData, 0, metaDataLen );
            current += metaDataLen;
            SectionMetaData meta = SectionMetaData.Deserialize( metaData );
            read = current - start;

            return new RawCodeSection( asmResolve, name, meta, code );
        }

        public override byte[] Write( ISectionWriter writer )
        {
            return CreateCodeSection(
                                     writer.SectionName,
                                     writer.GetMetaData().Serialize(),
                                     writer.GetBytes()
                                    );
        }

        #endregion

    }

}
