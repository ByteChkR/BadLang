using System.Reflection;

namespace BadVM.Shared.PluginSystem;

public static class PluginLoader
{

    private static bool s_Initialized = false;
    private static readonly List < Plugin > s_Plugins = new List < Plugin >();

    #region Public

    public static void InitializePlugins(
        string pluginDirectory,
        string globalConfigDirectory,
        string presetConfigDirectory )
    {
        if ( s_Initialized )
        {
            return;
        }

        foreach ( string plugin in Directory.GetFiles( pluginDirectory, "*.dll", SearchOption.AllDirectories ) )
        {
            try
            {
                Assembly asm = Assembly.LoadFrom( plugin );
                Type[] types = asm.GetTypes();

                foreach ( Type type in types )
                {
                    if ( type.IsSubclassOf( typeof( Plugin ) ) )
                    {
                        Plugin pluginInstance = ( Plugin )Activator.CreateInstance( type );
                        s_Plugins.Add( pluginInstance );
                    }
                }
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        foreach ( Plugin plugin in s_Plugins )
        {
            string globalPluginConfigs = Path.Combine( globalConfigDirectory, plugin.Name );

            if ( !Directory.Exists( globalPluginConfigs ) )
            {
                Directory.CreateDirectory( globalPluginConfigs );
            }

            string presetPluginConfigs = Path.Combine( presetConfigDirectory, plugin.Name );

            if ( !Directory.Exists( presetPluginConfigs ) )
            {
                Directory.CreateDirectory( presetPluginConfigs );
            }

            plugin.InnerInitialize( globalPluginConfigs, presetPluginConfigs );
        }

        s_Initialized = true;
    }

    public static void LoadPlugins( object obj )
    {
        if ( !s_Initialized )
        {
            throw new PluginLoaderException( "PluginLoader not initialized" );
        }

        foreach ( Plugin plugin in s_Plugins )
        {
            plugin.InnerLoad( obj );
        }
    }

    #endregion

}
