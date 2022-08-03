#include "LFAssemblyLoader.h"

#include <cstdint>
#include <fstream>
#include <iostream>
#include <map>
#include <ostream>
#include <sstream>
#include <string>
#include <vector>

#include "../../Benchmark.h"
#include "../../Debugging/LFAssemblyDebugSymbol.h"
#include "../Structures/LFAssemblySection.h"
#include "../Structures/LFAssemblyTarget.h"

class LFAssemblySection;

std::vector<LFAssemblyTarget> LoadPatchTargets(uint8_t* patchTargetData)
{
    std::vector<LFAssemblyTarget> targets = {};
    uint32_t current = 0;
    const auto targetCount = *reinterpret_cast<uint32_t*>(patchTargetData + current);
    current += sizeof(uint32_t);
    for (size_t i = 0; i < targetCount; ++i)
    {
        const auto targetNameLength = *reinterpret_cast<uint32_t*>(patchTargetData + current);
        current += sizeof(uint32_t);
        auto targetName = std::string(reinterpret_cast<char*>(patchTargetData + current), targetNameLength);
        current += targetNameLength;
        auto targetAddr = *reinterpret_cast<uint32_t*>(patchTargetData + current);
        current += sizeof(uint32_t);
        targets.emplace_back(targetName, targetAddr);
    }
    return targets;
}

std::map<std::string, std::vector<LFAssemblyPatchSite>> LoadPatchSites(const uint8_t* patchSiteData,
                                                                       const size_t patchSiteLen)
{
    std::map<std::string, std::vector<LFAssemblyPatchSite>> r;
    size_t current = 0;
    auto siteCount = *(uint32_t*)(patchSiteData + current);
    current += sizeof(uint32_t);
    while (current < patchSiteLen)
    {
        const auto nameLen = *(uint32_t*)(patchSiteData + current);
        current += sizeof(uint32_t);
        const char* name = (char*)(patchSiteData + current);
        current += nameLen;
        const auto numSites = *(int*)(patchSiteData + current);
        current += sizeof(int);
        std::vector<LFAssemblyPatchSite> sites;
        for (int i = 0; i < numSites; i++)
        {
            LFAssemblyPatchSite site;
            site.Offset = *(uint32_t*)(patchSiteData + current);
            current += sizeof(uint32_t);
            site.Size = *(uint32_t*)(patchSiteData + current);
            current += sizeof(uint32_t);
            sites.push_back(site);
        }
        r[std::string(name, nameLen)] = sites;
    }
    return r;
}


std::vector<std::string> LoadStringList(const uint8_t* stringListData, const size_t stringListLen)
{
    std::vector<std::string> r;
    uint32_t current = 0;
    const auto stringCount = *(uint32_t*)(stringListData + current);
    current += sizeof(uint32_t);
    for (size_t i = 0; i < stringCount; i++)
    {
        const auto stringLen = *(uint32_t*)(stringListData + current);
        current += sizeof(uint32_t);
        const char* string = (char*)(stringListData + current);
        current += stringLen;
        r.emplace_back(std::string(string, stringLen));
    }
    return r;
}

LFAssemblyLoader::LFAssemblyLoader()
= default;


LFAssembly* LFAssemblyLoader::LoadAssembly(const std::string& fileName)
{
    auto loadingSection = Benchmark::StartSection("Loading " + fileName);
    std::ifstream fin(fileName, std::ios::binary);
    if (!fin.is_open())
    {
        std::cout << "Error opening file: " << fileName << std::endl;
        return nullptr;
    }

    uint32_t asmNameLen = 0;
    fin.read(reinterpret_cast<char*>(&asmNameLen), sizeof(uint32_t));
    const auto asmNamePtr = new char[asmNameLen];
    fin.read(asmNamePtr, asmNameLen);

    std::string asmName(asmNamePtr, asmNameLen);
#if _DEBUG
    std::cout << "Loading Assembly: " << asmName << std::endl;
#endif

    int requiredNamesLen = 0;
    fin.read(reinterpret_cast<char*>(&requiredNamesLen), sizeof(int));
    const auto requiredNamesData = new uint8_t[requiredNamesLen];
    fin.read(reinterpret_cast<char*>(requiredNamesData), requiredNamesLen);
    std::vector<std::string> requiredAssemblies = LoadStringList(requiredNamesData, requiredNamesLen);

#if _DEBUG
    std::cout << "Required Assemblies: " << requiredAssemblies.size() << std::endl;
#endif


    uint32_t sectionCount = 0;
    fin.read(reinterpret_cast<char*>(&sectionCount), sizeof(uint32_t));

#if _DEBUG
    std::cout << "Code Section Count: " << sectionCount << std::endl;
#endif

    std::vector<LFAssemblySection*> sections = {};
    for (size_t i = 0; i < sectionCount; ++i)
    {
        uint32_t codeSectionNameLen = 0;
        fin.read(reinterpret_cast<char*>(&codeSectionNameLen), sizeof(uint32_t));
        const auto codeSectionNamePtr = new char[codeSectionNameLen];
        fin.read(codeSectionNamePtr, codeSectionNameLen);
        std::string codeSectionName(codeSectionNamePtr, codeSectionNameLen);

#if _DEBUG
        std::cout << "Code Section: " << codeSectionName << std::endl;
#endif
        uint32_t patchTargetLen = 0;
        fin.read(reinterpret_cast<char*>(&patchTargetLen), sizeof(uint32_t));

        const auto patchTarget = new uint8_t[patchTargetLen];
        fin.read(reinterpret_cast<char*>(patchTarget), patchTargetLen);
        std::vector<LFAssemblyTarget> patchTargets = LoadPatchTargets(patchTarget);

#if _DEBUG
        std::cout << "Patch Targets: " << patchTargets.size() << std::endl;
#endif
        int codeSectionLen = 0;
        fin.read(reinterpret_cast<char*>(&codeSectionLen), sizeof(int));
        const auto codeSectionData = new uint8_t[codeSectionLen];
        fin.read(reinterpret_cast<char*>(codeSectionData), codeSectionLen);

#if _DEBUG
        std::cout << "Code Section Data: " << codeSectionLen << std::endl;
#endif
        int patchSiteLen = 0;
        fin.read(reinterpret_cast<char*>(&patchSiteLen), sizeof(int));
        const auto patchSiteData = new uint8_t[patchSiteLen];
        fin.read(reinterpret_cast<char*>(patchSiteData), patchSiteLen);

        std::map<std::string, std::vector<LFAssemblyPatchSite>> patchSites =
            LoadPatchSites(patchSiteData, patchSiteLen);

#if _DEBUG
        std::cout << "Patch Sites: " << patchSites.size() << std::endl;
#endif
        int importNamesLen = 0;
        fin.read(reinterpret_cast<char*>(&importNamesLen), sizeof(int));
        const auto importNamesData = new uint8_t[importNamesLen];
        fin.read(reinterpret_cast<char*>(importNamesData), importNamesLen);
        std::vector<std::string> importNames = LoadStringList(importNamesData, importNamesLen);

#if _DEBUG
        std::cout << "Import Names: " << importNames.size() << std::endl;
#endif
        int exportNamesLen = 0;
        fin.read(reinterpret_cast<char*>(&exportNamesLen), sizeof(int));
        const auto exportNamesData = new uint8_t[exportNamesLen];
        fin.read(reinterpret_cast<char*>(exportNamesData), exportNamesLen);
        std::vector<std::string> exportNames = LoadStringList(exportNamesData, exportNamesLen);

#if _DEBUG
        std::cout << "Export Names: " << exportNames.size() << std::endl;
#endif
        auto section = new LFAssemblySection(
            codeSectionName,
            codeSectionData,
            codeSectionLen,
            importNames,
            exportNames,
            patchTargets,
            patchSites);

        sections.push_back(section);
    }

    uint32_t dataSectionCount = 0;
    fin.read(reinterpret_cast<char*>(&dataSectionCount), sizeof(uint32_t));

#if _DEBUG
    std::cout << "Data Section Count: " << dataSectionCount << std::endl;
#endif

    for (size_t i = 0; i < dataSectionCount; ++i)
    {
        uint32_t dataSectionNameLen = 0;
        fin.read(reinterpret_cast<char*>(&dataSectionNameLen), sizeof(uint32_t));
        const auto dataSectionNamePtr = new char[dataSectionNameLen];
        fin.read(dataSectionNamePtr, dataSectionNameLen);
        std::string dataSectionName(dataSectionNamePtr, dataSectionNameLen);

#if _DEBUG
        std::cout << "Data Section: " << dataSectionName << std::endl;
#endif
        uint32_t patchTargetLen = 0;
        fin.read(reinterpret_cast<char*>(&patchTargetLen), sizeof(uint32_t));

        const auto patchTarget = new uint8_t[patchTargetLen];
        fin.read(reinterpret_cast<char*>(patchTarget), patchTargetLen);
        std::vector<LFAssemblyTarget> patchTargets = LoadPatchTargets(patchTarget);

#if _DEBUG
        std::cout << "Patch Targets: " << patchTargets.size() << std::endl;
#endif
        int dataSectionLen = 0;
        fin.read(reinterpret_cast<char*>(&dataSectionLen), sizeof(int));
        const auto dataSectionData = new uint8_t[dataSectionLen];
        fin.read(reinterpret_cast<char*>(dataSectionData), dataSectionLen);

#if _DEBUG
        std::cout << "Data Section Data: " << dataSectionLen << std::endl;
#endif

        std::vector<std::string> exportNames = {};
        exportNames.reserve(patchTargets.size());
        for (const auto& target : patchTargets)
        {
            exportNames.push_back(target.Name);
        }
        sections.push_back(new LFAssemblySection(dataSectionName, dataSectionData, dataSectionLen, {}, exportNames,
                                                 patchTargets, {}));
    }

    const auto assembly = new LFAssembly(asmName, sections, requiredAssemblies);

    Benchmark::EndSection(loadingSection);

    return assembly;
}

std::vector<LFAssemblyDebugSymbol*> LFAssemblyLoader::LoadSymbols(const std::string& filename)
{
    const auto fileMap = new std::map<std::string, std::string>();
    std::ifstream fin(filename, std::ios::binary);
    if (!fin.is_open())
    {
        throw std::runtime_error("Failed to open file: " + filename);
    }

    std::vector<LFAssemblyDebugSymbol*> symbols = {};

    std::string line;
    while (std::getline(fin, line))
    {
        const size_t sectionIndex = line.find('|');
        if (sectionIndex == std::string::npos)
        {
            throw std::runtime_error("Invalid symbol line: " + line);
        }
        const std::string sectionName = line.substr(0, sectionIndex);
        const size_t symbolIndex = line.find('|', sectionIndex + 1);
        if (symbolIndex == std::string::npos)
        {
            throw std::runtime_error("Invalid symbol line: " + line);
        }
        const std::string symbolName = line.substr(sectionIndex + 1, symbolIndex - sectionIndex - 1);

        const size_t sectionOffsetIndex = line.find('|', symbolIndex + 1);
        if (sectionOffsetIndex == std::string::npos)
        {
            throw std::runtime_error("Invalid symbol line: " + line);
        }

        const int sectionOffset = std::stoi(line.substr(symbolIndex + 1, sectionOffsetIndex - symbolIndex - 1));

        const size_t fileNameIndex = line.find('|', sectionOffsetIndex + 1);
        if (fileNameIndex == std::string::npos)
        {
            throw std::runtime_error("Invalid symbol line: " + line);
        }
        const std::string fileName = line.substr(sectionOffsetIndex + 1, fileNameIndex - sectionOffsetIndex - 1);

        const size_t startIndex = line.find('|', fileNameIndex + 1);
        if (startIndex == std::string::npos)
        {
            throw std::runtime_error("Invalid symbol line: " + line);
        }
        const int start = std::stoi(line.substr(fileNameIndex + 1, startIndex - fileNameIndex - 1));


        const int end = std::stoi(line.substr(startIndex + 1, line.size() - 1));

        symbols.emplace_back(
            new LFAssemblyDebugSymbol(fileName, start, end, sectionName, symbolName, sectionOffset, fileMap));
    }

    delete fileMap;
    return symbols;
}
