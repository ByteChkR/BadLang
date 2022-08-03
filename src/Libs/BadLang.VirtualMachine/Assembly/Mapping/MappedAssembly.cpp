#include "MappedAssembly.h"

#include <iostream>
#include <ostream>
#include <utility>

#include "AssemblyMapper.h"


std::string MappedAssembly::GetName() const
{
    return m_BaseAssembly->GetName();
}

LFAssembly* MappedAssembly::GetBaseAssembly() const
{
    return m_BaseAssembly;
}

uint8_t* MappedAssembly::GetMappedSection(LFAssemblySection* section)
{
    return m_MappedSections[section];
}

uint8_t* MappedAssembly::GetMappedSection(const std::string& name) const
{
    for (auto section : m_MappedSections)
    {
        if (section.first->GetName() == name)
        {
            return section.second;
        }
    }
    return nullptr;
}

MappedAssembly::MappedAssembly(LFAssembly* assembly, std::map<LFAssemblySection*, uint8_t*> mappedSections,
                               std::vector<std::string> requiredAssemblies)
{
    m_BaseAssembly = assembly;
    m_MappedSections = std::move(mappedSections);
    m_RequiredAssemblies = std::move(requiredAssemblies);
}

void PatchSection(AssemblyMapper* mapper, MappedAssembly* assembly, LFAssemblySection* section, uint8_t* data)
{
    const std::map<std::string, std::vector<LFAssemblyPatchSite>> patchSites = section->GetPatchSites();
    const std::vector<LFAssemblyTarget> targets = mapper->CreateTargets(assembly, section);
    for (const auto& patchSite : patchSites)
    {
        bool patched = false;
        for (const auto& target : targets)
        {
            if (target.Name == patchSite.first)
            {
                for (const auto patch : patchSite.second)
                {
                    patched = true;
#if _DEBUG
                    std::cout << "Patching Offset " << patch.Offset << " with " << target.Offset << " size: " << patch.
                        Size << std::endl;
#endif
                    switch (patch.Size)
                    {
                    case 1:
                        *(data + patch.Offset) = static_cast<char>(target.Offset);
                        break;
                    case 2:
                        *(reinterpret_cast<uint16_t*>(data + patch.Offset)) = static_cast<uint16_t>(target.Offset);
                        break;
                    case 4:
                        *(reinterpret_cast<uint32_t*>(data + patch.Offset)) = static_cast<uint32_t>(target.Offset);
                        break;
                    case 8:
                        *(reinterpret_cast<uint64_t*>(data + patch.Offset)) = target.Offset;
                        break;
                    default:
                        throw std::invalid_argument("Invalid patch size");
                    }
                }

                break;
            }
        }

        if (!patched)
        {
            throw std::invalid_argument("Could not find target for patch site " + patchSite.first);
        }
    }
}

std::vector<LFAssemblyTarget> MappedAssembly::GetExportedTargets(const std::vector<std::string>& sectionNames) const
{
    std::vector<LFAssemblyTarget> targets = {};

    for (auto section : m_MappedSections)
    {
        for (const auto& sectionName : sectionNames)
        {
            if (section.first->GetName() == sectionName)
            {
                for (const auto& targetName : section.first->GetExportNames())
                {
                    LFAssemblyTarget target = section.first->GetTarget(targetName);
                    targets.emplace_back(
                        m_BaseAssembly->GetName() + "::" + section.first->GetName() + "::" + target.Name,
                        reinterpret_cast<uint64_t>(section.second) + target.Offset);
                }
            }
        }
    }

    return targets;
}


std::vector<std::string> MappedAssembly::GetRequiredAssemblies() const
{
    return m_RequiredAssemblies;
}


void MappedAssembly::PatchSections(AssemblyMapper* mapper)
{
    for (const auto section : m_MappedSections)
    {
        PatchSection(mapper, this, section.first, section.second);
    }
}
