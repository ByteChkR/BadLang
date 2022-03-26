using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Code;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Data;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats
{

    public abstract class AssemblySectionFormat
    {

        private static readonly List < AssemblySectionFormat > s_Formats = new List < AssemblySectionFormat >
                                                                           {
                                                                               new RawCodeSectionFormat(),
                                                                               new RawDataSectionFormat(),
                                                                           };

        public static IEnumerable < AssemblySectionFormat > Formats => s_Formats;

        public string Name { get; }

        #region Public

        public static void RegisterFormat( AssemblySectionFormat format )
        {
            s_Formats.Add( format );
        }

        public abstract AssemblySection Read( byte[] data, int start, out int readBytes, Func < Assembly > asmLookup );

        public abstract byte[] Write( ISectionWriter writer );

        #endregion

        #region Protected

        protected AssemblySectionFormat( string name )
        {
            Name = name;
        }

        #endregion

    }

}
