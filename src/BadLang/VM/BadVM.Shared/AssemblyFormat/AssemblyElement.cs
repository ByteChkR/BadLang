namespace BadVM.Shared.AssemblyFormat
{

    public class AssemblyElement
    {

        public readonly AssemblySymbol Name;
        public readonly int Offset;
        public readonly int Size;
        public readonly AssemblyElementVisibility Visibility;

        #region Public

        public AssemblyElement( AssemblySymbol name, int offset, int size, AssemblyElementVisibility visibility )
        {
            Name = name;
            Offset = offset;
            Size = size;
            Visibility = visibility;
        }

        public static AssemblyElement Deserialize( byte[] bytes, int start, out int read )
        {
            int current = start;
            AssemblySymbol name = AssemblySymbol.Deserialize( bytes, current, out int nameRead );
            current += nameRead;
            int offset = BitConverter.ToInt32( bytes, current );
            current += sizeof( int );
            int size = BitConverter.ToInt32( bytes, current );
            current += sizeof( int );
            AssemblyElementVisibility visibility = ( AssemblyElementVisibility )bytes[current];
            current += sizeof( byte );
            read = current - start;

            return new AssemblyElement( name, offset, size, visibility );
        }

        public bool IsVisibleFrom( AssemblySymbol symbol )
        {
            return Visibility == AssemblyElementVisibility.Export ||
                   Visibility == AssemblyElementVisibility.Assembly && symbol.AssemblyName == Name.AssemblyName ||
                   Visibility == AssemblyElementVisibility.Local &&
                   symbol.AssemblyName == Name.AssemblyName &&
                   symbol.SectionName == Name.SectionName;
        }

        public byte[] Serialize()
        {
            List < byte > bytes = new List < byte >();
            bytes.AddRange( Name.Serialize() );
            bytes.AddRange( BitConverter.GetBytes( Offset ) );
            bytes.AddRange( BitConverter.GetBytes( Size ) );
            bytes.Add( ( byte )Visibility );

            return bytes.ToArray();
        }

        public override  string ToString()
        {
            return $"{Name} {Offset} {Size} {Visibility}";
        }
        
        #endregion

    }

}
