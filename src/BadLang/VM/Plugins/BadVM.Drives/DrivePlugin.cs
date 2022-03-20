using BadVM.Interop;
using BadVM.Shared.PluginSystem;

using Newtonsoft.Json;

namespace BadVM.Drives;

public class DrivePlugin : Plugin
{

    private DrivePluginSettings? m_Settings;

    private List < Drive > m_Drives = new List < Drive >();

    #region Public

    public DrivePlugin() : base( "drives", "Drive Plugin" )
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

        foreach ( DriveSettings drive in m_Settings.Drives )
        {
            if ( drive.PersistentPath != null )
            {
                m_Drives.Add( new Drive( drive.PersistentPath, drive.Size ) );
            }
            else
            {
                m_Drives.Add( new Drive( drive.Size ) );
            }
        }

        InteropHelper.Register(
                               new InteropFunction < int >( "Drive::Management::GetDriveCount", () => m_Drives.Count )
                              );

        InteropHelper.Register(
                               new InteropFunction < int, int >(
                                                                "Drive::Management::GetDriveSize",
                                                                ( index ) => m_Drives[index].Size
                                                               )
                              );

        InteropHelper.Register(
                               new InteropAction < int, int, byte >(
                                                                    "Drive::IO::WriteI8",
                                                                    ( id, addr, value ) =>
                                                                        m_Drives[id].Write( addr, value )
                                                                   )
                              );

        InteropHelper.Register(
                               new InteropFunction < int, int, byte >(
                                                                      "Drive::IO::ReadI8",
                                                                      ( id, addr ) => m_Drives[id].Read( addr )
                                                                     )
                              );

        InteropHelper.Register(
                               new InteropAction < int, int, short >(
                                                                     "Drive::IO::WriteI16",
                                                                     ( id, addr, value ) =>
                                                                         m_Drives[id].Write( addr, value )
                                                                    )
                              );

        InteropHelper.Register(
                               new InteropFunction < int, int, short >(
                                                                       "Drive::IO::ReadI16",
                                                                       ( id, addr ) => m_Drives[id].Read16( addr )
                                                                      )
                              );

        InteropHelper.Register(
                               new InteropAction < int, int, int >(
                                                                   "Drive::IO::WriteI32",
                                                                   ( id, addr, value ) =>
                                                                       m_Drives[id].Write( addr, value )
                                                                  )
                              );

        InteropHelper.Register(
                               new InteropFunction < int, int, int >(
                                                                     "Drive::IO::ReadI32",
                                                                     ( id, addr ) => m_Drives[id].Read32( addr )
                                                                    )
                              );

        InteropHelper.Register(
                               new InteropAction < int, int, long >(
                                                                    "Drive::IO::WriteI64",
                                                                    ( id, addr, value ) =>
                                                                        m_Drives[id].Write( addr, value )
                                                                   )
                              );

        InteropHelper.Register(
                               new InteropFunction < int, int, long >(
                                                                      "Drive::IO::ReadI64",
                                                                      ( id, addr ) => m_Drives[id].Read64( addr )
                                                                     )
                              );
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
            m_Settings = JsonConvert.DeserializeObject < DrivePluginSettings >( File.ReadAllText( file ) )!;

            return;
        }

        file = Path.Combine( globalConfigDirectory, "settings.json" );

        if ( File.Exists( file ) )
        {
            m_Settings = JsonConvert.DeserializeObject < DrivePluginSettings >( File.ReadAllText( file ) )!;
        }
        else
        {
            m_Settings = new DrivePluginSettings();
            File.WriteAllText( file, JsonConvert.SerializeObject( m_Settings, Formatting.Indented ) );
        }
    }

    #endregion

}
