using CommandLine;

namespace basm
{

    internal class CommandlineArgs
    {

        [Option( 'n', "name", Required = false, HelpText = "Name of the assembly." )]
        public string AssemblyName { get; set; } = null!;

        [Option( 'o', "output", Required = true, HelpText = "Output File." )]
        public string OutputFile { get; set; } = null!;

        [Option( 'i', "input", Required = true, HelpText = "Input Source Files." )]
        public IEnumerable < string > SourceFiles { get; set; } = Enumerable.Empty < string >();

        [Option(
                   "compileInterop",
                   Required = false,
                   HelpText =
                       "If Specified the input is interpreted as Wrapper Export Output. All Assemblies inside this folder will be compiled."
               )]
        public bool WrapperDirectory { get; set; } = false;

    }

}
