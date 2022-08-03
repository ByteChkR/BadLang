#pragma once
#include "MappedAssembly.h"
#include "../../Debugging/LFAssemblyDebugSymbol.h"
#include "../../Debugging/LFAssemblyDebugSymbolServer.h"
#include "../Structures/LFAssembly.h"

class MappedAssembly;
class LFAssemblyDebugSymbolServer;

class AssemblyMapper
{
    std::vector<MappedAssembly*> m_MappedAssemblies;
    std::map<MappedAssembly*, std::vector<LFAssemblyDebugSymbol*>> m_MappedSymbols;
public:
    AssemblyMapper();
    MappedAssembly* Map(const std::string& fileName, const std::string& workingDir, LFAssembly* assembly,
                        bool noSymbols);
    std::vector<LFAssemblyTarget> CreateTargets(MappedAssembly* assembly, LFAssemblySection* section) const;
    void PatchAssemblies();
    [[nodiscard]] LFAssemblyDebugSymbolServer* GetSymbolServer() const;
};
