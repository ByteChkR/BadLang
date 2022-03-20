namespace BadVM.Shared.AssemblyFormat.Serialization.Sections
{

    public readonly struct PatchSite
    {

        public PatchSite( AssemblySymbol symbol, int size )
        {
            Symbol = symbol;
            Size = size;
        }

        public int Size { get; }

        public AssemblySymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }

    }

}
