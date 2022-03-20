using System.Text;

using BadVM.Shared.AssemblyFormat.Exceptions;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats;

namespace BadVM.Shared.AssemblyFormat.Serialization
{

    public class AssemblyReader
    {

        private readonly IEnumerable < AssemblySectionFormat > m_Formats;
        private readonly byte[] m_Data;

        private const long MAGIC = 0x6969_6969_6969_6969;

        #region Public

        public AssemblyReader( IEnumerable < AssemblySectionFormat > formats, byte[] data )
        {
            m_Formats = formats;
            m_Data = data;
        }

        public static bool IsAssemblyFile( byte[] data )
        {
            return BitConverter.ToInt64( data, 0 ) == MAGIC;
        }

        public static bool TryReadAssemblyName( byte[] data, out string name )
        {
            int current = 0;

            long magic = BitConverter.ToInt64( data, current );
            current += sizeof( long );

            if ( magic != MAGIC )
            {
                name = "Error";

                return false;
            }

            int nameLen = BitConverter.ToInt32( data, current );
            current += sizeof( int );
            name = Encoding.UTF8.GetString( data, current, nameLen );

            return true;
        }

        public Assembly ReadAssembly( int start )
        {
            int current = start;

            long magic = BitConverter.ToInt64( m_Data, current );
            current += sizeof( long );

            if ( magic != MAGIC )
            {
                throw new AssemblyReaderException( "Invalid File Type" );
            }

            int nameLen = BitConverter.ToInt32( m_Data, current );
            current += sizeof( int );
            string name = Encoding.UTF8.GetString( m_Data, current, nameLen );
            current += nameLen;

            Assembly? asm = null;

            AssemblySection[] sections = ReadSections(
                                                      current,
                                                      out int sectionSize,
                                                      () =>
                                                      {
                                                          if ( asm != null )
                                                          {
                                                              return asm;
                                                          }

                                                          throw new AssemblyReaderException(
                                                               "Internal Assembly Load Error."
                                                              );
                                                      }
                                                     );

            current += sectionSize;
            int depLen = BitConverter.ToInt32( m_Data, current );
            current += sizeof( int );
            string[] dependencies = new string[depLen];

            for ( int i = 0; i < depLen; i++ )
            {
                int depNameLen = BitConverter.ToInt32( m_Data, current );
                current += sizeof( int );
                dependencies[i] = Encoding.UTF8.GetString( m_Data, current, depNameLen );
                current += depNameLen;
            }

            asm = new Assembly( name, sections, dependencies );

            foreach ( AssemblySection section in sections )
            {
                section.FinalizeSerialization();
            }

            return asm;
        }

        public AssemblySection ReadSection( int start, out int read, Func < Assembly > asmLookup )
        {
            int current = start;
            int formatNameLen = BitConverter.ToInt32( m_Data, current );
            current += sizeof( int );
            string formatName = Encoding.UTF8.GetString( m_Data, current, formatNameLen );
            current += formatNameLen;
            AssemblySectionFormat format = m_Formats.FirstOrDefault( f => f.Name == formatName );

            if ( format == null )
            {
                throw new SectionFormatNotFoundException( $"Unknown assembly section format: {formatName}" );
            }

            AssemblySection s = format.Read( m_Data, current, out int sread, asmLookup );
            current += sread;
            read = current - start;

            return s;
        }

        public AssemblySection[] ReadSections( int start, out int read, Func < Assembly > asmLookup )
        {
            int current = start;
            int sectionCount = BitConverter.ToInt32( m_Data, current );
            current += sizeof( int );
            AssemblySection[] sections = new AssemblySection[sectionCount];

            for ( int i = 0; i < sectionCount; i++ )
            {
                AssemblySection section = ReadSection( current, out int sectionRead, asmLookup );
                current += sectionRead;
                sections[i] = section;
            }

            read = current - start;

            return sections;
        }

        #endregion

    }

}
