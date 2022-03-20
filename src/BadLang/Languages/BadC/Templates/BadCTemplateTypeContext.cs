using BadAssembler;

using BadC.Types;

using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Templates;

public class BadCTemplateTypeContext
{

    public readonly PostSegmentParseTasks TaskList;
    public readonly CodeSectionWriter Writer;

    private readonly Dictionary < BadCTemplateType, BadCType > m_Types;

    #region Public

    public BadCTemplateTypeContext(
        Dictionary < BadCTemplateType, BadCType > types,
        PostSegmentParseTasks taskList,
        CodeSectionWriter writer )
    {
        m_Types = types;
        TaskList = taskList;
        Writer = writer;
    }

    public uint GetTemplateHash()
    {
        uint code = 0;

        foreach ( BadCType arg in m_Types.Values )
        {
            code ^= ( uint )arg.TypeName.ToString().GetHashCode();
        }

        return code;
    }

    public BadCType ResolveType( BadCType templateType )
    {
        if ( templateType is BadCTemplateType template && m_Types.ContainsKey( template ) )
        {
            return m_Types[template];
        }

        return templateType;
    }

    #endregion

}
