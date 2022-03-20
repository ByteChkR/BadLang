using BadVM.Shared.Logging;

using CommandLine;

namespace bvm
{

    public static class BvmProgram
    {

        #region Public

        public static void Main( string[] args )
        {
            Log.AddLogger( new ConsoleLogger() );
            ParserResult < CommandlineArgs > a = Parser.Default.ParseArguments < CommandlineArgs >( args );

            if ( a.Errors != null && a.Errors.Any() )
            {
                return;
            }

            Commandline cmd = new Commandline();
            cmd.Run( a.Value );
        }

        #endregion

    }

}
