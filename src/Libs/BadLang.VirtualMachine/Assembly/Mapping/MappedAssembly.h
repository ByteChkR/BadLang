#pragma once
#include <string>

#include "AssemblyMapper.h"
#include "../Structures/LFAssembly.h"

class AssemblyMapper;

class MappedAssembly
{
    LFAssembly* m_BaseAssembly;
    std::map<LFAssemblySection*, uint8_t*> m_MappedSections;
    std::vector<std::string> m_RequiredAssemblies;
public:
    std::string GetName() const;
    LFAssembly* GetBaseAssembly() const;
    uint8_t* GetMappedSection(LFAssemblySection* section);
    uint8_t* GetMappedSection(const std::string& name) const;
    std::vector<std::string> GetRequiredAssemblies() const;
    std::vector<LFAssemblyTarget> GetExportedTargets(const std::vector<std::string>& sectionNames) const;
    void PatchSections(AssemblyMapper* mapper);
    MappedAssembly(LFAssembly* assembly, std::map<LFAssemblySection*, uint8_t*> mappedSections,
                   std::vector<std::string> requiredAssemblies);
};
