#pragma once
#include <cstdint>
#include <vector>

#include "MemoryBusElement.h"


class MemoryBus
{
private:
    std::vector<MemoryBusElement*> m_Elements;
public:
    MemoryBus(std::vector<MemoryBusElement*> elements);
    MemoryBus();
    uint64_t Read(uint8_t* ptr) const;
    void Write(uint8_t* ptr, uint64_t value) const;
    void AddElement(MemoryBusElement* element);
};
