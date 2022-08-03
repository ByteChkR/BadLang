#pragma once
#include <cstdint>
#include <string>
#include <vector>

#include "Bus/MemoryBus.h"
#include "Debugging/LFAssemblyDebugSymbol.h"
#include "Debugging/LFAssemblyDebugSymbolServer.h"

class VirtualMachine
{
private:
    uint8_t* m_PC;
    uint8_t* m_SP;
    bool m_Halted;
    std::vector<uint8_t*> m_CallStack;
    std::vector<uint8_t*> m_CallTargets;
    std::vector<uint8_t*> m_StackFrames;
    LFAssemblyDebugSymbolServer* m_SymbolServer;
    MemoryBus m_Bus;

    template <typename T>
    T ReadPC();
    template <typename T>
    T ReadSP();
    template <typename T>
    void WriteSP(T value);

    template <typename T>
    void Add();
    template <typename T>
    void Sub();
    template <typename T>
    void Mul();
    template <typename T>
    void Div();
    template <typename T>
    void Mod();
    template <typename T>
    void And();
    template <typename T>
    void Or();
    template <typename T>
    void XOr();
    template <typename T>
    void Not();
    template <typename T>
    void Shl();
    template <typename T>
    void Shr();

    template <typename T>
    void Load();
    void LoadN();
    template <typename T>
    void Store();
    void StoreN();
    template <typename T>
    void Assign();
    void AssignN();
    template <typename T>
    void Push();
    template <typename T>
    void Pop();
    template <typename T>
    void Dup();
    template <typename T>
    void Swap();
    template <typename T>
    void TestZero();
    template <typename T>
    void TestEqual();
    template <typename T>
    void TestNotEqual();
    template <typename T>
    void TestLess();
    template <typename T>
    void TestLessEqual();
    template <typename T>
    void TestGreater();
    template <typename T>
    void TestGreaterEqual();

    inline void PushSF();
    inline void MoveSP();

    void Jump();
    void JumpRel();
    template <typename T>
    void JumpIfZero();
    template <typename T>
    void JumpIfNotZero();

    inline void Call();
    inline void CallRel();
    inline void Return();
    inline void Alloc();
    inline void Free();
    inline void Halt();
    inline void Error();
    void GetStackTrace() const;
    template <typename T>
    void WriteBus();
    template <typename T>
    void ReadBus();


    void Cycle();
public:
    VirtualMachine(uint8_t* pc, uint8_t* sp, LFAssemblyDebugSymbolServer* symbolServer);

    static VirtualMachine FromFile(const std::string& filename, size_t stackSize, bool noSymbols);
    static void Run(const std::string& filename, size_t stackSize, bool noSymbols);
    void Run();
};
