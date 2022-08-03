#include "AssemblyMapper.h"

#include <fstream>
#include <iostream>
#include <ostream>
#include <stdexcept>

#include "../Loader/LFAssemblyLoader.h"
#include "../../Debugging/LFAssemblyDebugSymbol.h"
#include "../../Benchmark.h"


//Create pointers for each assembly section
//  Patch Assembly Code by looking up the local targets and the exported targets of the imported sections
//Create a MappedAssembly class that maps the sections to their pointers


AssemblyMapper::AssemblyMapper()
{
    m_MappedAssemblies = {};
    m_MappedSymbols = {};
}


std::vector<LFAssemblyTarget> AssemblyMapper::CreateTargets(MappedAssembly* assembly, LFAssemblySection* section) const
{
    std::vector<LFAssemblyTarget> targets = {};
    const auto off = reinterpret_cast<uint64_t>(assembly->GetMappedSection(section));
    for (const auto& target : section->GetTargets())
    {
        targets.emplace_back(target.Name, off + target.Offset);
    }

    const size_t sectionCount = assembly->GetBaseAssembly()->GetSectionCount();
    for (size_t i = 0; i < sectionCount; i++)
    {
        LFAssemblySection* sec = assembly->GetBaseAssembly()->GetSection(i);
        if (section != sec)
        {
            for (const auto& import : section->GetImportNames())
            {
                if (sec->GetName() == import)
                {
                    const auto offset = reinterpret_cast<uint64_t>(assembly->GetMappedSection(sec));
                    for (const auto& export_name : sec->GetExportNames())
                    {
                        const LFAssemblyTarget target = sec->GetTarget(export_name);
                        targets.emplace_back(sec->GetName() + "::" + export_name, offset + target.Offset);
                    }
                }
            }
        }
    }

    for (const auto mappedAssembly : m_MappedAssemblies)
    {
        for (const auto& asmName : assembly->GetRequiredAssemblies())
        {
            if (mappedAssembly->GetName() == asmName)
            {
                for (const auto& target : mappedAssembly->GetExportedTargets(section->GetImportNames()))
                {
                    targets.push_back(target);
                }
            }
        }
    }

    return targets;
}


void AssemblyMapper::PatchAssemblies()
{
    for (const auto assembly : m_MappedAssemblies)
    {
        assembly->PatchSections(this);
    }
}

LFAssemblyDebugSymbolServer* AssemblyMapper::GetSymbolServer() const
{
    return new LFAssemblyDebugSymbolServer(m_MappedSymbols);
}


MappedAssembly* AssemblyMapper::Map(const std::string& fileName, const std::string& workingDir, LFAssembly* assembly,
                                    bool noSymbols)
{
    std::map<LFAssemblySection*, uint8_t*> map = {};
    const size_t sectionCount = assembly->GetSectionCount();
    std::string symbolPath = fileName + ".sym";
    std::ifstream f(symbolPath.c_str());
    std::vector<LFAssemblyDebugSymbol*> symbols = {};
    if (f.good() && !noSymbols)
    {
        std::cout << "Loading symbols from " << symbolPath << std::endl;
        auto section = Benchmark::StartSection("Loading Symbols " + symbolPath);
        symbols = LFAssemblyLoader::LoadSymbols(symbolPath);
        Benchmark::EndSection(section);
#if _DEBUG
        std::cout << "Loaded " << symbols.size() << " Symbols." << std::endl;
#endif
    }

    auto mappingSection = Benchmark::StartSection("Mapping " + fileName);
    for (size_t i = 0; i < sectionCount; i++)
    {
        LFAssemblySection* section = assembly->GetSection(i);

        const auto sectionData = new uint8_t[section->GetDataSize()];
        memcpy(sectionData, section->GetData(), section->GetDataSize());

        map[section] = sectionData;


        for (auto symbol : symbols)
        {
            if (symbol->SectionName == section->GetName())
            {
#if _DEBUG
                if (!symbol->SymbolName.empty())
                {
                    std::cout << "Patching symbol '" << symbol->SymbolName << "' for section " << symbol->SectionName <<
                        std::endl;
                }
#endif
                symbol->SectionOffset = reinterpret_cast<uint64_t>(sectionData) + symbol->SectionOffset;
            }
        }

#if _DEBUG
        std::cout << "Mapped section " << section->GetName() << " to " << reinterpret_cast<uint64_t>(sectionData) <<
            std::endl;
#endif
    }
    Benchmark::EndSection(mappingSection);

    //Get Required Assemblies
    std::vector<std::string> requiredAssemblies = assembly->GetRequiredAssemblies();
    std::vector<std::string> requiredAssemblyNames = {};
    for (const auto& require : requiredAssemblies)
    {
        std::string filePath;
        if (require.rfind("file://", 0) == 0)
        {
            std::string fileName = require.substr(7);

            if (fileName.rfind("/", 0) == 0 ||
                fileName.rfind(":\\", 1) == 1)
            {
                filePath = fileName;
            }
            else
            {
                filePath = workingDir + "/" + fileName;
            }
        }
        else if (require.rfind("name://", 0) == 0)
        {
            std::string assemblyName = require.substr(7);
            filePath = workingDir + "/" + assemblyName + ".lfbin";
        }
        else
        {
            throw std::invalid_argument("Invalid required assembly: " + require);
        }

        std::string directory;
        size_t last_bslash_idx = filePath.rfind('\\');
        size_t last_fslash_idx = filePath.rfind('/');
        size_t last_slash_idx = last_bslash_idx;
        if (last_slash_idx == std::string::npos || (last_fslash_idx != std::string::npos && last_slash_idx <
            last_fslash_idx))
        {
            last_slash_idx = last_fslash_idx;
        }

        if (std::string::npos != last_slash_idx)
        {
            directory = filePath.substr(0, last_slash_idx);
        }
        else
        {
            throw std::invalid_argument("Invalid required assembly: " + require);
        }

        LFAssembly* requiredAssembly = LFAssemblyLoader::LoadAssembly(filePath);
        requiredAssemblyNames.push_back(requiredAssembly->GetName());
        Map(filePath, directory, requiredAssembly, noSymbols);
    }
    const auto mAsm = new MappedAssembly(assembly, map, requiredAssemblyNames);


    m_MappedSymbols.emplace(mAsm, symbols);
    m_MappedAssemblies.push_back(mAsm);

    return mAsm;
}
