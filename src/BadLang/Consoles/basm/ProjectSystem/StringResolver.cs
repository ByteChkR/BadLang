using System.Reflection;

namespace basm.ProjectSystem;

public static class StringResolver
{

    #region Public

    public static void Resolve( ProjectSettings settings )
    {
        Dictionary < string, string > vars = new Dictionary < string, string >();

        foreach ( KeyValuePair < string, string > var in settings.Variables )
        {
            vars.Add( var.Key, var.Value );
        }

        ResolveObject( settings, vars );

        foreach ( BuildTarget target in settings.Targets )
        {
            ResolveObject( target, vars );

            foreach ( BuildDependency dependency in target.Dependencies )
            {
                ResolveObject( dependency, vars );
            }
        }

        ResolveStaticVariables( settings );
    }

    public static void Resolve( ProjectSettings settings, BuildTarget target )
    {
        Dictionary < string, string > vars = new Dictionary < string, string >();

        foreach ( KeyValuePair < string, string > var in settings.Variables )
        {
            vars.Add( var.Key, var.Value );
        }

        foreach ( KeyValuePair < string, string > var in target.Variables )
        {
            vars.Add( var.Key, var.Value );
        }

        ResolveObject( settings, vars );
        ResolveObject( target, vars );

        foreach ( BuildDependency dependency in target.Dependencies )
        {
            ResolveObject( dependency, vars );
        }

        ResolveStaticVariables( settings, target );
    }

    #endregion

    #region Private

    private static string InnerResolve(
        string value,
        Dictionary < string, string > variables )
    {
        int varStart = value.IndexOf( "$(", StringComparison.Ordinal );

        while ( varStart != -1 )
        {
            int varEnd = value.IndexOf( ')', varStart );

            if ( varEnd == -1 )
            {
                throw new Exception( "Unclosed variable" );
            }

            string varName = value.Substring( varStart + 2, varEnd - varStart - 2 );

            if ( variables.TryGetValue( varName, out string? varValue ) )
            {
                value = value.Substring( 0, varStart ) + varValue + value.Substring( varEnd + 1 );

                varStart = value.IndexOf( "$(", StringComparison.Ordinal );
            }
            else
            {
                varStart = value.IndexOf( "$(", varStart + 1, StringComparison.Ordinal );
            }
        }

        return value;
    }

    private static void ResolveObject( object o, Dictionary < string, string > variables )
    {
        PropertyInfo[] properties = o.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );

        foreach ( PropertyInfo propertyInfo in properties )
        {
            object? v = propertyInfo.GetValue( o );

            if ( v is string s )
            {
                propertyInfo.SetValue( o, InnerResolve( s, variables ) );
            }
            else if ( v is string[] e )
            {
                List < string > elements = new List < string >();

                foreach ( string element in e )
                {
                    elements.Add( InnerResolve( element, variables ) );
                }

                propertyInfo.SetValue( o, elements.ToArray() );
            }
            else if ( v is Dictionary < string, string > d )
            {
                Dictionary < string, string > elements = new Dictionary < string, string >();

                foreach ( KeyValuePair < string, string > keyValuePair in d )
                {
                    elements.Add(
                                 InnerResolve( keyValuePair.Key, variables ),
                                 InnerResolve( keyValuePair.Value, variables )
                                );
                }

                propertyInfo.SetValue( o, elements );
            }
        }
    }

    private static void ResolveStaticVariables( ProjectSettings settings )
    {
        Dictionary < string, string > vars = new Dictionary < string, string >();
        vars.Add( "ProjectName", settings.ProjectName );
        vars.Add( "CompilerDirectory", AppDomain.CurrentDomain.BaseDirectory );

        ResolveObject( settings, vars );

        foreach ( BuildTarget target in settings.Targets )
        {
            ResolveObject( target, vars );

            foreach ( BuildDependency dependency in target.Dependencies )
            {
                ResolveObject( dependency, vars );
            }
        }
    }

    private static void ResolveStaticVariables( ProjectSettings settings, BuildTarget target )
    {
        Dictionary < string, string > vars = new Dictionary < string, string >();
        vars.Add( "ProjectName", settings.ProjectName );
        vars.Add( "Target", target.Name );
        vars.Add( "CompilerDirectory", AppDomain.CurrentDomain.BaseDirectory );

        ResolveObject( settings, vars );
        ResolveObject( target, vars );
    }

    #endregion

}
