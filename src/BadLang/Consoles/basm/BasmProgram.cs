using BadVM.Shared.Logging;

using basm.ProjectSystem;

using CommandLine;

namespace basm
{

    public static class BasmProgram
    {

        #region Public

        public static void Main( string[] args )
        {
            Log.AddLogger( new ConsoleLogger() );

            if ( args.Length != 0 )
            {
                if ( args[0] == "project" )
                {
                    if ( args.Length == 2 && args[1] == "new" )
                    {
                        CreateDefaultProject();

                        return;
                    }

                    ParserResult < BuildDependency > projectArgs =
                        Parser.Default.ParseArguments < BuildDependency >( args.Skip( 1 ) );

                    if ( projectArgs.Errors != null && projectArgs.Errors.Any() )
                    {
                        return;
                    }

                    ProjectSettings settings = ProjectSettings.Load( projectArgs.Value.File );

                    ProjectBuildContext context =
                        settings.CreateContext(
                                               Path.GetDirectoryName(
                                                                     Path.GetFullPath( projectArgs.Value.File )
                                                                    )!,
                                               projectArgs.Value.Target
                                              );

                    context.Run();

                    return;
                }
            }

            ParserResult < CommandlineArgs > a = Parser.Default.ParseArguments < CommandlineArgs >( args );

            if ( a.Errors != null && a.Errors.Any() )
            {
                return;
            }

            Commandline cmd = new Commandline();
            cmd.Run( a.Value );
        }

        #endregion

        #region Private

        private static void CreateDefaultProject()
        {
            string templateDir = Path.Combine(
                                              Commandline.CompilerDirectory,
                                              "project",
                                              "Template"
                                             );

            string projectDir = Directory.GetCurrentDirectory();

            foreach ( string file in Directory.GetFiles( templateDir, "*", SearchOption.AllDirectories ) )
            {
                string dstFile = Path.Combine(
                                              projectDir,
                                              file.Replace( templateDir + Path.DirectorySeparatorChar, "" )
                                             );

                string dir = Path.GetDirectoryName( dstFile )!;
                Directory.CreateDirectory( dir );

                if ( !File.Exists( dstFile ) )
                {
                    File.Copy( file, dstFile );
                }
            }
        }

        #endregion

    }

}
