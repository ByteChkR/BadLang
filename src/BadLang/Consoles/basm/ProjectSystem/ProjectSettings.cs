using Newtonsoft.Json;

namespace basm.ProjectSystem;

public class ProjectSettings
{

    public string ProjectName { get; set; } = "NewProject";

    public string DefaultTarget { get; set; } = "build";

    public List < BuildTarget > Targets { get; set; } = new List < BuildTarget >();

    public string[]? IncludeFiles { get; set; } = Array.Empty < string >();

    public Dictionary < string, string > Variables { get; set; } = new Dictionary < string, string >();

    #region Public

    public static ProjectSettings Load( string path )
    {
        ProjectSettings settings = new ProjectSettings();
        Populate( settings, path );

        return settings;
    }

    public ProjectBuildContext CreateContext( string workingDir, string? targetName = null )
    {
        return new ProjectBuildContext( this, targetName ?? DefaultTarget, workingDir );
    }

    #endregion

    #region Private

    private static void Populate( ProjectSettings settings, string path )
    {
        JsonConvert.PopulateObject( File.ReadAllText( path ), settings );
        string dir = Path.GetDirectoryName( Path.GetFullPath( path ) )!;

        StringResolver.Resolve( settings );

        while ( settings.IncludeFiles != null )
        {
            string[] files = settings.IncludeFiles;
            settings.IncludeFiles = null;

            foreach ( string file in files )
            {
                if ( Path.IsPathRooted( file ) )
                {
                    Populate( settings, file );
                }
                else
                {
                    Populate( settings, Path.Combine( dir, file ) );
                }
            }
        }
    }

    #endregion

}
