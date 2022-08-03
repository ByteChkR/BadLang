#include <chrono>
#include <iostream>
#include <ostream>

#include "../../Libs/BadLang.VirtualMachine/Benchmark.h"
#include "../../Libs/BadLang.VirtualMachine/VirtualMachine.h"

constexpr size_t VM_STACK_SIZE = 1024ULL * 1024ULL;

bool HasFlag(int argc, char* argv[], const char* flag, int start)
{
    for (int i = start; i < argc; ++i)
    {
        if (strcmp(argv[i], flag) == 0)
        {
            return true;
        }
    }
    return false;
}

int main(int argc, char* argv[])
{
    if (argc < 2)
    {
        std::cout << "Usage: <binary file>" << std::endl;
        return -1;
    }

    bool noSymbols = HasFlag(argc, argv, "--no-symbols", 2) ||
        HasFlag(argc, argv, "-nsym", 2);

    VirtualMachine::Run(argv[1], VM_STACK_SIZE, noSymbols);

    Benchmark::WriteResults();
    Benchmark::Clear();

    return 0;
}
