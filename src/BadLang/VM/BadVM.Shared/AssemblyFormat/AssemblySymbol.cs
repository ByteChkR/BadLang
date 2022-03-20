using System.Text;

namespace BadVM.Shared.AssemblyFormat
{

    public struct AssemblySymbol
    {

        public override bool Equals( object obj )
        {
            if ( obj is AssemblySymbol asm )
            {
                return Equals( asm );
            }

            return false;
        }

        public bool Equals( AssemblySymbol other )
        {
            bool sameSection = false;

            if ( SectionName.Contains( ':' ) )
            {
                if ( other.SectionName.Contains( ':' ) ) //Both have subsection = exact match
                {
                    sameSection = SectionName == other.SectionName;
                }
                else
                {
                    sameSection = SectionName.Split( ':' )[0] == other.SectionName; //Only compare main section
                }
            }
            else if ( other.SectionName.Contains( ':' ) )
            {
                sameSection = other.SectionName.Split( ':' )[0] == SectionName; //Only compare main section
            }
            else
            {
                sameSection = SectionName == other.SectionName;
            }

            return AssemblyName == other.AssemblyName &&
                   sameSection &&
                   SymbolName == other.SymbolName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AssemblyName.GetHashCode();
                hashCode = ( hashCode * 397 ) ^ SectionName.GetHashCode();
                hashCode = ( hashCode * 397 ) ^ SymbolName.GetHashCode();

                return hashCode;
            }
        }

        public static bool operator ==( AssemblySymbol left, AssemblySymbol right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( AssemblySymbol left, AssemblySymbol right )
        {
            return !left.Equals( right );
        }

        public AssemblySymbol( string assemblyName, string sectionName, string symbolName )
        {
            AssemblyName = assemblyName;
            SectionName = sectionName;
            SymbolName = symbolName;
        }

        public override string ToString()
        {
            return $"{AssemblyName}::{SectionName}::{SymbolName}";
        }

        public string AssemblyName { get; }

        public string SectionName { get; }

        public string SymbolName { get; }

        private static string[] Split( string name )
        {
            return name.Split( new[] { "::" }, 3, StringSplitOptions.None );
        }

        public static AssemblySymbol Parse( string name )
        {
            string[] parts = Split( name );

            return new AssemblySymbol( parts[0], parts[1], parts[2] );
        }

        public static AssemblySymbol Parse( string name, string localAssembly, string localSection )
        {
            string[] parts = Split( name );

            if ( parts.Length == 1 )
            {
                return new AssemblySymbol( localAssembly, localSection, parts[0] );
            }
            else if ( parts.Length == 2 )
            {
                return new AssemblySymbol( localAssembly, parts[0], parts[1] );
            }
            else
            {
                return new AssemblySymbol( parts[0], parts[1], parts[2] );
            }
        }

        public byte[] Serialize()
        {
            List < byte > bytes = Encoding.UTF8.GetBytes( ToString() ).ToList();
            bytes.InsertRange( 0, BitConverter.GetBytes( bytes.Count ) );

            return bytes.ToArray();
        }

        public static AssemblySymbol Deserialize( byte[] bytes, int start, out int read )
        {
            int current = start;
            int length = BitConverter.ToInt32( bytes, current );
            current += sizeof( int );
            string name = Encoding.UTF8.GetString( bytes, current, length );
            current += length;
            read = current - start;

            return Parse( name );
        }

    }

}
