using BadVM.Shared.Logging;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers
{

    public class CodeSectionWriter : ISectionWriter
    {

        public readonly AssemblyWriter AssemblyWriter;

        private static readonly LogMask m_LogMask = new LogMask( "CodeSectionWriter" );

        private int m_UniqueCounter = 0;
        private readonly List < byte > m_Bytes = new List < byte >();
        private readonly SectionMetaData m_MetaData = new SectionMetaData();

        public bool IsEmpty => m_Bytes.Count == 0;

        public int CurrentSize => m_Bytes.Count;

        public string AssemblyName { get; }

        public string SectionName { get; }

        public string SectionFormat { get; }

        #region Public

        public CodeSectionWriter(
            string assemblyName,
            string sectionName,
            string sectionFormat,
            AssemblyWriter assemblyWriter )
        {
            SectionName = sectionName;
            SectionFormat = sectionFormat;
            AssemblyWriter = assemblyWriter;
            AssemblyName = assemblyName;
        }

        public void AddPatchSite( AssemblySymbol sym, int off, int size )
        {
            m_MetaData.PatchSites.Add( off, new PatchSite( sym, size ) );
        }

        public void Emit( OpCode op, byte[] args )
        {
            m_Bytes.AddRange( BitConverter.GetBytes( ( ushort )op ) );
            m_Bytes.AddRange( args );
        }

        public byte[] GetBytes()
        {
            return m_Bytes.ToArray();
        }

        public SectionMetaData GetMetaData()
        {
            return m_MetaData;
        }

        public AssemblySymbol GetUniqueName( string prefix )
        {
            return new AssemblySymbol( AssemblyName, SectionName, $"{prefix}_{m_UniqueCounter++}" );
        }

        public void Label( string name, AssemblyElementVisibility visibility )
        {
            Label( new AssemblySymbol( AssemblyName, SectionName, name ), visibility );
        }

        public void Label( AssemblySymbol name, AssemblyElementVisibility visibility )
        {
            m_MetaData.Elements.Add(
                                    new AssemblyElement(
                                                        name,
                                                        m_Bytes.Count,
                                                        sizeof( long ),
                                                        visibility
                                                       )
                                   );
        }

        #endregion

    }

}
