#pragma once
#include <map>
#include <vector>

#include "LFAssemblyDebugSymbol.h"
#include "../Assembly/Mapping/MappedAssembly.h"

class MappedAssembly;

class LFAssemblyDebugSymbolServer
{
private:
    std::map<MappedAssembly*, std::vector<LFAssemblyDebugSymbol*>> m_Symbols;
public:
    LFAssemblyDebugSymbolServer(std::map<MappedAssembly*, std::vector<LFAssemblyDebugSymbol*>> symbols);

    bool HasSymbol(uint8_t* address) const;
    LFAssemblyDebugSymbol* GetSymbol(uint8_t* address) const;
};
