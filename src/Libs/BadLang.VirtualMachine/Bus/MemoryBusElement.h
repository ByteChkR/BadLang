#pragma once
#include <cstdint>


class MemoryBusElement
{
public:
    virtual void Write(uint8_t* ptr, uint64_t value) = 0;
    virtual uint64_t Read(uint8_t* ptr) = 0;
    virtual bool CanRead(uint8_t* ptr) = 0;

    virtual ~MemoryBusElement() = default;
};
