#pragma once
#include <chrono>
#include <string>

class BenchmarkSection
{
    std::string m_Name;
    std::chrono::high_resolution_clock::time_point m_StartTime;
    std::chrono::high_resolution_clock::time_point m_EndTime;
public:
    BenchmarkSection(std::string name);
    void SetStart(std::chrono::high_resolution_clock::time_point start);
    void SetEnd(std::chrono::high_resolution_clock::time_point end);
    std::string GetName();
    [[nodiscard]] std::chrono::high_resolution_clock::time_point GetStart() const;
    [[nodiscard]] std::chrono::high_resolution_clock::time_point GetEnd() const;
};

class Benchmark
{
private:
    inline static std::vector<BenchmarkSection*> m_Sections = {};
    inline static std::chrono::high_resolution_clock::time_point m_Start = std::chrono::high_resolution_clock::now();

public:
    static BenchmarkSection* StartSection(std::string section);
    static void EndSection(BenchmarkSection* section);

    static void WriteResults();
    static void Clear();
};
