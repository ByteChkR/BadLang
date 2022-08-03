#pragma once
#include <map>
#include <string>

class LFAssemblyDebugSymbol
{
private:
    std::string m_FileContent;
    bool m_HasFile;
public:
    LFAssemblyDebugSymbol(std::string fileName, int startIndex, int endIndex, std::string sectionName,
                          std::string symbolName, int sectionOffset, std::map<std::string, std::string>* fileMap);
    std::string FileName;
    int StartIndex;
    int EndIndex;
    std::string SectionName;
    std::string SymbolName;
    uint64_t SectionOffset;
    int GetLineNumber() const;
    int GetLinePosition() const;
    void WriteSymbolString() const;
    void WriteSymbolContent() const;
};
