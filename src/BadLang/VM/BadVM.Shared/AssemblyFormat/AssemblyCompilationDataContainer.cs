using BadVM.Shared.AssemblyFormat.Serialization;

namespace BadVM.Shared.AssemblyFormat;

public class AssemblyCompilationDataContainer
{

    private readonly Dictionary < Type, object > m_Data = new Dictionary < Type, object >();

    #region Public

    public void Export( string outputBinary )
    {
        foreach ( KeyValuePair < Type, object > kvp in m_Data )
        {
            if ( kvp.Value is ICompilationExporter exporter )
            {
                exporter.Export( outputBinary );
            }
        }
    }

    public T GetData < T >() where T : new()
    {
        if ( !m_Data.ContainsKey( typeof( T ) ) )
        {
            m_Data.Add( typeof( T ), new T() );
        }

        return ( T )m_Data[typeof( T )];
    }

    #endregion

}
