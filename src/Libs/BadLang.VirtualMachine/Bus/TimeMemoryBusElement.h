#pragma once
#include "MemoryBusElement.h"

class TimeMemoryBusElement : public MemoryBusElement
{
private:
    uint8_t* m_ReadPtr;
public:
    TimeMemoryBusElement(uint8_t* readPtr);
    void Write(uint8_t* ptr, uint64_t value) override;
    uint64_t Read(uint8_t* ptr) override;
    bool CanRead(uint8_t* ptr) override;
};
