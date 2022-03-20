namespace basm.ProjectSystem;

public class BuildTarget
{

    public string Name { get; set; } = "build";

    public string[] SubTargets { get; set; } = Array.Empty < string >();

    public string[] Sources { get; set; } = Array.Empty < string >();

    public string OutputFile { get; set; } = "./bin/$(Target)/$(ProjectName).bvm";

    public BuildDependency[] Dependencies { get; set; } = Array.Empty < BuildDependency >();

    public Dictionary < string, string > Variables { get; set; } = new Dictionary < string, string >();

}
