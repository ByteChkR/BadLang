#pragma once
#include <string>
#include <cstring>

#include "../Structures/LFAssembly.h"


class LFAssemblyDebugSymbol;

class LFAssemblyLoader
{
public:
    LFAssemblyLoader();

    static LFAssembly* LoadAssembly(const std::string& fileName);

    static std::vector<LFAssemblyDebugSymbol*> LoadSymbols(const std::string& filename);
};
