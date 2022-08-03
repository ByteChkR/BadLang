using System.Text;

using LF.Compiler.DataObjects.Code;
using LF.Compiler.DataObjects.Code.Instruction;

namespace LF.Compiler.DataObjects.Sections;

public class LFAssemblyCodeSection
{
    private readonly string[] m_ExportNames;
    private readonly string[] m_ImportNames;
    private readonly byte[] m_InstructionData;
    private readonly Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]> m_PatchSites;
    private readonly LFAssemblyPatchTarget[] m_PatchTargets;
    public readonly string Name;

    public LFAssemblyCodeSection(
        string name,
        byte[] instructionData,
        Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]> patchSites,
        LFAssemblyPatchTarget[] patchTargets,
        string[] importNames,
        string[] exportNames)
    {
        Name = name;
        m_InstructionData = instructionData;
        m_PatchSites = patchSites;
        m_PatchTargets = patchTargets;
        m_ImportNames = importNames;
        m_ExportNames = exportNames;
    }

    public int InstructionSize => m_InstructionData.Length;

    public IEnumerable<LFAssemblySymbol> PatchSiteSymbols => m_PatchSites.Keys;

    public IEnumerable<LFAssemblyPatchTarget> PatchTargets => m_PatchTargets;

    public IEnumerable<LFAssemblyPatchTarget> ExportedTargets =>
        m_PatchTargets.Where(x => m_ExportNames.Contains(x.Name));

    public IEnumerable<string> ImportedTargets => m_ImportNames;


    public string GetInformation(int addr)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string export in m_ExportNames)
        {
            sb.AppendLine($".export {export}");
        }

        foreach (string import in m_ImportNames)
        {
            sb.AppendLine($".import {import}");
        }

        int current = 0;
        while (current < m_InstructionData.Length)
        {
            LFAssemblyPatchTarget? target = m_PatchTargets.FirstOrDefault(x => x.Offset == current);
            if (target != null)
            {
                sb.AppendLine($"{target.Name}:");
            }

            OpCodes op = (OpCodes)BitConverter.ToUInt16(m_InstructionData, current);
            int opAddr = current;
            current += sizeof(ushort);
            int argC = LFAssemblyInstructionInfo.GetArgumentCount(op);
            List<string> argInfo = new List<string>();
            for (int i = 0; i < argC; i++)
            {
                int argSize = LFAssemblyInstructionInfo.GetArgumentSize(op, i);
                ulong value = 0;
                string? strValue = null;
                switch (argSize)
                {
                    case 1:
                        value = m_InstructionData[current];

                        break;
                    case 2:
                        value = BitConverter.ToUInt16(m_InstructionData, current);

                        break;

                    case 4:
                        value = BitConverter.ToUInt32(m_InstructionData, current);

                        break;

                    case 8:
                        value = BitConverter.ToUInt64(m_InstructionData, current);

                        break;
                    default:
                        throw new Exception($"Invalid Instruction Argument Size {argSize} for {op}");
                }

                IEnumerable<LFAssemblySymbol> sym = m_PatchSites
                    .Where(x => x.Value.Any(y => y.InstructionOffset == current))
                    .Select(x => x.Key);
                foreach (LFAssemblySymbol symbol in sym)
                {
                    if (strValue != null)
                    {
                        throw new Exception("Invalid Symbol Information");
                    }

                    strValue = symbol.ToString();
                }

                strValue ??= $"0x{value:X}";

                argInfo.Add(strValue);

                current += argSize;
            }

            sb.AppendLine($"\t[0x{addr + opAddr:X}] {op} {string.Join(' ', argInfo)}");
        }

        return sb.ToString();
    }

    public IEnumerable<LFAssemblyPatchSite> GetPatchSites(LFAssemblySymbol sym)
    {
        return m_PatchSites[sym];
    }
}