using System.IO;
using System.Linq;

using BadAssembler.Exceptions;

using BadLang.TestFramework;

using BadVM.Shared.AssemblyFormat.Exceptions;

using NUnit.Framework;

namespace BadC.Tests;

public class Tests
{

    private static readonly string s_PassingTestSourceDir =
        Path.Combine( TestContext.CurrentContext.TestDirectory, "tests", "passing" );

    private static readonly string s_ParseExceptionTestSourceDir =
        Path.Combine( TestContext.CurrentContext.TestDirectory, "tests", "failing", "ParseException" );

    private static readonly string s_SymbolNotFoundExceptionTestSourceDir =
        Path.Combine( TestContext.CurrentContext.TestDirectory, "tests", "failing", "SymbolNotFoundException" );

    private static readonly string s_TypeMismatchExceptionTestSourceDir =
        Path.Combine( TestContext.CurrentContext.TestDirectory, "tests", "failing", "TypeMismatchException" );

    private static readonly string s_LibDir = Path.Combine(
                                                           TestContext.CurrentContext.TestDirectory,
                                                           "data",
                                                           "runtime",
                                                           "libs"
                                                          );

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

    #region Public

    public static string[] ParseExceptionTestFiles()
    {
        return Directory.GetFiles( s_ParseExceptionTestSourceDir, "*.basm", SearchOption.AllDirectories ).
                         Select( x => x.Replace( s_ParseExceptionTestSourceDir + Path.DirectorySeparatorChar, "" ) ).
                         ToArray();
    }

    public static string[] PassingTestFiles()
    {
        return Directory.GetFiles( s_PassingTestSourceDir, "*.basm", SearchOption.AllDirectories ).
                         Select( x => x.Replace( s_PassingTestSourceDir + Path.DirectorySeparatorChar, "" ) ).
                         ToArray();
    }

    public static string[] SymbolNotFoundExceptionTestFiles()
    {
        return Directory.GetFiles( s_SymbolNotFoundExceptionTestSourceDir, "*.basm", SearchOption.AllDirectories ).
                         Select(
                                x => x.Replace(
                                               s_SymbolNotFoundExceptionTestSourceDir + Path.DirectorySeparatorChar,
                                               ""
                                              )
                               ).
                         ToArray();
    }

    public static string[] TypeMismatchExceptionTestFiles()
    {
        return Directory.GetFiles( s_TypeMismatchExceptionTestSourceDir, "*.basm", SearchOption.AllDirectories ).
                         Select(
                                x => x.Replace( s_TypeMismatchExceptionTestSourceDir + Path.DirectorySeparatorChar, "" )
                               ).
                         ToArray();
    }

    [Test]
    [TestCaseSource( nameof( ParseExceptionTestFiles ) )]
    public void ParseExceptionTests( string file )
    {
        Assert.Throws < ParseException >(
                                         () =>
                                         {
                                             TestRunner.Run(
                                                            s_ParseExceptionTestSourceDir +
                                                            Path.DirectorySeparatorChar +
                                                            file,
                                                            s_TempDir
                                                           );
                                         }
                                        );
    }

    [Test]
    [TestCaseSource( nameof( PassingTestFiles ) )]
    public void PassingTests( string file )
    {
        TestRunner.Run( s_PassingTestSourceDir + Path.DirectorySeparatorChar + file, s_TempDir );
    }

    [SetUp]
    public void Setup()
    {
        Directory.CreateDirectory( s_ParseExceptionTestSourceDir );
        Directory.CreateDirectory( s_SymbolNotFoundExceptionTestSourceDir );
        Directory.CreateDirectory( s_TypeMismatchExceptionTestSourceDir );
        Directory.CreateDirectory( s_PassingTestSourceDir );
        Directory.CreateDirectory( s_TempDir );
        Directory.CreateDirectory( s_ExportDir );
        Directory.CreateDirectory( s_LibDir );

        TestRunner.Initialize( s_ExportDir, s_LibDir );
    }

    [Test]
    [TestCaseSource( nameof( SymbolNotFoundExceptionTestFiles ) )]
    public void SymbolNotFoundExceptionTests( string file )
    {
        Assert.Throws < SymbolNotFoundException >(
                                                  () =>
                                                  {
                                                      TestRunner.Run(
                                                                     s_SymbolNotFoundExceptionTestSourceDir +
                                                                     Path.DirectorySeparatorChar +
                                                                     file,
                                                                     s_TempDir
                                                                    );
                                                  }
                                                 );
    }

    [Test]
    [TestCaseSource( nameof( TypeMismatchExceptionTestFiles ) )]
    public void TypeMismatchExceptionTests( string file )
    {
        Assert.Throws < TypeMismatchException >(
                                                () =>
                                                {
                                                    TestRunner.Run(
                                                                   s_TypeMismatchExceptionTestSourceDir +
                                                                   Path.DirectorySeparatorChar +
                                                                   file,
                                                                   s_TempDir
                                                                  );
                                                }
                                               );
    }

    #endregion

}
