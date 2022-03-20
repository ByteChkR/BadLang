using BadVM.Shared.AssemblyFormat.Serialization.Sections;
using BadVM.Shared.Memory;

namespace BadVM.Shared.AssemblyFormat
{

    public interface IAssemblyDomain
    {

        void Link( MemoryBus bus, Assembly asm, AssemblySection section, byte[] data );

        event Action < Assembly, string? >? OnAssemblyLoaded;

        event Action < AssemblySection, long >? OnSectionMapped;

    }

}
