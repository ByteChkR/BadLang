using BadVM.Core;
using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.Logging;
using BadVM.Shared.Memory;
using BadVM.Shared.PluginSystem;

namespace BadVM.Debugger.BASM;

public class Debugger : Plugin < DebuggerHost >, IDebugger
{

    private readonly LogMask m_LogMask;

    #region Public

    public Debugger() : base( "basm_debugger", "BASM_Debugger" )
    {
        m_LogMask = new LogMask( FriendlyName );
    }

    public void OnAssemblyLoad( Assembly asm, string? pathHint )
    {
        m_LogMask.Info( $"Loaded Assembly {asm.Name} from {pathHint ?? "null"}." );
    }

    public void OnCoreCycle( VirtualCore core )
    {
        OpCode op = ( OpCode )core.MemoryBus.ReadUInt16( core.ProgramCounter );
        m_LogMask.Info( $"[PC 0x{core.ProgramCounter:X16}][SP 0x{core.StackPointer:X16}] {op}" );
        m_LogMask.Info( "Press enter to continue." );
        Console.ReadLine();
    }

    public void OnSectionMap( AssemblySection section, long address )
    {
        m_LogMask.Info(
                       $"Mapped Assembly Section {section.Assembly!.Name}::{section.Name} to Address 0x{address:X16}."
                      );
    }

    #endregion

    #region Protected

    protected override void Initialize( string globalConfigDirectory, string presetConfigDirectory )
    {
    }

    protected override void Load( DebuggerHost o )
    {
        o.AddDebugger( this );
    }

    #endregion

}
