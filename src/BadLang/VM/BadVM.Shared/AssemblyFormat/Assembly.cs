using BadVM.Shared.AssemblyFormat.Serialization.Sections;

namespace BadVM.Shared.AssemblyFormat
{

    public class Assembly
    {

        private readonly string[] m_Dependencies;

        public string Name { get; }

        public AssemblySection[] Sections { get; }

        #region Public

        public Assembly( string name, AssemblySection[] sections, string[] dependencies )
        {
            Name = name;
            Sections = sections;
            m_Dependencies = dependencies;
        }

        public IEnumerable < string > GetDependencies()
        {
            return m_Dependencies;
        }

        public IEnumerable < AssemblySection > GetSections( string name )
        {
            return Sections.Where( s => IsSectionName( s, name ) );
        }

        #endregion

        #region Private

        private static bool IsSectionName( AssemblySection section, string name )
        {
            string[] nameParts = name.Split( ':' );
            string[] sectionNameParts = section.Name.Split( ':' );

            if ( nameParts.Length == 1 ) //No Sub Section
            {
                return sectionNameParts[0] == nameParts[0];
            }

            if ( sectionNameParts.Length == 1 )
            {
                return false;
            }

            return sectionNameParts[0] == nameParts[0] && sectionNameParts[1] == nameParts[1];
        }

        #endregion

    }

}
