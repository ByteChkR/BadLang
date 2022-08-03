#pragma once
#include <cstdint>

#include "MemoryBusElement.h"

class ConsoleMemoryBusElement : public MemoryBusElement
{
private:
    uint8_t* m_WritePtr;
    uint8_t* m_WriteNumPtr;
    uint8_t* m_WriteHexNumPtr;
    uint8_t* m_ReadPtr;

public:
    ConsoleMemoryBusElement(uint8_t* writePtr, uint8_t* readPtr, uint8_t* writeNumPtr, uint8_t* writeHexNumPtr);
    void Write(uint8_t* ptr, uint64_t value) override;
    uint64_t Read(uint8_t* ptr) override;
    bool CanRead(uint8_t* ptr) override;
};
