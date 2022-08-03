#include "MemoryBus.h"

#include <utility>

MemoryBus::MemoryBus(std::vector<MemoryBusElement*> elements)
{
    m_Elements = std::move(elements);
}

MemoryBus::MemoryBus()
{
    m_Elements = std::vector<MemoryBusElement*>();
}

void MemoryBus::AddElement(MemoryBusElement* element)
{
    m_Elements.push_back(element);
}

uint64_t MemoryBus::Read(uint8_t* ptr) const
{
    for (MemoryBusElement* element : m_Elements)
    {
        if (element->CanRead(ptr))
        {
            return element->Read(ptr);
        }
    }

    return 0;
}

void MemoryBus::Write(uint8_t* ptr, uint64_t value) const
{
    for (MemoryBusElement* element : m_Elements)
    {
        element->Write(ptr, value);
    }
}
