#include "FileSystemMemoryBusElement.h"

void FileSystemMemoryBusElement::SetStatus(FileSystemMemoryBusElementStatus status)
{
    m_Status = status;
}

FileSystemMemoryBusElement::FileSystemMemoryBusElement(uint8_t* statusPtr, uint8_t* fileOpenRead,
                                                       uint8_t* fileOpenWrite, uint8_t* setFile, uint8_t* getFile,
                                                       uint8_t* write8, uint8_t* write16, uint8_t* write32,
                                                       uint8_t* write64, uint8_t* read8, uint8_t* read16,
                                                       uint8_t* read32, uint8_t* read64, uint8_t* fileClose,
                                                       uint8_t* getPosition, uint8_t* getEOF, uint8_t* getSize,
                                                       uint8_t* fileExists)
{
    m_StatusPtr = statusPtr;
    m_FileOpenRead = fileOpenRead;
    m_FileOpenWrite = fileOpenWrite;
    m_SetFile = setFile;
    m_GetFile = getFile;
    m_Write8 = write8;
    m_Write16 = write16;
    m_Write32 = write32;
    m_Write64 = write64;
    m_Read8 = read8;
    m_Read16 = read16;
    m_Read32 = read32;
    m_Read64 = read64;
    m_FileClose = fileClose;
    m_GetPosition = getPosition;
    m_GetEOF = getEOF;
    m_GetSize = getSize;
    m_FileExists = fileExists;
    m_Status = NONE;
    m_CurrentFile = nullptr;
}


void FileSystemMemoryBusElement::Write(uint8_t* ptr, uint64_t value)
{
    if (m_FileOpenRead == ptr)
    {
        const char* fileName = reinterpret_cast<char*>(value);
        m_CurrentFile = new std::fstream(fileName, std::ios::in | std::ios::binary);

        if (m_CurrentFile)
        {
            SetStatus(FILE_OPEN_READ);
        }
        else
        {
            SetStatus(ERR_FILE_OPEN_READ);
        }
    }
    else if (m_FileOpenWrite == ptr)
    {
        const char* fileName = reinterpret_cast<char*>(value);
        m_CurrentFile = new std::fstream(fileName, std::ios::out | std::ios::binary);

        if (m_CurrentFile)
        {
            SetStatus(FILE_OPEN_WRITE);
        }
        else
        {
            SetStatus(ERR_FILE_OPEN_WRITE);
        }
    }
    else if (m_SetFile == ptr)
    {
        m_CurrentFile = reinterpret_cast<std::fstream*>(value);
        SetStatus(FILE_SET);
    }
    else if (m_Write8 == ptr)
    {
        if (m_CurrentFile)
        {
            char c = static_cast<char>(value);
            m_CurrentFile->write(&c, 1);
            SetStatus(FILE_WRITE_8);
        }
        else
        {
            SetStatus(ERR_FILE_WRITE_8);
        }
    }
    else if (m_Write16 == ptr)
    {
        if (m_CurrentFile)
        {
            auto c = static_cast<uint16_t>(value);
            m_CurrentFile->write(reinterpret_cast<char*>(&c), 2);
            SetStatus(FILE_WRITE_16);
        }
        else
        {
            SetStatus(ERR_FILE_WRITE_16);
        }
    }
    else if (m_Write32 == ptr)
    {
        if (m_CurrentFile)
        {
            auto c = static_cast<uint32_t>(value);
            m_CurrentFile->write(reinterpret_cast<char*>(&c), 4);
            SetStatus(FILE_WRITE_32);
        }
        else
        {
            SetStatus(ERR_FILE_WRITE_32);
        }
    }
    else if (m_Write64 == ptr)
    {
        if (m_CurrentFile)
        {
            auto c = value;
            m_CurrentFile->write(reinterpret_cast<char*>(&c), 8);
            SetStatus(FILE_WRITE_64);
        }
        else
        {
            SetStatus(ERR_FILE_WRITE_64);
        }
    }
}

uint64_t FileSystemMemoryBusElement::Read(uint8_t* ptr)
{
    if (ptr == m_StatusPtr)
        return m_Status;

    if (ptr == m_GetFile)
    {
        SetStatus(FILE_GET);
        return reinterpret_cast<uint64_t>(m_CurrentFile);
    }
    if (ptr == m_Read8)
    {
        if (m_CurrentFile)
        {
            char chr;

            if (m_CurrentFile->read(&chr, 1))
            {
                SetStatus(FILE_READ_8);
                return static_cast<uint64_t>(chr);
            }
        }

        SetStatus(ERR_FILE_READ_8);
        return 0;
    }
    if (ptr == m_Read16)
    {
        if (m_CurrentFile)
        {
            uint16_t c;
            if (m_CurrentFile->read(reinterpret_cast<char*>(&c), 2))
            {
                SetStatus(FILE_READ_16);
                return c;
            }
        }

        SetStatus(ERR_FILE_READ_16);
        return 0;
    }
    if (ptr == m_Read32)
    {
        if (m_CurrentFile)
        {
            uint32_t c;
            if (m_CurrentFile->read(reinterpret_cast<char*>(&c), 4))
            {
                SetStatus(FILE_READ_32);
                return c;
            }
        }

        SetStatus(ERR_FILE_READ_32);
        return 0;
    }
    if (ptr == m_Read64)
    {
        if (m_CurrentFile)
        {
            uint64_t c;
            if (m_CurrentFile->read(reinterpret_cast<char*>(&c), 8))
            {
                SetStatus(FILE_READ_64);
                return c;
            }
        }

        SetStatus(ERR_FILE_READ_64);
        return 0;
    }
    if (ptr == m_FileClose)
    {
        if (m_CurrentFile)
        {
            m_CurrentFile->close();
            delete m_CurrentFile;
            SetStatus(FILE_CLOSE);
            return 1;
        }

        SetStatus(ERR_FILE_CLOSE);
        return 0;
    }
    if (ptr == m_GetPosition)
    {
        if (m_CurrentFile)
        {
            SetStatus(FILE_GET_POSITION);
            return m_CurrentFile->tellg();
        }

        SetStatus(ERR_FILE_GET_POSITION);
        return 0;
    }

    if (ptr == m_GetEOF)
    {
        if (m_CurrentFile)
        {
            SetStatus(FILE_GET_EOF);
            return m_CurrentFile->eof() ? 1 : 0;
        }

        SetStatus(ERR_FILE_GET_EOF);
        return 0;
    }

    if (ptr == m_GetSize)
    {
        if (m_CurrentFile)
        {
            SetStatus(FILE_GET_SIZE);
            int64_t pos = m_CurrentFile->tellg();
            m_CurrentFile->seekg(0, std::ios::end);
            int64_t size = m_CurrentFile->tellg();
            m_CurrentFile->seekg(pos, std::ios::beg);
            return size;
        }
        SetStatus(ERR_FILE_GET_SIZE);
        return 0;
    }
    if (ptr == m_FileExists)
    {
        if (m_CurrentFile != nullptr)
        {
            SetStatus(FILE_EXISTS);
            return m_CurrentFile->good() ? 1 : 0;
        }
        SetStatus(ERR_FILE_EXISTS);
        return 0;
    }
    throw std::invalid_argument("FileSystemMemoryBusElement::Read: Invalid pointer");
}

bool FileSystemMemoryBusElement::CanRead(uint8_t* ptr)
{
    return ptr == m_StatusPtr ||
        ptr == m_GetFile ||
        ptr == m_Read8 ||
        ptr == m_Read16 ||
        ptr == m_Read32 ||
        ptr == m_Read64 ||
        ptr == m_GetPosition ||
        ptr == m_FileClose ||
        ptr == m_GetEOF ||
        ptr == m_GetSize ||
        ptr == m_FileExists;
}
