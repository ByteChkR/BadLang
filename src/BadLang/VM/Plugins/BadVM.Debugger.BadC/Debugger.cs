using System.Globalization;
using System.Text;

using BadVM.Core;
using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;
using BadVM.Shared.PluginSystem;

using Newtonsoft.Json;

namespace BadVM.Debugger.BadC;

public class Debugger : Plugin < DebuggerHost >, IDebugger
{

    private readonly LogMask m_LogMask;

    private readonly Dictionary < string, Action < VirtualCore, string[] > > m_Commands =
        new Dictionary < string, Action < VirtualCore, string[] > >();

    private readonly List < DebugSymbol > m_Symbols = new List < DebugSymbol >();

    private readonly List < DebugSymbol > m_MappedSymbols = new List < DebugSymbol >();
    private readonly List < DebugSymbol > m_BreakSymbols = new List < DebugSymbol >();
    private readonly List < long > m_BreakAddress = new List < long >();

    private readonly Dictionary < string, string > m_SourceCache = new Dictionary < string, string >();

    private readonly Dictionary < VirtualCore, Stack < (string sym, long addr) > > m_CallStacks =
        new Dictionary < VirtualCore, Stack < (string sym, long addr) > >();

    private readonly Dictionary < AssemblySection, long > m_MappedSections = new Dictionary < AssemblySection, long >();

    private int m_LastLine = -1;

    private bool m_IsFirstCycle = true;
    private bool m_Step;
    private bool m_Run;

    #region Public

    public Debugger() : base( "badc_debugger", "BadC_Debugger" )
    {
        m_LogMask = new LogMask( FriendlyName );

        m_Commands["help"] = ( _, _ ) => Help();

        m_Commands["step"] = ( _, _  ) =>
                             {
                                 m_Step = !m_Step;
                                 m_LogMask.Info( $"Step Mode: {m_Step}" );
                             };

        m_Commands["callstack"] = ( core, _ ) =>
                                  {
                                      string s = GetStackTrace( core );
                                      m_LogMask.Info( s );
                                  };

        m_Commands["break"] = ( _, args ) =>
                              {
                                  foreach ( string arg in args )
                                  {
                                      if ( arg.StartsWith( "0x" ) )
                                      {
                                          long addr = Convert.ToInt64( arg.Substring( 2 ), 16 );
                                          m_BreakAddress.Add( addr );
                                          m_LogMask.Info( $"Breakpoint set at {GetSymbolName( addr )}" );
                                      }
                                      else
                                      {
                                          try
                                          {
                                              AssemblySymbol sym = AssemblySymbol.Parse( arg );
                                              m_BreakAddress.Add( GetSymbolAddress( sym ) );
                                              m_LogMask.Info( $"Breakpoint set at {sym}" );
                                          }
                                          catch ( Exception )
                                          {
                                              m_LogMask.Warning( $"can not parse symbol: '{arg}'" );
                                          }
                                      }
                                  }
                              };

        m_Commands["unbreak"] = ( _, args ) =>
                                {
                                    foreach ( string arg in args )
                                    {
                                        if ( arg.StartsWith( "0x" ) )
                                        {
                                            long addr = Convert.ToInt64( arg.Substring( 2 ), 16 );

                                            if ( !m_BreakAddress.Contains( addr ) )
                                            {
                                                continue;
                                            }

                                            m_BreakAddress.Remove( addr );
                                            m_LogMask.Info( $"Breakpoint unset at {GetSymbolName( addr )}" );
                                        }
                                        else
                                        {
                                            try
                                            {
                                                AssemblySymbol sym = AssemblySymbol.Parse( arg );
                                                long addr = GetSymbolAddress( sym );

                                                if ( !m_BreakAddress.Contains( addr ) )
                                                {
                                                    continue;
                                                }

                                                m_BreakAddress.Remove( addr );
                                                m_LogMask.Info( $"Breakpoint unset at {GetSymbolName( addr )}" );
                                            }
                                            catch ( Exception )
                                            {
                                                m_LogMask.Warning( $"can not parse symbol: '{arg}'" );
                                            }
                                        }
                                    }
                                };

        m_Commands["breakpoints"] = ( _, _ ) =>
                                    {
                                        foreach ( long addr in m_BreakAddress )
                                        {
                                            m_LogMask.Info( $"Breakpoint set at {GetSymbolName( addr )}" );
                                        }
                                    };

        m_Commands["linebreak"] = ( _, args ) =>
                                  {
                                      foreach ( string line in args )
                                      {
                                          int split = line.LastIndexOf( ':' );

                                          if ( split == -1 )
                                          {
                                              m_LogMask.Warning( $"can not parse line: '{line}'" );

                                              continue;
                                          }

                                          string file = line.Substring( 0, split );
                                          string lineStr = line.Substring( split + 1 );
                                          int lineNum = Convert.ToInt32( lineStr );

                                          foreach ( DebugSymbol symbol in m_MappedSymbols )
                                          {
                                              if ( file != symbol.SourceToken.FileName )
                                              {
                                                  continue;
                                              }

                                              string l = GetSourceExcerpt( symbol, out int symLine );

                                              if ( lineNum != symLine )
                                              {
                                                  continue;
                                              }

                                              m_BreakSymbols.Add( symbol );

                                              m_LogMask.Info(
                                                             $"Breakpoint set at {Path.GetFileName( symbol.SourceToken.FileName )}:{symLine} : '{l}'"
                                                            );

                                              return;
                                          }

                                          foreach ( DebugSymbol symbol in m_MappedSymbols )
                                          {
                                              if ( Path.GetFileName( file ) !=
                                                   Path.GetFileName( symbol.SourceToken.FileName ) )
                                              {
                                                  continue;
                                              }

                                              string l = GetSourceExcerpt( symbol, out int symLine );

                                              if ( lineNum != symLine )
                                              {
                                                  continue;
                                              }

                                              m_BreakSymbols.Add( symbol );

                                              m_LogMask.Info(
                                                             $"Breakpoint set at {Path.GetFileName( symbol.SourceToken.FileName )}:{symLine} : '{l}'"
                                                            );

                                              return;
                                          }
                                      }
                                  };

        m_Commands["linebreakpoints"] = ( _, _ ) =>
                                        {
                                            foreach ( DebugSymbol symbol in m_BreakSymbols )
                                            {
                                                string l = GetSourceExcerpt( symbol, out int symLine );

                                                m_LogMask.Info(
                                                               $"Breakpoint set at {Path.GetFileName( symbol.SourceToken.FileName )}:{symLine} : '{l}'"
                                                              );
                                            }
                                        };

        m_Commands["halt"] = ( core, _ ) => { core.Halt(); };
        m_Commands["breakclear"] = ( _, _ ) => { m_BreakAddress.Clear(); };
        m_Commands["linebreakclear"] = ( _, _ ) => { m_BreakSymbols.Clear(); };

        m_Commands["lineunbreak"] = ( _, args ) =>

                                    {
                                        foreach ( string line in args )
                                        {
                                            int split = line.LastIndexOf( ':' );

                                            if ( split == -1 )
                                            {
                                                m_LogMask.Warning( $"can not parse line: '{line}'" );

                                                continue;
                                            }

                                            string file = line.Substring( 0, split );
                                            string lineStr = line.Substring( split + 1 );
                                            int lineNum = Convert.ToInt32( lineStr );

                                            foreach ( DebugSymbol symbol in m_MappedSymbols )
                                            {
                                                if ( file != symbol.SourceToken.FileName )
                                                {
                                                    continue;
                                                }

                                                string l = GetSourceExcerpt( symbol, out int symLine );

                                                if ( lineNum != symLine || !m_BreakSymbols.Contains( symbol ) )
                                                {
                                                    continue;
                                                }

                                                m_BreakSymbols.Remove( symbol );
                                                m_LogMask.Info( $"Breakpoint unset at {symLine}: {l}" );

                                                return;
                                            }

                                            foreach ( DebugSymbol symbol in m_MappedSymbols )
                                            {
                                                if ( Path.GetFileName( file ) !=
                                                     Path.GetFileName( symbol.SourceToken.FileName ) )
                                                {
                                                    continue;
                                                }

                                                string l = GetSourceExcerpt( symbol, out int symLine );

                                                if ( lineNum != symLine || !m_BreakSymbols.Contains( symbol ) )
                                                {
                                                    continue;
                                                }

                                                m_BreakSymbols.Remove( symbol );
                                                m_LogMask.Info( $"Breakpoint unset at {symLine}: {l}" );

                                                return;
                                            }
                                        }
                                    };

        m_Commands["symbols"] = ( _, _ ) =>
                                {
                                    IEnumerable < string > symbols = m_MappedSymbols.Select(
                                             s =>
                                             {
                                                 string l = GetSourceExcerpt( s, out int symLine );

                                                 return $"{Path.GetFileName( s.SourceToken.FileName )}:{symLine} '{l}'";
                                             }
                                            ).
                                        Distinct();

                                    foreach ( string symbol in symbols )
                                    {
                                        m_LogMask.Info( symbol );
                                    }
                                };

        m_Commands["elements"] = ( _, _ ) =>
                                 {
                                     IEnumerable < string > symbols = m_MappedSections.SelectMany(
                                              s => s.Key.MetaData.Elements.Select( x => x.Name.ToString() )
                                             ).
                                         Distinct();

                                     foreach ( string symbol in symbols )
                                     {
                                         m_LogMask.Info( symbol );
                                     }
                                 };

        m_Commands["stack"] = ( core, args ) =>
                              {
                                  if ( args.Length < 1 || args.Length > 2 )
                                  {
                                      m_LogMask.Warning( "Usage: stack <size> <offset>" );

                                      return;
                                  }

                                  int size = 0;

                                  if ( args[0].StartsWith( "0x" ) )
                                  {
                                      try
                                      {
                                          size = int.Parse( args[0].Remove( 0, 2 ), NumberStyles.HexNumber );
                                      }
                                      catch ( Exception )
                                      {
                                          m_LogMask.Warning( $"Invalid size '{args[0]}'" );

                                          return;
                                      }
                                  }
                                  else if ( !int.TryParse( args[0], out size ) )
                                  {
                                      m_LogMask.Warning( $"Invalid size '{args[0]}'" );

                                      return;
                                  }

                                  int offset = 0;

                                  if ( args.Length == 2 )
                                  {
                                      if ( args[1].StartsWith( "0x" ) )
                                      {
                                          try
                                          {
                                              offset = int.Parse( args[1].Remove( 0, 2 ), NumberStyles.HexNumber );
                                          }
                                          catch ( Exception )
                                          {
                                              m_LogMask.Warning( $"Invalid offset '{args[1]}'" );

                                              return;
                                          }
                                      }
                                      else if ( !int.TryParse( args[1], out offset ) )
                                      {
                                          m_LogMask.Warning( $"Invalid offset '{args[1]}'" );

                                          return;
                                      }
                                  }

                                  m_LogMask.Info( $"Stack dump: size={size}, offset={offset}" );

                                  switch ( size )
                                  {
                                      case 1:
                                          m_LogMask.Info(
                                                         $"0x{core.StackPointer + offset:X16}: {core.MemoryBus.Read( core.StackPointer + offset ):X2}"
                                                        );

                                          break;

                                      case 2:
                                          m_LogMask.Info(
                                                         $"0x{core.StackPointer + offset:X16}: {core.MemoryBus.ReadUInt16( core.StackPointer + offset ):X4}"
                                                        );

                                          break;

                                      case 4:
                                          m_LogMask.Info(
                                                         $"0x{core.StackPointer + offset:X16}: {core.MemoryBus.ReadUInt32( core.StackPointer + offset ):X8}"
                                                        );

                                          break;

                                      case 8:
                                          m_LogMask.Info(
                                                         $"0x{core.StackPointer + offset:X16}: {core.MemoryBus.ReadUInt64( core.StackPointer + offset ):X16}"
                                                        );

                                          break;

                                      default:
                                          byte[] buffer = new byte[size];
                                          core.MemoryBus.Read( buffer, 0, core.StackPointer + offset, size );

                                          StringBuilder sb = new StringBuilder( "\n" );

                                          for ( int i = 0; i < buffer.Length; i++ )
                                          {
                                              byte b = buffer[i];

                                              if ( i % sizeof( long ) == 0 )
                                              {
                                                  sb.Append( $"0x{core.StackPointer + offset + i:X16}: " );
                                              }

                                              sb.Append(
                                                        $"{b:X2}"
                                                       );

                                              if ( i % sizeof( long ) == sizeof( long ) - 1 )
                                              {
                                                  sb.AppendLine();
                                              }
                                              else
                                              {
                                                  sb.Append( ' ' );
                                              }
                                          }

                                          m_LogMask.Info( sb.ToString() );

                                          break;
                                  }
                              };

        m_Commands["clear"] = ( _, _ ) => Console.Clear();

        m_Commands["run"] = ( _, _ ) => m_Run = true;
    }

    public void OnAssemblyLoad( Assembly asm, string? pathHint )
    {
        m_LogMask.Info( $"Loaded Assembly {asm.Name} from {pathHint ?? "null"}." );
        string? symbolPath = Path.ChangeExtension( pathHint, ".sym.json" );

        if ( symbolPath != null && File.Exists( symbolPath ) )
        {
            try
            {
                List < DebugSymbol > symbols =
                    JsonConvert.DeserializeObject < List < DebugSymbol > >( File.ReadAllText( symbolPath ) )!;

                m_Symbols.AddRange( symbols );
            }
            catch ( Exception )
            {
                m_LogMask.Warning( $"Can not load symbols from '{symbolPath}'" );
            }
        }
        else
        {
            m_LogMask.Warning( $"Can not find debug symbols '{symbolPath}'" );
        }
    }

    public void OnCoreCycle( VirtualCore core )
    {
        if ( m_IsFirstCycle )
        {
            Help();
            m_IsFirstCycle = false;
            ProcessCommand( core );
        }

        ProcessStacktrace( core );

        if ( m_Step )
        {
            bool r = false;

            DisplayCurrentLine(
                               core,
                               ( l, s ) =>
                               {
                                   r = m_LastLine != l;
                                   m_LastLine = l;

                                   return !r;
                               },
                               () => OnNotFound( core )
                              );

            ProcessCommand( core );

            return;
        }

        if ( m_BreakAddress.Contains( core.ProgramCounter ) )
        {
            m_LogMask.Info( $"Breakpoint at {GetSymbolName( core.ProgramCounter )}" );
            DisplayCurrentLine( core, ( l, s ) => false, () => OnNotFound( core ) );
            ProcessCommand( core );
        }

        bool r2 = false;

        DisplayCurrentLine(
                           core,
                           ( l, s ) =>
                           {
                               if ( m_BreakSymbols.Contains( s ) )
                               {
                                   m_LogMask.Info(
                                                  $"Breakpoint at {Path.GetFileName( s.SourceToken.FileName )}:{l} '{GetSourceExcerpt( s, out int line )}'"
                                                 );

                                   r2 = true;

                                   return false;
                               }

                               return true;
                           },
                           () => { }
                          );

        if ( r2 )
        {
            ProcessCommand( core );
        }
    }

    public void OnSectionMap( AssemblySection section, long address )
    {
        m_MappedSections.Add( section, address );

        m_LogMask.Info(
                       $"Mapped Assembly Section {section.Assembly!.Name}::{section.Name} to Address 0x{address:X16}."
                      );

        for ( int i = 0; i < m_Symbols.Count; i++ )
        {
            DebugSymbol debugSymbol = m_Symbols[i];

            if ( debugSymbol.AssemblyName == section.Assembly!.Name && debugSymbol.SectionName == section.Name )
            {
                m_MappedSymbols.Add(new DebugSymbol(debugSymbol.SourceToken, debugSymbol.AssemblyName,
                    debugSymbol.SectionName, (int) (debugSymbol.SectionOffset + address)));
            }
        }
    }

    #endregion

    #region Protected

    protected override void Initialize( string globalConfigDirectory, string presetConfigDirectory )
    {
        //Debugger has no config files to load
    }

    protected override void Load( DebuggerHost o )
    {
        o.AddDebugger( this );
    }

    #endregion

    #region Private

    private void DisplayCurrentLine( VirtualCore core, Func < int, DebugSymbol, bool > lineSkip, Action onNotFound )
    {
        bool found = false;

        foreach ( DebugSymbol symbol in m_MappedSymbols )
        {
            if ( symbol.SectionOffset == core.ProgramCounter )
            {
                string src = GetSourceExcerpt( symbol, out int line );

                if ( !lineSkip( line, symbol ) )
                {
                    found = true;
                    m_LogMask.Info( $"{symbol.SourceToken.FileName}:{line}\t '{src}'" );
                }
            }
        }

        if ( !found )
        {
            onNotFound();
        }
    }

    private string GetSourceExcerpt( DebugSymbol symbol, out int line )
    {
        string source;

        if ( !m_SourceCache.ContainsKey( symbol.SourceToken.FileName ) )
        {
            if ( !File.Exists( symbol.SourceToken.FileName ) )
            {
                line = -1;

                return symbol.SourceToken.ToString();
            }

            source = File.ReadAllText( symbol.SourceToken.FileName );
            m_SourceCache.Add( symbol.SourceToken.FileName, source );
        }
        else
        {
            source = m_SourceCache[symbol.SourceToken.FileName];
        }

        int index = symbol.SourceToken.Index;
        int start = source.LastIndexOf( '\n', index, index );

        line = source.Substring( 0, index ).Count( c => c == '\n' ) + 1;

        if ( start == -1 )
        {
            start = 0;
        }

        int end = source.IndexOf( '\n', index );

        if ( end == -1 )
        {
            end = source.Length;
        }

        return source.Substring( start, end - start ).Trim();
    }

    private string GetStackTrace( VirtualCore core )
    {
        StringBuilder sb = new StringBuilder( "\n" );
        Stack < (string sym, long addr) > stack = m_CallStacks[core];

        foreach ( (string sym, long addr) element in stack )
        {
            sb.AppendLine( $"{element.sym} at 0x{element.addr:X16}" );
        }

        return sb.ToString();
    }

    private long GetSymbolAddress( AssemblySymbol sym )
    {
        foreach ( KeyValuePair < AssemblySection, long > mappedSection in m_MappedSections )
        {
            foreach ( AssemblyElement sectionElement in mappedSection.Key.MetaData.Elements )
            {
                if ( sectionElement.Name == sym )
                {
                    return sectionElement.Offset + mappedSection.Value;
                }
            }
        }

        return -1;
    }

    private string GetSymbolName( long addr )
    {
        foreach ( KeyValuePair < AssemblySection, long > mappedSection in m_MappedSections )
        {
            foreach ( AssemblyElement sectionElement in mappedSection.Key.MetaData.Elements )
            {
                if ( sectionElement.Offset + mappedSection.Value == addr )
                {
                    return sectionElement.Name.ToString();
                }
            }
        }

        return $"0x{addr:X:16}";
    }

    private void Help()
    {
        m_LogMask.Info( "clear = Clear Console output" );
        m_LogMask.Info( "step <True|False> = Set step mode" );
        m_LogMask.Info( "halt = Stop Core" );
        m_LogMask.Info( "help = Display this help text" );
        m_LogMask.Info( "callstack = Display Callstack info" );
        m_LogMask.Info( "stack <size> <offset> = Display value from stack" );
        m_LogMask.Info( "symbols = Display Mapped Symbols" );
        m_LogMask.Info( "elements = Display Assembly Elements" );

        m_LogMask.Info( "break <address|symbol> = Set breakpoint at address or symbol" );
        m_LogMask.Info( "unbreak <address|symbol> = Unset Breakpoint" );
        m_LogMask.Info( "breakpoints = Display breakpoints" );
        m_LogMask.Info( "breakclear = Clear all breakpoints" );

        m_LogMask.Info( "linebreak <FileName:Line> = Set breakpoint at line" );
        m_LogMask.Info( "linebreakpoints = Display Line Breakpoints" );
        m_LogMask.Info( "lineunbreak <FileName:Line> = Unset breakpoint at line" );
        m_LogMask.Info( "linebreakclear = Clear all line breakpoints" );

        m_LogMask.Info( "continue or no input : Continue Execution" );
    }

    private void OnNotFound( VirtualCore core )
    {
        OpCode op = ( OpCode )core.MemoryBus.ReadUInt16( core.ProgramCounter );
        m_LogMask.Info( $"[PC 0x{core.ProgramCounter:X16}][SP 0x{core.StackPointer:X16}] {op}" );
    }

    private void ProcessCommand( VirtualCore core )
    {
        while ( !m_Run )
        {
            Console.Write( "DBG> " );
            string line = Console.ReadLine()!;

            if ( line == "" )
            {
                break;
            }

            string[] args = line.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            string cmd = args[0].ToLower(CultureInfo.InvariantCulture);
            args = args.Skip( 1 ).ToArray();

            if ( m_Commands.ContainsKey( cmd ) )
            {
                m_Commands[cmd]( core, args );
            }
            else if ( cmd is "continue" or "" )
            {
                break;
            }
            else
            {
                m_LogMask.Warning( $"Unknown command '{cmd}'" );
                Help();
            }
        }
    }

    private void ProcessStacktrace( VirtualCore core )
    {
        OpCode op = ( OpCode )core.MemoryBus.ReadUInt16( core.ProgramCounter );

        if ( !m_CallStacks.ContainsKey( core ) )
        {
            m_CallStacks.Add( core, new Stack < (string sym, long addr) >() );
        }

        if ( op == OpCode.Call )
        {
            long addr = core.MemoryBus.ReadInt64( core.StackPointer );

            bool added = false;

            foreach ( KeyValuePair < AssemblySection, long > mappedSection in m_MappedSections )
            {
                foreach ( AssemblyElement sectionElement in mappedSection.Key.MetaData.Elements )
                {
                    if ( sectionElement.Offset + mappedSection.Value == addr )
                    {
                        added = true;
                        m_CallStacks[core].Push( ( sectionElement.Name.ToString(), addr ) );
                    }
                }
            }

            if ( !added )
            {
                m_CallStacks[core].Push( ( "UNKNOWN FUNCTION", addr ) );
            }
        }
        else if ( op == OpCode.Return )
        {
            m_CallStacks[core].Pop();
        }
    }

    #endregion

}
