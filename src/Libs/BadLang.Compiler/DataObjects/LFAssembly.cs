using System.Text;

using LF.Compiler.DataObjects.Code;
using LF.Compiler.DataObjects.Sections;
using LF.Compiler.Mapper;

namespace LF.Compiler.DataObjects;

public class LFAssembly
{
    private readonly LFAssemblyCodeSection[] m_CodeSections;
    private readonly LFAssemblyDataSection[] m_DataSections;
    private readonly string[] m_Requires;
    public readonly string Name;

    public LFAssembly(
        string name,
        LFAssemblyCodeSection[] codeSections,
        LFAssemblyDataSection[] dataSections,
        string[] requires)
    {
        Name = name;
        m_CodeSections = codeSections;
        m_DataSections = dataSections;
        m_Requires = requires;
    }

    public IEnumerable<string> RequiredAssemblies => m_Requires;

    private static Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]> LoadPatchSites(byte[] data)
    {
        int current = 0;
        int codeSectionPatchSiteCount = BitConverter.ToInt32(data, current);
        current += sizeof(int);
        Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]> codeSectionPatchSites =
            new Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]>();
        for (int j = 0; j < codeSectionPatchSiteCount; j++)
        {
            int codeSectionPatchSiteSymbolLen = BitConverter.ToInt32(data, current);
            current += sizeof(int);
            string codeSectionPatchSiteSymbol = Encoding.UTF8.GetString(data, current, codeSectionPatchSiteSymbolLen);
            current += codeSectionPatchSiteSymbolLen;
            int patchElementCount = BitConverter.ToInt32(data, current);
            current += sizeof(int);
            LFAssemblyPatchSite[] patchElements = new LFAssemblyPatchSite[patchElementCount];
            for (int k = 0; k < patchElementCount; k++)
            {
                int patchElementOffset = BitConverter.ToInt32(data, current);
                current += sizeof(int);
                int patchElementSize = BitConverter.ToInt32(data, current);
                current += sizeof(int);
                patchElements[k] = new LFAssemblyPatchSite(
                    patchElementOffset,
                    patchElementSize
                );
            }

            codeSectionPatchSites[codeSectionPatchSiteSymbol] = patchElements;
        }

        return codeSectionPatchSites;
    }

    private static LFAssemblyPatchTarget[] LoadTargets(byte[] data)
    {
        int current = 0;

        int codeSectionPatchTargetCount = BitConverter.ToInt32(data, current);
        current += sizeof(int);
        LFAssemblyPatchTarget[] codeSectionPatchTargets = new LFAssemblyPatchTarget[codeSectionPatchTargetCount];
        for (int j = 0; j < codeSectionPatchTargetCount; j++)
        {
            int codeSectionPatchTargetNameLen = BitConverter.ToInt32(data, current);
            current += sizeof(int);
            string codeSectionPatchTargetName = Encoding.UTF8.GetString(
                data,
                current,
                codeSectionPatchTargetNameLen
            );
            current += codeSectionPatchTargetNameLen;

            int codeSectionPatchTargetOffset = BitConverter.ToInt32(data, current);
            current += sizeof(int);
            codeSectionPatchTargets[j] = new LFAssemblyPatchTarget(
                codeSectionPatchTargetName,
                codeSectionPatchTargetOffset
            );
        }

        return codeSectionPatchTargets;
    }


    private static string[] LoadStringList(byte[] data)
    {
        int current = 0;
        int elemLen = BitConverter.ToInt32(data, current);
        current += sizeof(int);
        string[] elements = new string[elemLen];
        for (int j = 0; j < elemLen; j++)
        {
            int elementLen = BitConverter.ToInt32(data, current);
            current += sizeof(int);
            elements[j] = Encoding.UTF8.GetString(data, current, elementLen);
            current += elementLen;
        }

        return elements;
    }

    public static LFAssembly FromFile(string filePath)
    {
        FileInfo file = new FileInfo(filePath);
        if (!file.Exists)
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        Stream s = file.OpenRead();

        int nameLen = s.ReadInt32();
        byte[] nameBytes = new byte[nameLen];
        s.Read(nameBytes, 0, nameLen);
        string name = Encoding.UTF8.GetString(nameBytes);


        int requiresBytesLen = s.ReadInt32();
        byte[] requiresBytes = new byte[requiresBytesLen];
        s.Read(requiresBytes, 0, requiresBytesLen);
        string[] requires = LoadStringList(requiresBytes);


        int codeSectionCount = s.ReadInt32();
        LFAssemblyCodeSection[] codeSections = new LFAssemblyCodeSection[codeSectionCount];
        for (int i = 0; i < codeSectionCount; i++)
        {
            int codeSectionNameLen = s.ReadInt32();
            byte[] codeSectionNameBytes = new byte[codeSectionNameLen];
            s.Read(codeSectionNameBytes, 0, codeSectionNameLen);
            string codeSectionName = Encoding.UTF8.GetString(codeSectionNameBytes);

            int patchTargetBytesLen = s.ReadInt32();
            byte[] patchTargetBytes = new byte[patchTargetBytesLen];
            s.Read(patchTargetBytes, 0, patchTargetBytesLen);

            LFAssemblyPatchTarget[] patchTargets = LoadTargets(patchTargetBytes);

            int codeSectionInstructionDataLen = s.ReadInt32();
            byte[] codeSectionInstructionData = new byte[codeSectionInstructionDataLen];
            s.Read(codeSectionInstructionData, 0, codeSectionInstructionDataLen);

            int patchSiteBytesLen = s.ReadInt32();
            byte[] patchSiteBytes = new byte[patchSiteBytesLen];
            s.Read(patchSiteBytes, 0, patchSiteBytesLen);
            Dictionary<LFAssemblySymbol, LFAssemblyPatchSite[]> patchSites = LoadPatchSites(patchSiteBytes);

            int importBytesLen = s.ReadInt32();
            byte[] importBytes = new byte[importBytesLen];
            s.Read(importBytes, 0, importBytesLen);
            string[] importNames = LoadStringList(importBytes);

            int exportBytesLen = s.ReadInt32();
            byte[] exportBytes = new byte[exportBytesLen];
            s.Read(exportBytes, 0, exportBytesLen);

            string[] exportNames = LoadStringList(exportBytes);

            codeSections[i] = new LFAssemblyCodeSection(
                codeSectionName,
                codeSectionInstructionData,
                patchSites,
                patchTargets,
                importNames,
                exportNames
            );
        }

        int dataSectionCount = s.ReadInt32();
        LFAssemblyDataSection[] dataSections = new LFAssemblyDataSection[dataSectionCount];
        for (int i = 0; i < dataSectionCount; i++)
        {
            int dataSectionNameLen = s.ReadInt32();
            byte[] dataSectionNameBytes = new byte[dataSectionNameLen];
            s.Read(dataSectionNameBytes, 0, dataSectionNameLen);
            string dataSectionName = Encoding.UTF8.GetString(dataSectionNameBytes);

            int patchTargetBytesLen = s.ReadInt32();
            byte[] patchTargetBytes = new byte[patchTargetBytesLen];
            s.Read(patchTargetBytes, 0, patchTargetBytesLen);

            LFAssemblyPatchTarget[] patchTargets = LoadTargets(patchTargetBytes);

            int dataLen = s.ReadInt32();
            byte[] data = new byte[dataLen];
            s.Read(data, 0, dataLen);
            dataSections[i] = new LFAssemblyDataSection(dataSectionName, patchTargets, data);
        }

        s.Close();

        return new LFAssembly(name, codeSections, dataSections, requires);
    }


    public int Initialize(int start, string workingDir, LFAssemblyMapper mapper)
    {
        LFMappedAssembly asm = mapper.Load(this);
        foreach (LFAssemblyCodeSection codeSection in m_CodeSections)
        {
            asm.Load(start, codeSection);
            start += codeSection.InstructionSize;
        }

        foreach (LFAssemblyDataSection dataSection in m_DataSections)
        {
            asm.Load(start, dataSection);
            start += dataSection.DataSize;
        }

        foreach (string require in m_Requires)
        {
            if (require.StartsWith("file://"))
            {
                string file = Path.Combine(workingDir, require.Substring("file://".Length));
                LFAssembly assembly = FromFile(file);
                asm.AddRequiredAssembly(assembly.Name);
                start = assembly.Initialize(start, Path.GetDirectoryName(Path.GetFullPath(file))!, mapper);
            }
            else if (require.StartsWith("name://"))
            {
                string name = require.Substring("name://".Length);
                string file = Path.Combine(workingDir, $"{name}.lfbin");
                LFAssembly assembly = FromFile(file);
                asm.AddRequiredAssembly(assembly.Name);
                start = assembly.Initialize(start, Path.GetDirectoryName(Path.GetFullPath(file))!, mapper);
            }
            else
            {
                throw new Exception($"Invalid require: '{require}'");
            }
        }


        return start;
    }
}