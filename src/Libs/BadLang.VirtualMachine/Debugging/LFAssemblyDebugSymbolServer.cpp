#include "LFAssemblyDebugSymbolServer.h"

#include <utility>

LFAssemblyDebugSymbolServer::LFAssemblyDebugSymbolServer(
    std::map<MappedAssembly*, std::vector<LFAssemblyDebugSymbol*>> symbols)
{
    m_Symbols = std::move(symbols);
}

bool LFAssemblyDebugSymbolServer::HasSymbol(uint8_t* address) const
{
    for (const auto& pair : m_Symbols)
    {
        for (const auto symbol : pair.second)
        {
            if (symbol->SectionOffset == reinterpret_cast<uint64_t>(address))
            {
                return true;
            }
        }
    }
    return false;
}

LFAssemblyDebugSymbol* LFAssemblyDebugSymbolServer::GetSymbol(uint8_t* address) const
{
    for (const auto& pair : m_Symbols)
    {
        for (const auto symbol : pair.second)
        {
            if (symbol->SectionOffset == reinterpret_cast<uint64_t>(address) && !symbol->SymbolName.empty())
            {
                return symbol;
            }
        }
    }

    for (const auto& pair : m_Symbols)
    {
        for (const auto symbol : pair.second)
        {
            if (symbol->SectionOffset == reinterpret_cast<uint64_t>(address))
            {
                return symbol;
            }
        }
    }
    return nullptr;
}
