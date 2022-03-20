using BadVM.Shared.Logging;

using basm;

using bvm;

namespace TestConsole
{

    internal class Program
    {

        #region Private

        private static void Main( string[] args )
        {
            Log.AddLogger( new ConsoleLogger() );

            while ( true )
            {
                Console.Write( $"TestConsole {Directory.GetCurrentDirectory()} > " );
                string input = Console.ReadLine()!;
                string[] parts = input.Split( ' ' );

                if ( parts[0].ToLower() == "exit" )
                {
                    break;
                }

                if ( parts[0].ToLower() == "basm" )
                {
                    BasmProgram.Main( parts.Skip( 1 ).ToArray() );
                }
                else if ( parts[0].ToLower() == "bvm" )
                {
                    BvmProgram.Main( parts.Skip( 1 ).ToArray() );
                }
                else if ( parts[0].ToLower() == "cd" )
                {
                    Directory.SetCurrentDirectory( parts[1] );
                }
                else
                {
                    Console.WriteLine( $"Unknown command: '{input}'" );
                }
            }
        }

        #endregion

    }

}
