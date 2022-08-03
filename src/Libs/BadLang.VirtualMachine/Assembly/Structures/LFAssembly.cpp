#include "LFAssembly.h"

#include <utility>


std::string LFAssembly::GetName() const
{
    return m_Name;
}

size_t LFAssembly::GetSectionCount() const
{
    return m_Sections.size();
}

LFAssemblySection* LFAssembly::GetSection(size_t index) const
{
    return m_Sections[index];
}

LFAssembly::LFAssembly(std::string name, std::vector<LFAssemblySection*> sections,
                       std::vector<std::string> requiredAssemblies)
{
    m_Name = std::move(name);
    m_Sections = std::move(sections);
    m_RequiredAssemblies = std::move(requiredAssemblies);
}


std::vector<std::string> LFAssembly::GetRequiredAssemblies() const
{
    return m_RequiredAssemblies;
}
