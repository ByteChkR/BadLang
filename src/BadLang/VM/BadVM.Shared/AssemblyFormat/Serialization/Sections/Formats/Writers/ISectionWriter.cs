namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers
{

    public interface ISectionWriter
    {

        string AssemblyName { get; }

        int CurrentSize { get; }

        byte[] GetBytes();

        SectionMetaData GetMetaData();

        bool IsEmpty { get; }

        string SectionFormat { get; }

        string SectionName { get; }

    }

}
