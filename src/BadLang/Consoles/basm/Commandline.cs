using BadAssembler;
using BadAssembler.AssemblerSyntax;
using BadAssembler.Preprocessor;
using BadAssembler.Segments;

using BadC;

using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats;
using BadVM.Shared.Logging;

using Newtonsoft.Json;

namespace basm
{

    internal class Commandline
    {

        public static readonly LogMask LogMask = Assembler.LogMask.CreateChild( "Console" );

        public static string DataDirectory
        {
            get
            {
                string dir = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "data" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public static string CompilerDirectory
        {
            get
            {
                string dir = Path.Combine( DataDirectory, "compiler" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                string dir = Path.Combine( CompilerDirectory, "settings" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public static CompilerSettings Settings
        {
            get
            {
                string file = Path.Combine( SettingsDirectory, "settings.json" );
                CompilerSettings settings;

                if ( !File.Exists( file ) )
                {
                    settings = new CompilerSettings();
                    File.WriteAllText( file, JsonConvert.SerializeObject( settings, Formatting.Indented ) );
                }
                else
                {
                    try
                    {
                        settings = JsonConvert.DeserializeObject < CompilerSettings >( File.ReadAllText( file ) )!;
                    }
                    catch ( Exception )
                    {
                        LogMask.Warning(
                                        $"Can not load settings file {file}. Using default settings. Delete the file to regenerate default settings on disk."
                                       );

                        settings = new CompilerSettings();
                    }
                }

                return settings;
            }
        }

        #region Public

        public static void LoadFiles(PreprocessorContext ctx, IEnumerable<string> input, string workingDir = "./")
        {
            foreach ( string s in input )
            {
                string path;

                if ( Path.IsPathRooted( s ) )
                {
                    path = s;
                }
                else
                {
                    path = Path.Combine( workingDir, s );
                }

                if ( File.Exists( path ) )
                {
                    LogMask.LogMessage( $"Loading file {path}" );

                    ctx.ReadFile(path);
                }
                else if ( Directory.Exists( path ) )
                {
                    foreach ( string ss in Directory.GetFiles( path, "*.basm", SearchOption.AllDirectories ) )
                    {
                        LogMask.LogMessage( $"Loading file {ss}" );

                        ctx.ReadFile(ss);
                    }
                }
            }

            foreach ( string includeDir in Settings.IncludeDirectores )
            {
                if ( Directory.Exists( includeDir ) )
                {
                    foreach ( string includeFile in Directory.GetFiles(
                                 includeDir,
                                 "*.basm",
                                 SearchOption.AllDirectories
                             ) )
                    {
                        LogMask.LogMessage( $"Loading file {includeFile}" );

                        ctx.ReadFile(includeFile);
                    }
                }
            }
        }

        public void Run( CommandlineArgs args )
        {
            List < SegmentParser > parsers = new List < SegmentParser >
                                             {
                                                 new CodeSegmentParser(
                                                                       new List < CodeSyntaxParser >
                                                                       {
                                                                           new BadAssemblyCodeParser(),
                                                                           new BadCCodeParser()
                                                                       }
                                                                      ),
                                                 new DataSegmentParser(
                                                                       new List < DataSyntaxParser >
                                                                       {
                                                                           new BadAssemblyDataParser()
                                                                       }
                                                                      ),
                                                 new DependencySegmentParser()
                                             };

            Assembler assembler = new Assembler( parsers );

            if ( args.WrapperDirectory )
            {
                foreach ( string wrapperDir in args.SourceFiles )
                {
                    if ( !Directory.Exists( wrapperDir ) )
                    {
                        LogMask.Warning( $"Wrapper directory does not exist: {wrapperDir}" );

                        continue;
                    }

                    Directory.CreateDirectory( args.OutputFile );

                    string wrapperAssemblyPath = Path.Combine( wrapperDir, "Assemblies" );

                    foreach ( string asmPath in Directory.GetDirectories( wrapperAssemblyPath ) )
                    {
                        string asmName = Path.GetFileNameWithoutExtension( asmPath );
                        LogMask.LogMessage( $"Compiling Wrapper Assembly: {asmName}" );
                        string asmSourcePath = Path.Combine( asmPath, "code.basm" );

                        if ( !File.Exists( asmSourcePath ) )
                        {
                            LogMask.Warning( $"Wrapper Assembly Source does not exist: {asmSourcePath}" );

                            continue;
                        }

                        string asmOutputPath = Path.Combine( args.OutputFile, asmName + ".bvm" );
                        AssemblyWriter wwriter = new AssemblyWriter( asmName, AssemblySectionFormat.Formats );

                        PreprocessorContext wctx = new PreprocessorContext( );

                        LoadFiles(wctx, new[] {asmSourcePath});
                        
                        ( byte[] wasmBytes, AssemblyCompilationDataContainer wdata ) =
                            assembler.Assemble( wctx.GetSources(), wwriter );

                        File.WriteAllBytes(
                                           asmOutputPath,
                                           wasmBytes
                                          );

                        wdata.Export( asmOutputPath );
                    }
                }

                return;
            }

            string assemblyName = args.AssemblyName ?? Path.GetFileNameWithoutExtension( args.OutputFile );

            AssemblyWriter writer = new AssemblyWriter( assemblyName, AssemblySectionFormat.Formats );
            string outDir = Path.GetDirectoryName( Path.GetFullPath( args.OutputFile ) )!;

            if ( !Directory.Exists( outDir ) )
            {
                Directory.CreateDirectory( outDir );
            }
            
            PreprocessorContext ctx = new PreprocessorContext( );
            LoadFiles(ctx, args.SourceFiles);

            ( byte[] asmBytes, AssemblyCompilationDataContainer data ) =
                assembler.Assemble( ctx.GetSources(), writer );

            File.WriteAllBytes( args.OutputFile, asmBytes );
            data.Export( args.OutputFile );
        }

        #endregion

    }

}
