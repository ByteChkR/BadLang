namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers
{

    public class DataSectionWriter : ISectionWriter
    {

        private readonly List < byte > m_Bytes = new List < byte >();
        private readonly SectionMetaData m_MetaData = new SectionMetaData();

        public string SectionFormat { get; }

        public string AssemblyName { get; }

        public string SectionName { get; }

        public int CurrentSize => m_Bytes.Count;

        public bool IsReadOnly { get; }

        public bool IsEmpty => m_Bytes.Count == 0;

        #region Public

        public DataSectionWriter( string sectionFormat, string assemlyName, string sectionName, bool isReadOnly )
        {
            SectionFormat = sectionFormat;
            AssemblyName = assemlyName;
            SectionName = sectionName;
            IsReadOnly = isReadOnly;
        }

        public void AddData( string name, AssemblyElementVisibility visibility, byte[] data )
        {
            int offset = m_Bytes.Count;
            m_Bytes.AddRange( data );

            m_MetaData.Elements.Add(
                                    new AssemblyElement(
                                                        AssemblySymbol.Parse( name, AssemblyName, SectionName ),
                                                        offset,
                                                        data.Length,
                                                        visibility
                                                       )
                                   );
        }

        public byte[] GetBytes()
        {
            return m_Bytes.ToArray();
        }

        public SectionMetaData GetMetaData()
        {
            return m_MetaData;
        }

        #endregion

    }

}
