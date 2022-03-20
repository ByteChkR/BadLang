using BadVM.Interop;

using basm;

using bvm;

namespace BadLang.TestFramework;

public static class TestRunner
{

    private static bool s_Initialized = false;

    #region Public

    public static void Initialize( string exportDir, string wrapperExportDir )
    {
        if ( !s_Initialized )
        {
            if ( !Directory.Exists( exportDir ) )
            {
                Directory.CreateDirectory( exportDir );
            }

            if ( !Directory.Exists( wrapperExportDir ) )
            {
                Directory.CreateDirectory( wrapperExportDir );
            }

            s_Initialized = true;

            InteropHelper.Register(
                                   new InteropAction < string >(
                                                                "TestFramework::Assert::Fail",
                                                                msg =>
                                                                {
                                                                    throw new TestRunnerException(
                                                                         $"Assertion failed: {msg}"
                                                                        );
                                                                }
                                                               )
                                  );

            InteropHelper.Register(
                                   new InteropAction < byte, byte, string >(
                                                                            "TestFramework::Assert::Equals8",
                                                                            ( value, expected, msg ) =>
                                                                            {
                                                                                if ( value != expected )
                                                                                {
                                                                                    throw new TestRunnerException(
                                                                                         $"Assertion failed: expected {expected}, got {value}: {msg}"
                                                                                        );
                                                                                }
                                                                            }
                                                                           )
                                  );

            InteropHelper.Register(
                                   new InteropAction < ushort, ushort, string >(
                                                                                "TestFramework::Assert::Equals16",
                                                                                ( value, expected, msg ) =>
                                                                                {
                                                                                    if ( value != expected )
                                                                                    {
                                                                                        throw new TestRunnerException(
                                                                                             $"Assertion failed: expected {expected}, got {value}: {msg}"
                                                                                            );
                                                                                    }
                                                                                }
                                                                               )
                                  );

            InteropHelper.Register(
                                   new InteropAction < uint, uint, string >(
                                                                            "TestFramework::Assert::Equals32",
                                                                            ( value, expected, msg ) =>
                                                                            {
                                                                                if ( value != expected )
                                                                                {
                                                                                    throw new TestRunnerException(
                                                                                         $"Assertion failed: expected {expected}, got {value}: {msg}"
                                                                                        );
                                                                                }
                                                                            }
                                                                           )
                                  );

            InteropHelper.Register(
                                   new InteropAction < ulong, ulong, string >(
                                                                              "TestFramework::Assert::Equals64",
                                                                              ( value, expected, msg ) =>
                                                                              {
                                                                                  if ( value != expected )
                                                                                  {
                                                                                      throw new TestRunnerException(
                                                                                           $"Assertion failed: expected {expected}, got {value}: {msg}"
                                                                                          );
                                                                                  }
                                                                              }
                                                                             )
                                  );

            InteropHelper.Register(
                                   new InteropAction < string, string, string >(
                                                                                "TestFramework::Assert::EqualsString",
                                                                                ( value, expected, msg ) =>
                                                                                {
                                                                                    if ( value != expected )
                                                                                    {
                                                                                        throw new TestRunnerException(
                                                                                             $"Assertion failed: expected {expected}, got {value}: {msg}"
                                                                                            );
                                                                                    }
                                                                                }
                                                                               )
                                  );

            InteropHelper.GenerateWrapperAssemblies().ExportToFolder( exportDir );

            BasmProgram.Main( new[] { "-i", exportDir, "-o", wrapperExportDir, "--compileInterop" } );
        }
    }

    public static void Run( string file, string tempDir )
    {
        string outFile = Path.Combine( tempDir, Path.GetFileNameWithoutExtension( file ) + ".bvm" );

        BasmProgram.Main( new[] { "-i", file, "-o", outFile } );

        BvmProgram.Main( new[] { outFile, "--synchonous" } );
    }

    #endregion

}
