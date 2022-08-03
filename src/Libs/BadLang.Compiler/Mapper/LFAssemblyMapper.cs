using System.Text;

using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Sections;

namespace LF.Compiler.Mapper;

public class LFAssemblyMapper
{
    private readonly Dictionary<LFAssembly, LFMappedAssembly> m_MappedAssemblies =
        new Dictionary<LFAssembly, LFMappedAssembly>();


    public string GetInformation()
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<LFAssembly, LFMappedAssembly> mappedAssembly in m_MappedAssemblies)
        {
            sb.AppendLine($"Assembly '{mappedAssembly.Key.Name}':");
            string info = $"\t{mappedAssembly.Value.GetInformation()}";
            info = string.Join("\n\t", info.Split("\n"));
            sb.AppendLine(info);
        }

        return sb.ToString();
    }

    private int AddressOf(LFAssembly asm, LFAssemblyCodeSection section)
    {
        return m_MappedAssemblies[asm].AddressOf(section);
    }

    private int AddressOf(LFAssembly asm, LFAssemblyDataSection section)
    {
        return m_MappedAssemblies[asm].AddressOf(section);
    }

    public void PatchAssemblies()
    {
        foreach (KeyValuePair<LFAssembly, LFMappedAssembly> asm in m_MappedAssemblies)
        {
            asm.Value.PatchSections(section => GetPatchTargets(asm.Key, section));
        }
    }


    public LFMappedAssembly Load(LFAssembly asm)
    {
        LFMappedAssembly mAsm = new LFMappedAssembly();
        m_MappedAssemblies.Add(asm, mAsm);

        return mAsm;
    }


    private IEnumerable<LFAssemblyPatchTarget> GetPatchTargets(LFAssembly asm, LFAssemblyCodeSection section)
    {
        foreach (LFAssemblyPatchTarget target in section.PatchTargets)
        {
            yield return new LFAssemblyPatchTarget(target.Name, AddressOf(asm, section) + target.Offset);
        }

        foreach (LFAssemblyPatchTarget target in m_MappedAssemblies[asm].GetPatchTargets(section))
        {
            yield return target;
        }

        foreach (KeyValuePair<LFAssembly, LFMappedAssembly> asmAssembly in m_MappedAssemblies.Where(
                     x => m_MappedAssemblies[asm].RequiredAssemblies.Contains(x.Key.Name)
                 ))
        {
            if (asmAssembly.Key == asm)
            {
                continue;
            }

            foreach (LFAssemblyPatchTarget target in asmAssembly.Value.GetExportedTargets(
                         asmAssembly.Key.Name,
                         section.ImportedTargets
                     ))
            {
                yield return target;
            }
        }
    }
}