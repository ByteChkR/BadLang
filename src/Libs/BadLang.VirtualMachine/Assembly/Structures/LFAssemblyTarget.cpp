#include "LFAssemblyTarget.h"

#include <utility>


LFAssemblyTarget::LFAssemblyTarget(std::string name, uint64_t offset)
{
    Name = std::move(name);
    Offset = offset;
}
