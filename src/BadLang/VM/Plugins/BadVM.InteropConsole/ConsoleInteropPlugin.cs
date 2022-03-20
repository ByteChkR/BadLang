using BadVM.Interop;
using BadVM.Shared.PluginSystem;

using Newtonsoft.Json;

namespace BadVM.InteropConsole
{

    public class ConsoleInteropPlugin : Plugin
    {

        private ConsoleInteropSettings? m_Settings;

        #region Public

        public ConsoleInteropPlugin() : base( "console_interop", "Console Interop Plugin" )
        {
        }

        #endregion

        #region Protected

        protected override void Initialize( string globalConfigDirectory, string presetConfigDirectory )
        {
            LoadInteropSettings( globalConfigDirectory, presetConfigDirectory );

            if ( !m_Settings!.Enable )
            {
                return;
            }

            InteropHelper.Register( new InteropAction < string >( "Console::Interop::WriteLine", Console.WriteLine ) );
            InteropHelper.Register( new InteropAction < string >( "Console::Interop::Write", Console.Write ) );
            InteropHelper.Register( new InteropAction < char >( "Console::Interop::WriteChar", Console.Write ) );
            InteropHelper.Register( new InteropAction < long >( "Console::Interop::WriteNum", Console.Write ) );

            InteropHelper.Register(
                                   new InteropAction < byte >( "Console::Interop::WriteNum8", b => Console.Write( b ) )
                                  );

            InteropHelper.Register(
                                   new InteropAction < long >(
                                                              "Console::Interop::WriteHex",
                                                              l => Console.Write( $"0x{l:X16}" )
                                                             )
                                  );

            InteropHelper.Register( new InteropAction( "Console::Interop::Clear", Console.Clear ) );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetBackColor",
                                                               () => ( int )Console.BackgroundColor
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetForeColor",
                                                               () => ( int )Console.ForegroundColor
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetCursorLeft",
                                                               () => Console.CursorLeft
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetCursorTop",
                                                               () => Console.CursorTop
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetCursorSize",
                                                               () => Console.CursorSize
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetWindowWidth",
                                                               () => Console.WindowWidth
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetWindowHeight",
                                                               () => Console.WindowHeight
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetBufferWidth",
                                                               () => Console.BufferWidth
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetBufferHeight",
                                                               () => Console.BufferHeight
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetLargestWindowWidth",
                                                               () => Console.LargestWindowWidth
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetLargestWindowHeight",
                                                               () => Console.LargestWindowHeight
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetWindowLeft",
                                                               () => Console.WindowLeft
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < int >(
                                                               "Console::Interop::GetWindowTop",
                                                               () => Console.WindowTop
                                                              )
                                  );

            InteropHelper.Register(
                                   new InteropFunction < byte >(
                                                                "Console::Interop::GetCursorVisible",
                                                                () => ( byte )( Console.CursorVisible ? 1 : 0 )
                                                               )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetForeColor",
                                                             x => Console.ForegroundColor = ( ConsoleColor )x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetBackColor",
                                                             x => Console.BackgroundColor = ( ConsoleColor )x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetCursorLeft",
                                                             x => Console.CursorLeft = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetCursorTop",
                                                             x => Console.CursorTop = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetCursorSize",
                                                             x => Console.CursorSize = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetWindowWidth",
                                                             x => Console.WindowWidth = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetWindowHeight",
                                                             x => Console.WindowHeight = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetBufferWidth",
                                                             x => Console.BufferWidth = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetBufferHeight",
                                                             x => Console.BufferHeight = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetWindowLeft",
                                                             x => Console.WindowLeft = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < int >(
                                                             "Console::Interop::SetWindowTop",
                                                             x => Console.WindowTop = x
                                                            )
                                  );

            InteropHelper.Register(
                                   new InteropAction < string >( "Console::Interop::SetTitle", x => Console.Title = x )
                                  );

            InteropHelper.Register(
                                   new InteropAction < byte >(
                                                              "Console::Interop::SetCursorVisible",
                                                              x => Console.CursorVisible = x != 0
                                                             )
                                  );

            InteropHelper.Register(
                                   new InteropAction < byte >(
                                                              "Console::Interop::SetCursorSize",
                                                              x => Console.CursorSize = x
                                                             )
                                  );

            foreach ( ConsoleColor color in Enum.GetValues( typeof( ConsoleColor ) ) )
            {
                InteropHelper.Register(
                                       new InteropFunction < int >( $"Console::Colors::{color}", () => ( int )color )
                                      );
            }
        }

        protected override void Load( object o )
        {
        }

        #endregion

        #region Private

        private void LoadInteropSettings( string globalConfigDirectory, string presetConfigDirectory )
        {
            string file = Path.Combine( presetConfigDirectory, "settings.json" );

            if ( File.Exists( file ) )
            {
                m_Settings = JsonConvert.DeserializeObject < ConsoleInteropSettings >( File.ReadAllText( file ) )!;

                return;
            }

            file = Path.Combine( globalConfigDirectory, "settings.json" );

            if ( File.Exists( file ) )
            {
                m_Settings = JsonConvert.DeserializeObject < ConsoleInteropSettings >( File.ReadAllText( file ) )!;
            }
            else
            {
                m_Settings = new ConsoleInteropSettings();
                File.WriteAllText( file, JsonConvert.SerializeObject( m_Settings, Formatting.Indented ) );
            }
        }

        #endregion

    }

}
