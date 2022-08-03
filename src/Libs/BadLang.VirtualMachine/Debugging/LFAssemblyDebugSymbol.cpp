#include "LFAssemblyDebugSymbol.h"

#include <algorithm>
#include <fstream>
#include <iostream>
#include <sstream>
#include <utility>

LFAssemblyDebugSymbol::LFAssemblyDebugSymbol(std::string fileName, int startIndex, int endIndex,
                                             std::string sectionName, std::string symbolName, int sectionOffset,
                                             std::map<std::string, std::string>* fileMap)
{
    this->FileName = std::move(fileName);
    this->StartIndex = startIndex;
    this->EndIndex = endIndex;
    this->SectionName = std::move(sectionName);
    this->SymbolName = std::move(symbolName);
    this->SectionOffset = sectionOffset;


    if (fileMap->contains(this->FileName))
    {
        m_HasFile = true;
        m_FileContent = fileMap->at(this->FileName);
    }
    else
    {
        std::ifstream file(this->FileName, std::ios::binary);

        if (!file.is_open())
        {
            m_HasFile = false;
            m_FileContent = "";
        }
        else
        {
            m_HasFile = true;
            m_FileContent = std::string((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
            fileMap->insert(std::make_pair(this->FileName, m_FileContent));
            file.close();
        }
    }

    std::ranges::replace(FileName, '\\', '/');
}

int LFAssemblyDebugSymbol::GetLineNumber() const
{
    if (!m_HasFile)
        return -1;

    int index = 1;
    for (int i = 0; i < this->StartIndex; i++)
    {
        if (m_FileContent[i] == '\n')
            index++;
    }
    return index;
}

int LFAssemblyDebugSymbol::GetLinePosition() const
{
    if (!m_HasFile)
        return -1;

    int index = 0;
    for (int i = 0; i < this->StartIndex; i++)
    {
        if (m_FileContent[i] == '\n')
            index = 0;
        else
            index++;
    }
    return index;
}


void LFAssemblyDebugSymbol::WriteSymbolString() const
{
    if (!m_HasFile)
    {
        std::cout << SectionName << "::" << SymbolName << " in file " << FileName << "[" << StartIndex << " - " <<
            EndIndex << "]" << std::endl;
    }
    else
    {
        std::cout << SectionName << "::" << SymbolName << " in file://" << FileName << " : " << GetLineNumber() << "" <<
            std::endl;
    }
}

void LFAssemblyDebugSymbol::WriteSymbolContent() const
{
    if (!m_HasFile)
    {
        std::cout << "No file" << std::endl;
        return;
    }
    std::cout << m_FileContent.substr(StartIndex, (EndIndex - StartIndex) - 1) << std::endl;
}
