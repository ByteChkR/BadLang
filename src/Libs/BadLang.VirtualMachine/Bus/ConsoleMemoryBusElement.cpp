#include "ConsoleMemoryBusElement.h"

#include <iostream>

#include "MemoryBusElement.h"

void ConsoleMemoryBusElement::Write(uint8_t* ptr, uint64_t value)
{
    if (ptr == m_WritePtr)
    {
        std::cout << static_cast<char>(value);
    }
    else if (ptr == m_WriteNumPtr)
    {
        std::cout << value;
    }
    else if (ptr == m_WriteHexNumPtr)
    {
        std::cout << "0x" << reinterpret_cast<void*>(value);
    }
}

bool ConsoleMemoryBusElement::CanRead(uint8_t* ptr)
{
    return m_ReadPtr == ptr;
}

ConsoleMemoryBusElement::ConsoleMemoryBusElement(uint8_t* writePtr, uint8_t* readPtr, uint8_t* writeNumPtr,
                                                 uint8_t* writeHexNumPtr)
{
    m_WritePtr = writePtr;
    m_ReadPtr = readPtr;
    m_WriteNumPtr = writeNumPtr;
    m_WriteHexNumPtr = writeHexNumPtr;
}


uint64_t ConsoleMemoryBusElement::Read(uint8_t* ptr)
{
    if (ptr == m_ReadPtr)
    {
        const auto c = std::cin.get();
        return c;
    }
    throw std::invalid_argument("ConsoleMemoryBusElement::Read");
}
