using BadVM.Shared.AssemblyFormat;

namespace BadVM.Debugger;

public static class DebuggerExtensions
{

    #region Public

    public static void Attach( this IDebugger debugger, VirtualCPU cpu, IAssemblyDomain domain )
    {
        domain.OnAssemblyLoaded += debugger.OnAssemblyLoad;
        domain.OnSectionMapped += debugger.OnSectionMap;
        cpu.OnCoreCycle += debugger.OnCoreCycle;
    }

    #endregion

}
