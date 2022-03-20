using CommandLine;

namespace basm.ProjectSystem;

public class BuildDependency
{

    [Value( 0, HelpText = "The Project Settings to build from", Required = false )]
    public string File { get; set; } = "./project.json";

    [Value( 1, HelpText = "The target to build", Required = false, Default = null )]
    public string Target { get; set; } = null!;

}
