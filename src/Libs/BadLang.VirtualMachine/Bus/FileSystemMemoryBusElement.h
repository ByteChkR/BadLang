#pragma once
#include <fstream>

#include "MemoryBusElement.h"

enum FileSystemMemoryBusElementStatus
{
    NONE = 0,
    FILE_OPEN_READ = 1,
    ERR_FILE_OPEN_READ = 2,
    FILE_OPEN_WRITE = 3,
    ERR_FILE_OPEN_WRITE = 4,
    FILE_SET = 5,
    ERR_FILE_SET = 6,
    FILE_WRITE_8 = 7,
    ERR_FILE_WRITE_8 = 8,
    FILE_WRITE_16 = 9,
    ERR_FILE_WRITE_16 = 10,
    FILE_WRITE_32 = 11,
    ERR_FILE_WRITE_32 = 12,
    FILE_WRITE_64 = 13,
    ERR_FILE_WRITE_64 = 14,
    FILE_READ_8 = 15,
    ERR_FILE_READ_8 = 16,
    FILE_READ_16 = 17,
    ERR_FILE_READ_16 = 18,
    FILE_READ_32 = 19,
    ERR_FILE_READ_32 = 20,
    FILE_READ_64 = 21,
    ERR_FILE_READ_64 = 22,
    FILE_GET=23,
    ERR_FILE_GET=24,
    FILE_CLOSE=25,
    ERR_FILE_CLOSE=26,
    FILE_GET_POSITION=27,
    ERR_FILE_GET_POSITION=28,
    FILE_GET_EOF=29,
    ERR_FILE_GET_EOF=30,
    FILE_GET_SIZE=31,
    ERR_FILE_GET_SIZE=32,
    FILE_EXISTS=33,
    ERR_FILE_EXISTS=34,
};

class FileSystemMemoryBusElement : public MemoryBusElement
{
private:
    uint8_t* m_StatusPtr;
    FileSystemMemoryBusElementStatus m_Status;
    std::fstream* m_CurrentFile;

    uint8_t* m_FileOpenRead;
    uint8_t* m_FileOpenWrite;
    uint8_t* m_SetFile;
    uint8_t* m_GetFile;
    uint8_t* m_Write8;
    uint8_t* m_Write16;
    uint8_t* m_Write32;
    uint8_t* m_Write64;
    uint8_t* m_Read8;
    uint8_t* m_Read16;
    uint8_t* m_Read32;
    uint8_t* m_Read64;
    uint8_t* m_FileClose;
    uint8_t* m_GetPosition;
    uint8_t* m_GetEOF;
    uint8_t* m_GetSize;


    uint8_t* m_FileExists;


    void SetStatus(FileSystemMemoryBusElementStatus status);
public:
    FileSystemMemoryBusElement(uint8_t* statusPtr, uint8_t* fileOpenRead, uint8_t* fileOpenWrite, uint8_t* setFile,
                               uint8_t* getFile, uint8_t* write8, uint8_t* write16, uint8_t* write32, uint8_t* write64,
                               uint8_t* read8, uint8_t* read16, uint8_t* read32, uint8_t* read64, uint8_t* fileClose,
                               uint8_t* getPosition, uint8_t* getEOF, uint8_t* getSize, uint8_t* fileExists);
    void Write(uint8_t* ptr, uint64_t value) override;
    uint64_t Read(uint8_t* ptr) override;
    bool CanRead(uint8_t* ptr) override;
};
