using CommandLine;

namespace bvm
{

    internal class CommandlineArgs
    {

        [Option( 'e', "entry", HelpText = "Entry point to run", Required = false, Default = null )]
        public string? SymbolName { get; set; }

        [Option( "exportInterop", HelpText = "Exports BASM Wrappers for all Interop Functions", Required = false )]
        public bool ExportInterop { get; set; }

        [Option( "debug", HelpText = "Loads Debug symbols if they are present.", Required = false )]
        public bool Debug { get; set; }

        [Option(
                   "debugger",
                   HelpText = "The debugger that is going to be attached if --debug is specified",
                   Required = false,
                   Default = null
               )]
        public string? DebuggerName { get; set; } = null;

        [Option( 'p', "preset", HelpText = "Settings Preset", Required = false, Default = "default" )]
        public string Preset { get; set; } = "default";

        [Option( "synchonous", HelpText = "Runs the program synchronously", Required = false )]
        public bool Synchronous { get; set; }

        [Value(
                  0,
                  MetaName = "Assembly",
                  HelpText =
                      "Assembly to run | The Output Folder for the Wrapper Export if flag --exportInterop is set",
                  Required = true
              )]
        public string File { get; set; } = null!;

    }

}
