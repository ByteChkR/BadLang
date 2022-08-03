#include "LFAssemblySection.h"

#include <stdexcept>
#include <utility>


std::string LFAssemblySection::GetName() const
{
    return m_Name;
}

size_t LFAssemblySection::GetDataSize() const
{
    return m_DataSize;
}

uint8_t* LFAssemblySection::GetData() const
{
    return m_Data;
}

std::vector<std::string> LFAssemblySection::GetExportNames() const
{
    return m_ExportNames;
}

std::vector<std::string> LFAssemblySection::GetImportNames() const
{
    return m_ImportNames;
}

std::vector<LFAssemblyTarget> LFAssemblySection::GetTargets() const
{
    return m_Targets;
}

LFAssemblyTarget LFAssemblySection::GetTarget(std::string name) const
{
    for (const auto& target : m_Targets)
    {
        if (target.Name == name)
        {
            return target;
        }
    }
    throw std::invalid_argument("Target not found");
}


std::map<std::string, std::vector<LFAssemblyPatchSite>> LFAssemblySection::GetPatchSites() const
{
    return m_PatchSites;
}


LFAssemblySection::LFAssemblySection(std::string name,
                                     uint8_t* data,
                                     size_t dataSize,
                                     std::vector<std::string> importNames,
                                     std::vector<std::string> exportNames,
                                     std::vector<LFAssemblyTarget> targets,
                                     std::map<std::string, std::vector<LFAssemblyPatchSite>> patchSites)
{
    m_Name = std::move(name);
    m_Data = data;
    m_DataSize = dataSize;
    m_ImportNames = std::move(importNames);
    m_ExportNames = std::move(exportNames);
    m_Targets = std::move(targets);
    m_PatchSites = std::move(patchSites);
}
