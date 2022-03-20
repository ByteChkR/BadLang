namespace BadVM.Shared.PluginSystem;

public abstract class Plugin
{

    public string Name { get; }

    public string FriendlyName { get; }

    #region Public

    internal void InnerInitialize( string globalConfigDirectory, string presetConfigDirectory )
    {
        Initialize( globalConfigDirectory, presetConfigDirectory );
    }

    internal void InnerLoad( object o )
    {
        Load( o );
    }

    #endregion

    #region Protected

    protected Plugin( string name, string friendlyName )
    {
        Name = name;
        FriendlyName = friendlyName;
    }

    protected abstract void Initialize( string globalConfigDirectory, string presetConfigDirectory );

    protected abstract void Load( object o );

    #endregion

}

public abstract class Plugin < T > : Plugin
{

    #region Protected

    protected Plugin( string name, string friendlyName ) : base( name, friendlyName )
    {
    }

    protected abstract void Load( T o );

    protected override void Load( object o )
    {
        if ( o is T t )
        {
            Load( t );
        }
    }

    #endregion

}
