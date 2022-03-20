using BadVM.Core;
using BadVM.Shared.AssemblyFormat;
using BadVM.Shared.AssemblyFormat.Serialization.Sections;

namespace BadVM.Debugger;

public interface IDebugger
{

    string Name { get; }

    void OnAssemblyLoad( Assembly asm, string? pathHint );

    void OnCoreCycle( VirtualCore core );

    void OnSectionMap( AssemblySection section, long address );

}
