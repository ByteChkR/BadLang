using System.Text;

using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Code;
using LF.Compiler.DataObjects.Code.Instruction;
using LF.Compiler.Debugging;
using LF.Compiler.Logging;

namespace LF.Compiler.Reader;

public class LFReaderCodeSection
{
    private readonly List<string> m_ExportNames = new List<string>();
    private readonly List<string> m_ImportNames = new List<string>();
    private readonly List<LFAssemblyInstruction> m_Instructions = new List<LFAssemblyInstruction>();
    private readonly List<LFAssemblyLabel> m_Labels = new List<LFAssemblyLabel>();
    public readonly string Name;
    private int m_UniqueCounter;

    public LFReaderCodeSection(string name)
    {
        Name = name;
    }

    public IEnumerable<LFAssemblyLabel> Labels => m_Labels;
    public int InstructionCount => m_Instructions.Count;

    public bool IsEmpty => m_Instructions.Count == 0;

    public void RemoveInstructions(int count)
    {
        for (int i = 0; i < count; i++)
        {
            foreach (LFAssemblyLabel label in LabelsAt(m_Instructions.Count).ToArray())
            {
                m_Labels.Remove(label);
            }

            m_Instructions.RemoveAt(m_Instructions.Count - 1);
        }
    }

    public string MakeUniqueLabel(string name)
    {
        return name + m_UniqueCounter++;
    }

    public bool HasImport(string name)
    {
        return m_ImportNames.Contains(name);
    }

    public bool HasExport(string name)
    {
        return m_ExportNames.Contains(name);
    }

    public void AddImport(string name)
    {
        if (m_ImportNames.Contains(name))
        {
            return;
        }

        m_ImportNames.Add(name);
    }

    public void AddExport(string name)
    {
        if (m_ExportNames.Contains(name))
        {
            return;
        }

        m_ExportNames.Add(name);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < m_Instructions.Count; i++)
        {
            LFAssemblyInstruction instruction = m_Instructions[i];
            foreach (LFAssemblyLabel label in m_Labels)
            {
                if (label.InstructionIndex == i)
                {
                    sb.AppendLine($"{label.Name}:");
                }
            }

            sb.AppendLine($"{i}:\t{instruction}");
        }

        return sb.ToString();
    }

    public IEnumerable<LFAssemblyLabel> LabelsAt(int instrIndex)
    {
        foreach (LFAssemblyLabel label in m_Labels)
        {
            if (label.InstructionIndex == instrIndex)
            {
                yield return label;
            }
        }
    }

    public void CreateLabel(string name, LFSourcePosition sourcePosition)
    {
        m_Labels.Add(new LFAssemblyLabel(name, m_Instructions.Count, sourcePosition));
    }

    public void Emit(LFSourcePosition sourcePosition, OpCodes op, params LFAssemblyInstructionArgument[] args)
    {
        Emit(new LFAssemblyInstruction(sourcePosition, op, args));
    }

    public void Emit(LFAssemblyInstruction instruction)
    {
        if (instruction.SourcePosition != LFSourcePosition.Unknown)
        {
            Logger.Debug($"'0x{m_Instructions.Count:X}: {instruction}");
        }

        m_Instructions.Add(instruction);
    }


    private byte[] GetInstructionBytes(
        LFAssemblyDebugSymbolDatabase symbolDatabase,
        out Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>> patchSites,
        out LFAssemblyPatchTarget[] patchTargets)
    {
        patchSites =
            new Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>>();
        Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>> localPatchSites = new Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>>();
        List<LFAssemblyPatchTarget> targets = new List<LFAssemblyPatchTarget>();
        List<byte> instructionData = new List<byte>();
        if (m_Instructions.Count == 0)
        {
            foreach (LFAssemblyLabel label in LabelsAt(0))
            {
                symbolDatabase.AddSymbol(label.Position.CreateDebugSymbol(Name, 0, label.Name));
                targets.Add(new LFAssemblyPatchTarget(label.Name, instructionData.Count));
            }
        }

        for (int instrIndex = 0; instrIndex < m_Instructions.Count; instrIndex++)
        {
            LFAssemblyInstruction instruction = m_Instructions[instrIndex];
            foreach (LFAssemblyLabel label in LabelsAt(instrIndex))
            {
                symbolDatabase.AddSymbol(label.Position.CreateDebugSymbol(Name, instructionData.Count, label.Name));
                targets.Add(new LFAssemblyPatchTarget(label.Name, instructionData.Count));
            }

            symbolDatabase.AddSymbol(instruction.SourcePosition.CreateDebugSymbol(Name, instructionData.Count));
            instructionData.AddRange(BitConverter.GetBytes((ushort)instruction.OpCode));

            if (instruction.OpCode == OpCodes.Push64 && m_Instructions.Count > instrIndex + 1)
            {
                if (LFCompilerOptimizationSettings.Instance.OptimizeLocalJumps && m_Instructions[instrIndex + 1].OpCode == OpCodes.Jump)
                {
                    //Add first instruction argument to special patch sites
                    if (instruction.Arguments.Length == 1 && instruction.Arguments[0] is LFLiteralInstructionArgument literal && targets.Any(x => x.Name == literal.Value.ToString()))
                    {
                        Logger.Debug($"Found Local Jump to {literal.Value}");
                        if (!localPatchSites.ContainsKey(literal.Value))
                        {
                            localPatchSites[literal.Value] = new List<LFAssemblyPatchSite>
                                { new LFAssemblyPatchSite(instructionData.Count, 8) };
                        }
                        else
                        {
                            localPatchSites[literal.Value].Add(new LFAssemblyPatchSite(instructionData.Count, 8));
                        }
                    }
                }
                else if (LFCompilerOptimizationSettings.Instance.OptimizeLocalCalls && m_Instructions[instrIndex + 1].OpCode == OpCodes.Call)
                {
                    //Add first instruction argument to special patch sites
                    if (instruction.Arguments.Length == 1 && instruction.Arguments[0] is LFLiteralInstructionArgument literal && targets.Any(x => x.Name == literal.Value.ToString()))
                    {
                        Logger.Debug($"Found Local Call to {literal.Value}");
                        if (!localPatchSites.ContainsKey(literal.Value))
                        {
                            localPatchSites[literal.Value] = new List<LFAssemblyPatchSite>
                                { new LFAssemblyPatchSite(instructionData.Count, 8) };
                        }
                        else
                        {
                            localPatchSites[literal.Value].Add(new LFAssemblyPatchSite(instructionData.Count, 8));
                        }
                    }
                }
            }

            for (int i = 0; i < instruction.Arguments.Length; i++)
            {
                LFAssemblyInstructionArgument argument = instruction.Arguments[i];
                int size = LFAssemblyInstructionInfo.GetArgumentSize(instruction.OpCode, i);

                if (argument is LFLiteralInstructionArgument literal)
                {
                    if (!patchSites.ContainsKey(literal.Value))
                    {
                        patchSites[literal.Value] = new List<LFAssemblyPatchSite>
                            { new LFAssemblyPatchSite(instructionData.Count, size) };
                    }
                    else
                    {
                        patchSites[literal.Value].Add(new LFAssemblyPatchSite(instructionData.Count, size));
                    }
                }

                instructionData.AddRange(argument.GetValue(size));
            }
        }

        byte[] data = instructionData.ToArray();
        foreach (LFAssemblyPatchTarget target in targets)
        {
            foreach (KeyValuePair<LFAssemblySymbol, List<LFAssemblyPatchSite>> localSite in localPatchSites.ToArray())
            {
                if (target.Name == localSite.Key.ToString())
                {
                    localPatchSites.Remove(localSite.Key);
                    List<LFAssemblyPatchSite> globalSites = patchSites[localSite.Key];
                    Logger.Info("Removing patch sites for target: " + localSite.Key);
                    foreach (LFAssemblyPatchSite site in localSite.Value)
                    {
                        int nextInstrIndex = site.InstructionOffset + site.PatchSize;
                        if (nextInstrIndex >= data.Length)
                        {
                            throw new Exception($"Invalid patch site at {site.InstructionOffset}");
                        }

                        OpCodes nextOp = (OpCodes)Enum.ToObject(typeof(OpCodes), BitConverter.ToUInt16(data, nextInstrIndex));
                        if (nextOp == OpCodes.Jump && LFCompilerOptimizationSettings.Instance.OptimizeLocalJumps)
                        {
                            Logger.Debug($"Optimizing local jump at {nextInstrIndex}");
                            Array.Copy(BitConverter.GetBytes((ushort)OpCodes.JumpRel), 0, data, nextInstrIndex, sizeof(ushort));
                        }
                        else if (nextOp == OpCodes.Call && LFCompilerOptimizationSettings.Instance.OptimizeLocalCalls)
                        {
                            Logger.Debug($"Optimizing local call at {nextInstrIndex}");
                            Array.Copy(BitConverter.GetBytes((ushort)OpCodes.CallRel), 0, data, nextInstrIndex, sizeof(ushort));
                        }
                        else
                        {
                            throw new Exception("Invalid patch site at " + site.InstructionOffset);
                        }

                        Logger.Debug("Patched local jump at " + site.InstructionOffset);
                        Array.Copy(
                            BitConverter.GetBytes((ulong)(target.Offset - (site.InstructionOffset + site.PatchSize + sizeof(ushort)))),
                            0,
                            data,
                            site.InstructionOffset,
                            site.PatchSize
                        );
                        globalSites.Remove(site);
                    }

                    if (globalSites.Count == 0)
                    {
                        patchSites.Remove(localSite.Key);
                    }
                    else
                    {
                        Logger.Info("Keeping patch sites for target: " + localSite.Key);
                    }
                }
            }
        }

        if (localPatchSites.Count != 0)
        {
            throw new Exception("Could not find all local patch sites");
        }

        patchTargets = targets.ToArray();

        return data;
    }

    private byte[] GetImportBytes()
    {
        List<byte> importBytes = new List<byte>();
        importBytes.AddRange(BitConverter.GetBytes(m_ImportNames.Count));
        foreach (string importName in m_ImportNames)
        {
            byte[] importNameBytes = Encoding.UTF8.GetBytes(importName);
            importBytes.AddRange(BitConverter.GetBytes(importNameBytes.Length));
            importBytes.AddRange(importNameBytes);
        }

        return importBytes.ToArray();
    }

    private byte[] GetExportBytes()
    {
        List<byte> exportBytes = new List<byte>();
        exportBytes.AddRange(BitConverter.GetBytes(m_ExportNames.Count));
        foreach (string exportName in m_ExportNames)
        {
            byte[] exportNameBytes = Encoding.UTF8.GetBytes(exportName);
            exportBytes.AddRange(BitConverter.GetBytes(exportNameBytes.Length));
            exportBytes.AddRange(exportNameBytes);
        }

        return exportBytes.ToArray();
    }

    private static byte[] GetPatchTargetBytes(LFAssemblyPatchTarget[] targets)
    {
        List<byte> patchTargetBytes = new List<byte>();
        patchTargetBytes.AddRange(BitConverter.GetBytes(targets.Length));
        foreach (LFAssemblyPatchTarget target in targets)
        {
            byte[] targetNameBytes = Encoding.UTF8.GetBytes(target.Name);
            patchTargetBytes.AddRange(BitConverter.GetBytes(targetNameBytes.Length));
            patchTargetBytes.AddRange(targetNameBytes);
            patchTargetBytes.AddRange(BitConverter.GetBytes(target.Offset));
        }

        return patchTargetBytes.ToArray();
    }

    private static byte[] GetPatchSiteBytes(Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>> patchSites)
    {
        List<byte> patchSiteBytes = new List<byte>();
        patchSiteBytes.AddRange(BitConverter.GetBytes(patchSites.Count));
        foreach (KeyValuePair<LFAssemblySymbol, List<LFAssemblyPatchSite>> site in patchSites)
        {
            byte[] symbolBytes = Encoding.UTF8.GetBytes(site.Key.ToString());
            patchSiteBytes.AddRange(BitConverter.GetBytes(symbolBytes.Length));
            patchSiteBytes.AddRange(symbolBytes);
            patchSiteBytes.AddRange(BitConverter.GetBytes(site.Value.Count));
            foreach (LFAssemblyPatchSite patchSite in site.Value)
            {
                patchSiteBytes.AddRange(BitConverter.GetBytes(patchSite.InstructionOffset));
                patchSiteBytes.AddRange(BitConverter.GetBytes(patchSite.PatchSize));
            }
        }

        return patchSiteBytes.ToArray();
    }


    public void Serialize(Stream s, LFAssemblyDebugSymbolDatabase symbolDatabase)
    {
        byte[] instructionData =
            GetInstructionBytes(
                symbolDatabase,
                out Dictionary<LFAssemblySymbol, List<LFAssemblyPatchSite>> patchSites,
                out LFAssemblyPatchTarget[] patchTargets
            );


        byte[] patchSiteData = GetPatchSiteBytes(patchSites);
        byte[] patchTargetData = GetPatchTargetBytes(patchTargets);
        byte[] importBytes = GetImportBytes();
        byte[] exportBytes = GetExportBytes();

        byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
        s.Write(BitConverter.GetBytes(nameBytes.Length));
        s.Write(nameBytes, 0, nameBytes.Length);
        s.Write(BitConverter.GetBytes(patchTargetData.Length));
        s.Write(patchTargetData, 0, patchTargetData.Length);
        s.Write(BitConverter.GetBytes(instructionData.Length));
        s.Write(instructionData, 0, instructionData.Length);
        s.Write(BitConverter.GetBytes(patchSiteData.Length));
        s.Write(patchSiteData, 0, patchSiteData.Length);
        s.Write(BitConverter.GetBytes(importBytes.Length));
        s.Write(importBytes, 0, importBytes.Length);
        s.Write(BitConverter.GetBytes(exportBytes.Length));
        s.Write(exportBytes, 0, exportBytes.Length);
    }
}