namespace BadVM.Shared.AssemblyFormat.Serialization.Sections
{

    public readonly struct PatchSite : IEquatable<PatchSite>
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

        public bool Equals(PatchSite other)
        {
            return Size == other.Size && Symbol.Equals(other.Symbol);
        }

        public override bool Equals(object? obj)
        {
            return obj is PatchSite other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Size * 397) ^ Symbol.GetHashCode();
            }
        }
    }

}
