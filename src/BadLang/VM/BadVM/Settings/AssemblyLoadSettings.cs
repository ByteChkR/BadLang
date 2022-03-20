namespace BadVM.Settings;

public class AssemblyLoadSettings
{

    public bool ResolveFromLocalDirectory { get; set; }

    public bool ResolveFromLoadDirectory { get; set; }

    public string[] SearchDirectories { get; set; } =
        new[] { Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "data", "runtime", "libs" ) };

}
