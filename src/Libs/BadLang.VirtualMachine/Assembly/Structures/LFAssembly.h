#pragma once
#include <string>
#include <vector>

#include "LFAssemblySection.h"

class LFAssembly
{
private:
    std::string m_Name;
    std::vector<LFAssemblySection*> m_Sections;
    std::vector<std::string> m_RequiredAssemblies;
public:
    std::string GetName() const;
    size_t GetSectionCount() const;
    LFAssemblySection* GetSection(size_t index) const;
    LFAssembly(std::string name, std::vector<LFAssemblySection*> sections, std::vector<std::string> requiredAssemblies);
    std::vector<std::string> GetRequiredAssemblies() const;
};
