#pragma once
#include <string>

class LFAssemblyTarget
{
public:
    std::string Name;
    uint64_t Offset;
    LFAssemblyTarget(std::string name, uint64_t offset);
};
