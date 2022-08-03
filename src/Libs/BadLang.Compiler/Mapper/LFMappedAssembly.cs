using System.Text;

using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Sections;

namespace LF.Compiler.Mapper;

public class LFMappedAssembly
{
    private readonly Dictionary<LFAssemblyDataSection, int> m_DataSections =
        new Dictionary<LFAssemblyDataSection, int>();

    private readonly List<string> m_RequiredAssemblies = new List<string>();

    private readonly Dictionary<LFAssemblyCodeSection, int> m_Sections = new Dictionary<LFAssemblyCodeSection, int>();

    public IEnumerable<string> RequiredAssemblies => m_RequiredAssemblies;

    public string GetInformation()
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<LFAssemblyCodeSection, int> section in m_Sections)
        {
            sb.AppendLine($"Code Section '{section.Key.Name}' at 0x{section.Value:X}:");
            string info = $"\t{section.Key.GetInformation(section.Value)}";
            info = string.Join("\n\t", info.Split('\n'));
            sb.AppendLine(info);
        }

        foreach (KeyValuePair<LFAssemblyDataSection, int> section in m_DataSections)
        {
            sb.AppendLine($"Data Section '{section.Key.Name}' at 0x{section.Value:X}:");
            string info = $"\t{section.Key.GetInformation(section.Value)}";
            info = string.Join("\n\t", info.Split('\n'));
            sb.AppendLine(info);
        }


        return sb.ToString();
    }

    public void AddRequiredAssembly(string name)
    {
        m_RequiredAssemblies.Add(name);
    }

    public void Load(int addr, LFAssemblyCodeSection section)
    {
        m_Sections[section] = addr;
    }

    public void Load(int addr, LFAssemblyDataSection section)
    {
        m_DataSections[section] = addr;
    }

    public int AddressOf(LFAssemblyCodeSection section)
    {
        return m_Sections[section];
    }

    public int AddressOf(LFAssemblyDataSection section)
    {
        return m_DataSections[section];
    }

    private void PatchSection(LFAssemblyCodeSection section, LFAssemblyPatchTarget[] targets)
    {
        foreach (LFAssemblySymbol symbol in section.PatchSiteSymbols)
        {
            LFAssemblyPatchTarget? target = targets.FirstOrDefault(x => x.Name == symbol.ToString());
            if (target == null)
            {
                throw new Exception($"Could not find patch target for symbol {symbol} in section {section.Name}");
            }
        }
    }

    public void PatchSections(Func<LFAssemblyCodeSection, IEnumerable<LFAssemblyPatchTarget>> targetProvider)
    {
        foreach (KeyValuePair<LFAssemblyCodeSection, int> section in m_Sections)
        {
            PatchSection(section.Key, targetProvider(section.Key).ToArray());
        }
    }

    public IEnumerable<LFAssemblyPatchTarget> GetExportedTargets(string name, IEnumerable<string> imports)
    {
        foreach (KeyValuePair<LFAssemblyCodeSection, int> sections in m_Sections)
        {
            if (!imports.Contains(sections.Key.Name))
            {
                continue;
            }

            foreach (LFAssemblyPatchTarget target in sections.Key.ExportedTargets)
            {
                yield return new LFAssemblyPatchTarget(
                    $"{name}::{sections.Key.Name}::{target.Name}",
                    sections.Value + target.Offset
                );
            }
        }

        foreach (KeyValuePair<LFAssemblyDataSection, int> sections in m_DataSections)
        {
            if (!imports.Contains(sections.Key.Name))
            {
                continue;
            }

            foreach (LFAssemblyPatchTarget target in sections.Key.PatchTargets)
            {
                yield return new LFAssemblyPatchTarget(
                    $"{name}::{sections.Key.Name}::{target.Name}",
                    sections.Value + target.Offset
                );
            }
        }
    }

    public IEnumerable<LFAssemblyPatchTarget> GetPatchTargets(LFAssemblyCodeSection section)
    {
        foreach (KeyValuePair<LFAssemblyCodeSection, int> sections in m_Sections)
        {
            if (sections.Key == section || !section.ImportedTargets.Contains(sections.Key.Name))
            {
                continue;
            }

            foreach (LFAssemblyPatchTarget target in sections.Key.ExportedTargets)
            {
                yield return new LFAssemblyPatchTarget(
                    $"{sections.Key.Name}::{target.Name}",
                    sections.Value + target.Offset
                );
            }
        }

        foreach (KeyValuePair<LFAssemblyDataSection, int> sections in m_DataSections)
        {
            if (!section.ImportedTargets.Contains(sections.Key.Name))
            {
                continue;
            }

            foreach (LFAssemblyPatchTarget target in sections.Key.PatchTargets)
            {
                yield return new LFAssemblyPatchTarget(
                    $"{sections.Key.Name}::{target.Name}",
                    sections.Value + target.Offset
                );
            }
        }
    }
}