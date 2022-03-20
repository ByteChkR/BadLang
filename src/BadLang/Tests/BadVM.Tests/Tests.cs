using System.IO;
using System.Linq;

using BadLang.TestFramework;

using BadVM.Shared.Memory;
using BadVM.Shared.Memory.Exceptions;

using NUnit.Framework;

namespace BadVM.Tests;

public class Tests
{

    private static readonly string s_TestSourceDir =
        Path.Combine( TestContext.CurrentContext.TestDirectory, "tests", "passing", "opcodes" );

    private static readonly string s_TempDir = Path.Combine(
                                                            TestContext.CurrentContext.TestDirectory,
                                                            "tests_temp"
                                                           );

    private static readonly string s_ExportDir = Path.Combine(
                                                              TestContext.CurrentContext.TestDirectory,
                                                              "data",
                                                              "runtime",
                                                              "interop"
                                                             );

    private static readonly string s_LibDir = Path.Combine(
                                                           TestContext.CurrentContext.TestDirectory,
                                                           "data",
                                                           "runtime",
                                                           "libs"
                                                          );

    #region Public

    public static string[] OpCodeTestFiles()
    {
        if ( !Directory.Exists( s_TestSourceDir ) )
        {
            Directory.CreateDirectory( s_TestSourceDir );
        }

        return Directory.GetFiles( s_TestSourceDir, "*.basm", SearchOption.AllDirectories ).
                         Select( x => x.Replace( s_TestSourceDir + Path.DirectorySeparatorChar, "" ) ).
                         ToArray();
    }

    [Test]
    [TestCaseSource( nameof( OpCodeTestFiles ) )]
    public void OpCodes( string file )
    {
        TestRunner.Run( s_TestSourceDir + Path.DirectorySeparatorChar + file, s_TempDir );
    }

    [Test]
    public void OverlappingMemory()
    {
        Assert.Throws < MemoryMapConflictException >(
                                                     () =>
                                                     {
                                                         MemoryBus bus = new MemoryBus();
                                                         byte[] data = new byte[0x1000];

                                                         bus.Register( data.ToMemoryEntry( 0 ) );
                                                         bus.Register( data.ToMemoryEntry( 0x800 ) );
                                                     }
                                                    );
    }

    [SetUp]
    public void Setup()
    {
        if ( !Directory.Exists( s_TempDir ) )
        {
            Directory.CreateDirectory( s_TempDir );
        }

        TestRunner.Initialize( s_ExportDir, s_LibDir );
    }

    #endregion

}
