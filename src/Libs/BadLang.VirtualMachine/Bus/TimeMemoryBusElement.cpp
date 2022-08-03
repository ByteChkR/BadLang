#include "TimeMemoryBusElement.h"

#include <chrono>
#include <stdexcept>


TimeMemoryBusElement::TimeMemoryBusElement(uint8_t* readPtr)
{
    m_ReadPtr = readPtr;
}

bool TimeMemoryBusElement::CanRead(uint8_t* ptr)
{
    return m_ReadPtr == ptr;
}

void TimeMemoryBusElement::Write(uint8_t* ptr, uint64_t value)
{
}

uint64_t TimeMemoryBusElement::Read(uint8_t* ptr)
{
    if (ptr == m_ReadPtr)
    {
        return std::chrono::duration_cast<std::chrono::microseconds>(
            std::chrono::system_clock::now().time_since_epoch()).count();
    }
    throw std::invalid_argument("ConsoleMemoryBusElement::Read");
}
