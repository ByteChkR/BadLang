using System.Text;

namespace LF.Compiler.DataObjects.Sections;

public class LFAssemblyDataSection
{
    private readonly byte[] m_Data;
    private readonly LFAssemblyPatchTarget[] m_PatchTargets;
    public readonly string Name;

    public LFAssemblyDataSection(string name, LFAssemblyPatchTarget[] patchTargets, byte[] data)
    {
        Name = name;
        m_PatchTargets = patchTargets;
        m_Data = data;
    }

    public int DataSize => m_Data.Length;

    public IEnumerable<LFAssemblyPatchTarget> PatchTargets => m_PatchTargets;

    public string GetInformation(int addr)
    {
        StringBuilder sb = new StringBuilder();

        int GetSize(LFAssemblyPatchTarget target)
        {
            int min = m_Data.Length - target.Offset;
            for (int i = 0; i < m_PatchTargets.Length; i++)
            {
                int delta = m_PatchTargets[i].Offset - target.Offset;
                if (delta > 0 && min > delta)
                {
                    min = delta;
                }
            }

            return min;
        }

        foreach (LFAssemblyPatchTarget target in m_PatchTargets)
        {
            sb.AppendLine($"[0x{addr + target.Offset:X}] {target.Name} with size {GetSize(target)}");
        }

        return sb.ToString();
    }
}