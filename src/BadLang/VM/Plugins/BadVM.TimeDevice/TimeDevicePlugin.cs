using BadVM.Interop;
using BadVM.Shared.PluginSystem;

using Newtonsoft.Json;

namespace BadVM.TimeDevice;

public class TimeDevicePlugin : Plugin
{

    private TimeDeviceSettings? m_Settings;

    #region Public

    public TimeDevicePlugin() : base( "time_device", "Time Device" )
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

        InteropHelper.Register( new InteropFunction < long >( "TimeDevice::Time::GetUnixTime", GetUnixTime ) );
    }

    protected override void Load( object o )
    {
    }

    #endregion

    #region Private

    private long GetUnixTime()
    {
        return DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    private void LoadInteropSettings( string globalConfigDirectory, string presetConfigDirectory )
    {
        string file = Path.Combine( presetConfigDirectory, "settings.json" );

        if ( File.Exists( file ) )
        {
            m_Settings = JsonConvert.DeserializeObject < TimeDeviceSettings >( File.ReadAllText( file ) )!;

            return;
        }

        file = Path.Combine( globalConfigDirectory, "settings.json" );

        if ( File.Exists( file ) )
        {
            m_Settings = JsonConvert.DeserializeObject < TimeDeviceSettings >( File.ReadAllText( file ) )!;
        }
        else
        {
            m_Settings = new TimeDeviceSettings();
            File.WriteAllText( file, JsonConvert.SerializeObject( m_Settings, Formatting.Indented ) );
        }
    }

    #endregion

}
