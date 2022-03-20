using BadVM.Shared.Memory;

namespace BadVM.Shared.AssemblyFormat.Serialization.Sections
{

    public abstract class AssemblySection
    {

        private readonly Func < Assembly > m_AssemblyLookup;

        public Assembly? Assembly { get; private set; }

        public SectionMetaData MetaData { get; }

        public SectionType Type { get; }

        public string Name { get; }

        #region Public

        public abstract int GetLoadedSize();

        public abstract void Load(
            long address,
            IAssemblyDomain domain,
            MemoryBus bus,
            Action < Action > addFinishAction );

        public virtual void FinalizeSerialization()
        {
            if ( Assembly == null )
            {
                Assembly = m_AssemblyLookup();
            }
        }

        public AssemblyElement GetSymbol( string symbol )
        {
            return MetaData.Elements.FirstOrDefault( e => e.Name.SymbolName == symbol );
        }

        public bool HasSymbol( string symbol )
        {
            return MetaData.Elements.Any( x => x.Name.SymbolName == symbol );
        }

        #endregion

        #region Protected

        protected AssemblySection(
            Func < Assembly > asmLookup,
            string name,
            SectionType type,
            SectionMetaData metaData )
        {
            Name = name;
            Type = type;
            MetaData = metaData;
            m_AssemblyLookup = asmLookup;
        }

        #endregion

    }

}
