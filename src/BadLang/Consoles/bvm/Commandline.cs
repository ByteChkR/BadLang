using BadVM;
using BadVM.AssemblyFormat;
using BadVM.Debugger;
using BadVM.Interop;
using BadVM.Settings;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;
using BadVM.Shared.PluginSystem;

using bvm.Exceptions;

using Newtonsoft.Json;

using EntryPointNotFoundException = bvm.Exceptions.EntryPointNotFoundException;

namespace bvm
{

    internal class Commandline
    {

        public static readonly LogMask LogMask = VirtualCPU.LogMask.CreateChild( "Console" );

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

        public static string RuntimeDirectory
        {
            get
            {
                string dir = Path.Combine( DataDirectory, "runtime" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public string PresetName { get; set; } = "default";

        public string PresetDirectory
        {
            get
            {
                string dir = Path.Combine( RuntimeDirectory, PresetName );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public string PluginDirectory
        {
            get
            {
                string dir = Path.Combine( RuntimeDirectory, "plugins" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public string SettingsDirectory
        {
            get
            {
                string dir = Path.Combine( RuntimeDirectory, "settings" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public string PresetSettingsDirectory
        {
            get
            {
                string dir = Path.Combine( PresetDirectory, "settings" );

                if ( !Directory.Exists( dir ) )
                {
                    Directory.CreateDirectory( dir );
                }

                return dir;
            }
        }

        public BadLangSettings Settings
        {
            get
            {
                string file = Path.Combine( PresetSettingsDirectory, "settings.json" );

                if ( File.Exists( file ) )
                {
                    return JsonConvert.DeserializeObject < BadLangSettings >( File.ReadAllText( file ) )!;
                }

                file = Path.Combine( SettingsDirectory, "settings.json" );
                BadLangSettings settings;

                if ( !File.Exists( file ) )
                {
                    settings = new BadLangSettings();
                    File.WriteAllText( file, JsonConvert.SerializeObject( settings, Formatting.Indented ) );
                }
                else
                {
                    try
                    {
                        settings = JsonConvert.DeserializeObject < BadLangSettings >( File.ReadAllText( file ) )!;
                    }
                    catch ( Exception )
                    {
                        LogMask.Warning(
                                        $"Can not load settings file {file}. Using default settings. Delete the file to regenerate default settings on disk."
                                       );

                        settings = new BadLangSettings();
                    }
                }

                return settings;
            }
        }

        #region Public

        public void Run( CommandlineArgs args )
        {
            PresetName = args.Preset;
            PluginLoader.InitializePlugins( PluginDirectory, SettingsDirectory, PresetSettingsDirectory );

            DebuggerHost host = new DebuggerHost();
            PluginLoader.LoadPlugins( host );

            if ( args.ExportInterop )
            {
                InteropHelper.GenerateWrapperAssemblies().ExportToFolder( args.File );

                return;
            }

            MemoryBus bus = CreateBus( Settings );

            VirtualCPU cpu = new VirtualCPU( Settings, bus );

            BadAssemblyDomain domain = CreateAssemblyDomain( Settings, args );

            if ( args.Debug )
            {
                IDebugger? debugger = host.GetDebugger( args.DebuggerName );

                if ( debugger == null )
                {
                    throw new Exception( $"Debugger with name '{args.DebuggerName ?? "ANY"}' not found" );
                }

                debugger.Attach( cpu, domain );
            }

            if ( !File.Exists( args.File ) )
            {
                LogMask.Warning( $"File {args.File} does not exist." );

                return;
            }

            //Load Supplied Assemblies
            LogMask.LogMessage( $"Loading Assembly {args.File}" );
            Assembly asm = domain.LoadAssembly( File.ReadAllBytes( args.File ), args.File );

            //Map the Entry Assembly(this will load all dependencies)
            domain.MapAssembly( asm.Name, bus );

            //Get the Entry Address
            string sym = args.SymbolName ?? $"{asm.Name}::code::__main__";
            long entry = domain.GetSymbolAddress( bus, sym );

            if ( entry == -1 )
            {
                throw new EntryPointNotFoundException( $"Entry point '{sym}' not found" );
            }

            LogMask.LogMessage( $"Initializing Core 0 with Entry Point: {sym}" );

            //Initialize Core 0 and run
            cpu.InitCore( 0, entry );

            if ( args.Synchronous )
            {
                cpu.StartSynchronous();
            }
            else
            {
                cpu.Start();
            }

            Thread.Sleep( 100 );

            while ( cpu.IsRunning )
            {
                Thread.Sleep( 100 );
            }

            cpu.Exit();
            LogMask.LogMessage( $"Execution Terminated" );
        }

        #endregion

        #region Private

        private static BadAssemblyDomain CreateAssemblyDomain( BadLangSettings settings, CommandlineArgs args )
        {
            BadAssemblyDomain domain = new BadAssemblyDomain();

            if ( settings.AssemblySettings.ResolveFromLocalDirectory )
            {
                AssemblyResolver resolver = new AssemblyResolver( Directory.GetCurrentDirectory() );
                domain.AddResolver( resolver.Resolve );
            }

            if ( settings.AssemblySettings.ResolveFromLoadDirectory )
            {
                string file = Path.GetFullPath( args.File );

                string? dir = Path.GetDirectoryName( file );

                if ( dir == null )
                {
                    throw new LoadDirectoryNotFoundException( "Could not resolve load directory" );
                }

                Directory.CreateDirectory( dir );
                AssemblyResolver resolver = new AssemblyResolver( dir );
                domain.AddResolver( resolver.Resolve );
            }

            foreach ( string directory in settings.AssemblySettings.SearchDirectories )
            {
                Directory.CreateDirectory( directory );
                AssemblyResolver resolver = new AssemblyResolver( directory );
                domain.AddResolver( resolver.Resolve );
            }

            return domain;
        }

        private static MemoryBus CreateBus( BadLangSettings settings )
        {
            MemoryBus bus = new MemoryBus();

            foreach ( MemoryMap map in settings.MemoryLayout )
            {
                bus.Register( map.MakeEntry() );
            }

            return bus;
        }

        #endregion

    }

}
