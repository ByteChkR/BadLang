#pragma once
#include <map>
#include <string>
#include <vector>

#include "LFAssemblyPatchSite.h"
#include "LFAssemblyTarget.h"

class LFAssemblySection
{
private:
    std::string m_Name;
    std::vector<std::string> m_ImportNames;
    std::vector<std::string> m_ExportNames;
    std::vector<LFAssemblyTarget> m_Targets;
    std::map<std::string, std::vector<LFAssemblyPatchSite>> m_PatchSites;
    uint8_t* m_Data;
    size_t m_DataSize;

public:
    std::string GetName() const;
    size_t GetDataSize() const;
    uint8_t* GetData() const;
    std::vector<LFAssemblyTarget> GetTargets() const;
    LFAssemblyTarget GetTarget(std::string name) const;
    std::vector<std::string> GetImportNames() const;
    std::vector<std::string> GetExportNames() const;
    std::map<std::string, std::vector<LFAssemblyPatchSite>> GetPatchSites() const;
    LFAssemblySection(std::string name,
                      uint8_t* data,
                      size_t dataSize,
                      std::vector<std::string> importNames,
                      std::vector<std::string> exportNames,
                      std::vector<LFAssemblyTarget> targets,
                      std::map<std::string, std::vector<LFAssemblyPatchSite>> patchSites);
};
