using System.Text;

using BadVM.Core;
using BadVM.Interop.Internal;
using BadVM.Shared.Memory;

namespace BadVM.Interop;

public static class InteropHelper
{

    private static readonly List < InnerInteropFunction > s_Functions = new List < InnerInteropFunction >();

    #region Public

    public static void Call( VirtualCore core, MemoryBus bus )
    {
        long call = bus.ReadInt64( core.StackPointer ) - 1;
        core.FreeStackPointer( sizeof( long ) );

        s_Functions[( int )call].Invoke( core, bus );
    }

    public static WrapperResult GenerateWrapperAssemblies()
    {
        // Find All Symbols and order them into the correct assemblies

        Dictionary < string, Dictionary < string, List < InnerInteropFunction > > > assemblyMap =
            new Dictionary < string, Dictionary < string, List < InnerInteropFunction > > >();

        foreach ( InnerInteropFunction function in s_Functions )
        {
            if ( !assemblyMap.ContainsKey( function.Name.AssemblyName ) )
            {
                assemblyMap[function.Name.AssemblyName] = new Dictionary < string, List < InnerInteropFunction > >();
            }

            if ( !assemblyMap[function.Name.AssemblyName].ContainsKey( function.Name.SectionName ) )
            {
                assemblyMap[function.Name.AssemblyName][function.Name.SectionName] =
                    new List < InnerInteropFunction >();
            }

            assemblyMap[function.Name.AssemblyName][function.Name.SectionName].Add( function );
        }

        Dictionary < string, List < (StringBuilder asm, StringBuilder header) > > sectionCode =
            new Dictionary < string, List < (StringBuilder asm, StringBuilder header) > >();

        foreach ( KeyValuePair < string, Dictionary < string, List < InnerInteropFunction > > > assembly in
                 assemblyMap )
        {
            sectionCode[assembly.Key] = new List < (StringBuilder asm, StringBuilder header) >();

            foreach ( KeyValuePair < string, List < InnerInteropFunction > > section in assembly.Value )
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder header = new StringBuilder();

                header.AppendLine( $".code {section.Key} code_raw BadC" );
                header.AppendLine( "{" );

                foreach ( InnerInteropFunction function in section.Value )
                {
                    EmitHeader( function, header );
                }

                header.AppendLine( "}" );
                header.AppendLine();

                sb.AppendLine();
                sb.AppendLine( $".data {section.Key}_Data data_raw BASM_DATA" );
                sb.AppendLine( "{" );

                foreach ( InnerInteropFunction function in section.Value )
                {
                    EmitWrapperData( function, sb );
                }

                sb.AppendLine( "}" );
                sb.AppendLine();

                sb.AppendLine( $".code {section.Key} code_raw BASM" );
                sb.AppendLine( "{" );

                foreach ( InnerInteropFunction function in section.Value )
                {
                    EmitWrapperCode( function, sb );
                }

                sb.AppendLine( "}" );
                sb.AppendLine();

                sectionCode[assembly.Key].Add( ( sb, header ) );
            }
        }

        Dictionary < string, (string asm, string header) > ret =
            new Dictionary < string, (string asm, string header) >();

        foreach ( KeyValuePair < string, List < (StringBuilder asm, StringBuilder header) > > assembly in sectionCode )
        {
            StringBuilder code = new StringBuilder();
            StringBuilder header = new StringBuilder();

            foreach ( ( StringBuilder asm, StringBuilder headerSb ) in assembly.Value )
            {
                code.Append( asm );
                header.Append( headerSb );
            }

            ret[assembly.Key] = ( code.ToString(), header.ToString() );
        }

        return new WrapperResult( ret );
    }

    public static void Register( InnerInteropFunction function )
    {
        s_Functions.Add( function );
    }

    public static long Resolve( string name )
    {
        for ( int i = 0; i < s_Functions.Count; i++ )
        {
            if ( s_Functions[i].Name.ToString() == name )
            {
                return i + 1;
            }
        }

        throw new InteropFunctionNotFoundException( $"InteropFunction {name} not found" );
    }

    internal static string ConvertType < T >()
    {
        if ( typeof( T ) == typeof( byte ) ||
             typeof( T ) == typeof( sbyte ) )
        {
            return "i8";
        }

        if ( typeof( T ) == typeof( short ) ||
             typeof( T ) == typeof( ushort ) )
        {
            return "i16";
        }

        if ( typeof( T ) == typeof( int ) ||
             typeof( T ) == typeof( uint ) )
        {
            return "i32";
        }

        if ( typeof( T ) == typeof( long ) ||
             typeof( T ) == typeof( ulong ) )
        {
            return "i64";
        }

        if ( typeof( T ) == typeof( string ) )
        {
            return "i16*";
        }

        if ( typeof( T ) == typeof( char ) )
        {
            return "i16";
        }

        throw new NotSupportedException( "Can not convert type " + typeof( T ).Name );
    }

    #endregion

    #region Private

    private static void EmitHeader( InnerInteropFunction func, StringBuilder sb )
    {
        sb.AppendLine();
        sb.AppendLine( $"\textern {func.GetSignature()} : assembly;" );

        sb.AppendLine();
    }

    private static void EmitWrapperCode( InnerInteropFunction func, StringBuilder sb )
    {
        sb.AppendLine();
        sb.AppendLine( $"\t; Exported function {func.Name}" );

        sb.AppendLine( $"\t{func.Name.SymbolName}: export" );
        sb.AppendLine( $"\t\tPushI64 {func.Name.SectionName}_Data::{func.Name.SymbolName}" );
        sb.AppendLine( "\t\tLoadI64" );
        sb.AppendLine( $"\t\tPushI64 {func.Name.SymbolName}_CALL" );
        sb.AppendLine( "\t\tBranchNotZeroI64" );

        sb.AppendLine( $"\t\t; Resolve Function Handle {func.Name}" );
        sb.AppendLine( $"\t\tPushI64 {func.Name.SectionName}_Data::{func.Name.SymbolName}_NAME" );
        sb.AppendLine( "\t\tInteropResolve" );
        sb.AppendLine( $"\t\tPushI64 {func.Name.SectionName}_Data::{func.Name.SymbolName}" );
        sb.AppendLine( "\t\tStoreI64" );

        sb.AppendLine();

        sb.AppendLine( $"\t; Call interop function {func.Name}" );
        sb.AppendLine( $"\t{func.Name.SymbolName}_CALL: local" );
        sb.AppendLine( $"\t\tPushI64 {func.Name.SectionName}_Data::{func.Name.SymbolName}" );
        sb.AppendLine( "\t\tLoadI64" );
        sb.AppendLine( "\t\tInteropCall" );
        sb.AppendLine( "\t\tReturn" );

        sb.AppendLine();
        sb.AppendLine();
    }

    private static void EmitWrapperData( InnerInteropFunction func, StringBuilder sb )
    {
        sb.AppendLine();
        sb.AppendLine( $"\t.cstr {func.Name.SymbolName}_NAME assembly \"{func.Name}\"" );
        sb.AppendLine( $"\t.i64 {func.Name.SymbolName} assembly 0" );
        sb.AppendLine();
    }

    #endregion

}
