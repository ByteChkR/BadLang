using BadAssembler;
using BadAssembler.AssemblerSyntax;
using BadAssembler.Segments;

using BadC;

using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats;

namespace basm.ProjectSystem;

public class ProjectBuildContext
{

    private readonly Stack < ProjectBuildContext > m_SubContexts = new Stack < ProjectBuildContext >();

    private readonly ProjectSettings m_Settings;
    private readonly string m_TargetName;
    private readonly string m_WorkingDir;

    private BuildTarget Target => m_Settings.Targets.First( x => x.Name == m_TargetName );

    #region Public

    public ProjectBuildContext( ProjectSettings settings, string target, string workingDir )
    {
        m_Settings = settings;
        m_TargetName = target;
        m_WorkingDir = workingDir;
        StringResolver.Resolve( settings );

        foreach ( BuildDependency dependency in Target.Dependencies )
        {
            ProjectSettings subSettings;

            string file;

            if ( Path.IsPathRooted( dependency.File ) )
            {
                file = dependency.File;
            }
            else
            {
                file = Path.Combine( workingDir, dependency.File );
            }

            subSettings = ProjectSettings.Load( file );

            m_SubContexts.Push(
                               subSettings.CreateContext(
                                                         Path.GetDirectoryName( Path.GetFullPath( file ) )!,
                                                         dependency.Target ?? subSettings.DefaultTarget
                                                        )
                              );
        }

        foreach ( string subTarget in Target.SubTargets )
        {
            m_SubContexts.Push( new ProjectBuildContext( settings, subTarget, workingDir ) );
        }

        StringResolver.Resolve( settings, Target );
    }

    public void Run()
    {
        while ( m_SubContexts.Count > 0 )
        {
            m_SubContexts.Pop().Run();
        }

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
        AssemblyWriter writer = new AssemblyWriter( m_Settings.ProjectName, AssemblySectionFormat.Formats );
        string outDir;
        string outFile;

        if ( Path.IsPathRooted( Target.OutputFile ) )
        {
            outFile = Path.GetFullPath( Target.OutputFile );
            outDir = Path.GetDirectoryName( outFile )!;
        }
        else
        {
            outFile = Path.GetFullPath( Path.Combine( m_WorkingDir, Target.OutputFile ) );
            outDir = Path.GetDirectoryName( outFile )!;
        }

        if ( !Directory.Exists( outDir ) )
        {
            Directory.CreateDirectory( outDir );
        }

        ( byte[] asmBytes, AssemblyCompilationDataContainer data ) =
            assembler.Assemble( Commandline.LoadFiles( Target.Sources, m_WorkingDir ), writer );

        File.WriteAllBytes( outFile, asmBytes );
        data.Export( outFile );
    }

    #endregion

}
