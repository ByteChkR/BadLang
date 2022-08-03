using System.Text;

using LF.Compiler.DataObjects;

namespace LF.Compiler.Reader;

public class LFReaderDataSection
{
    private static int s_UniqueCounter;
    private readonly List<byte> m_Data = new List<byte>();
    private readonly List<LFAssemblyPatchTarget> m_PatchTargets = new List<LFAssemblyPatchTarget>();
    public readonly string Name;

    public LFReaderDataSection(string name)
    {
        Name = name;
    }

    public bool IsEmpty => m_Data.Count == 0;

    public string MakeUniqueName(string name)
    {
        return $"{name}__{s_UniqueCounter++}";
    }

    public void EmitData(string name, byte[]? data = null)
    {
        m_PatchTargets.Add(new LFAssemblyPatchTarget(name, m_Data.Count));
        if (data != null)
        {
            m_Data.AddRange(data);
        }
    }

    private byte[] SerializePatchTargets()
    {
        List<byte> result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(m_PatchTargets.Count));
        foreach (LFAssemblyPatchTarget label in m_PatchTargets)
        {
            byte[] labelNameBytes = Encoding.UTF8.GetBytes(label.Name);
            result.AddRange(BitConverter.GetBytes(labelNameBytes.Length));
            result.AddRange(labelNameBytes);
            result.AddRange(BitConverter.GetBytes(label.Offset));
        }

        return result.ToArray();
    }

    public void Serialize(Stream s)
    {
        byte[] sectionNameBytes = Encoding.UTF8.GetBytes(Name);
        s.Write(BitConverter.GetBytes(sectionNameBytes.Length));
        s.Write(sectionNameBytes, 0, sectionNameBytes.Length);

        byte[] patchTargetBytes = SerializePatchTargets();
        s.Write(BitConverter.GetBytes(patchTargetBytes.Length));
        s.Write(patchTargetBytes, 0, patchTargetBytes.Length);

        s.Write(BitConverter.GetBytes(m_Data.Count));
        s.Write(m_Data.ToArray());
    }
}