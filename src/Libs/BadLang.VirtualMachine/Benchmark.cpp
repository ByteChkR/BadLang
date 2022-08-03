#include "Benchmark.h"

#include <chrono>
#include <iostream>


BenchmarkSection::BenchmarkSection(std::string name)
{
    m_Name = name;
}

void BenchmarkSection::SetStart(std::chrono::high_resolution_clock::time_point start)
{
    m_StartTime = start;
}

void BenchmarkSection::SetEnd(std::chrono::high_resolution_clock::time_point end)
{
    m_EndTime = end;
}

std::string BenchmarkSection::GetName()
{
    return m_Name;
}

std::chrono::high_resolution_clock::time_point BenchmarkSection::GetEnd() const
{
    return m_EndTime;
}

std::chrono::high_resolution_clock::time_point BenchmarkSection::GetStart() const
{
    return m_StartTime;
}


BenchmarkSection* Benchmark::StartSection(std::string section)
{
    if (m_Sections.empty())
    {
        m_Start = std::chrono::high_resolution_clock::now();
    }
    auto s = new BenchmarkSection(std::move(section));
    s->SetStart(std::chrono::high_resolution_clock::now());
    m_Sections.push_back(s);
    return s;
}


void Benchmark::EndSection(BenchmarkSection* section)
{
    section->SetEnd(std::chrono::high_resolution_clock::now());
}

void Benchmark::Clear()
{
    for (auto s : m_Sections)
    {
        delete s;
    }
    m_Sections.clear();
}

void Benchmark::WriteResults()
{
    auto end = std::chrono::high_resolution_clock::now();
    std::cout << "Benchmark results:" << std::endl;
    for (auto s : m_Sections)
    {
        std::cout << '\t' << s->GetName() << ": " << std::chrono::duration_cast<
            std::chrono::microseconds>(s->GetEnd() - s->GetStart()).count() << " microseconds" << std::endl;
    }
    std::cout << "Total time: " << std::chrono::duration_cast<std::chrono::microseconds>(end - m_Start).count() <<
        " microseconds" << std::endl;
}
