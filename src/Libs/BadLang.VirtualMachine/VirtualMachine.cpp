#include "VirtualMachine.h"

#include <exception>
#include <iostream>
#include <ostream>

#include "Benchmark.h"
#include "OpCodes.h"
#include "Assembly/Loader/LFAssemblyLoader.h"
#include "Assembly/Mapping/AssemblyMapper.h"
#include "Bus/ConsoleMemoryBusElement.h"
#include "Bus/FileSystemMemoryBusElement.h"
#include "Bus/TimeMemoryBusElement.h"


VirtualMachine::VirtualMachine(uint8_t* pc, uint8_t* sp, LFAssemblyDebugSymbolServer* symbolServer)
{
    m_PC = pc;
    m_SP = sp;
    m_Halted = false;
    m_Bus = MemoryBus();
    m_Bus.AddElement(new ConsoleMemoryBusElement(
        reinterpret_cast<uint8_t*>(0xFAFAFAFAULL),
        reinterpret_cast<uint8_t*>(0xFAFAFAFBULL),
        reinterpret_cast<uint8_t*>(0xFAFAFAFCULL),
        reinterpret_cast<uint8_t*>(0xFAFAFAFDULL)));
    m_Bus.AddElement(new TimeMemoryBusElement(reinterpret_cast<uint8_t*>(0x54696d65ULL)));

    m_Bus.AddElement(new FileSystemMemoryBusElement(
        reinterpret_cast<uint8_t*>(0xDAFAFAF0ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF1ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF2ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF3ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF4ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF5ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF6ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF7ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF8ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAF9ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFAULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFBULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFCULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFDULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFEULL),
        reinterpret_cast<uint8_t*>(0xDAFAFAFFULL),
        reinterpret_cast<uint8_t*>(0xDAFAFB00ULL),
        reinterpret_cast<uint8_t*>(0xDAFAFB01ULL)));

    m_CallStack = {};
    m_StackFrames = {};
    m_CallTargets = {};
    m_SymbolServer = symbolServer;
}


void VirtualMachine::Run(const std::string& filename, size_t stackSize, bool noSymbols)
{
    VirtualMachine vm = FromFile(filename, stackSize, noSymbols);
    auto section = Benchmark::StartSection("Execution");
    vm.Run();
    Benchmark::EndSection(section);
}


VirtualMachine VirtualMachine::FromFile(const std::string& filename, size_t stackSize, bool noSymbols)
{
    LFAssembly* assembly = LFAssemblyLoader::LoadAssembly(filename);

    auto mapper = AssemblyMapper();
    std::string directory;
    size_t last_bslash_idx = filename.rfind('\\');
    size_t last_fslash_idx = filename.rfind('/');
    size_t last_slash_idx = last_bslash_idx;
    if (last_slash_idx == std::string::npos || (last_fslash_idx != std::string::npos && last_slash_idx <
        last_fslash_idx))
    {
        last_slash_idx = last_fslash_idx;
    }

    if (std::string::npos != last_slash_idx)
    {
        directory = filename.substr(0, last_slash_idx);
    }
    else
    {
        throw std::invalid_argument("Invalid required assembly");
    }
    const MappedAssembly* mappedAssembly = mapper.Map(filename, directory, assembly, noSymbols);

    auto section = Benchmark::StartSection("Patching Assemblies");
    mapper.PatchAssemblies();
    Benchmark::EndSection(section);

    uint8_t* data = mappedAssembly->GetMappedSection("entry");
    if (data == nullptr)
    {
        throw std::invalid_argument("No entry section found");
    }

    const auto stack = new uint8_t[stackSize];


    LFAssemblyDebugSymbolServer* symServer = mapper.GetSymbolServer();
    auto vm = VirtualMachine(data, stack, symServer);

    return vm;
}

void VirtualMachine::GetStackTrace() const
{
    for (const auto& frame : m_CallTargets)
    {
        if (m_SymbolServer->HasSymbol(frame))
        {
            auto symbol = m_SymbolServer->GetSymbol(frame);
            std::cout << '\t';
            symbol->WriteSymbolString();
        }
        else
        {
            std::cout << "\t0x" << reinterpret_cast<void*>(frame) << std::endl;
        }
    }
}


inline void VirtualMachine::Error()
{
    auto errCode = ReadSP<char*>();
    std::cout << "Error: " << reinterpret_cast<uint64_t>(errCode) << std::endl;
    std::cout << "Message: " << errCode << std::endl;
    std::cout << "StackTrace: " << errCode << std::endl;
    GetStackTrace();
    m_Halted = true;
}


template <typename T>
T VirtualMachine::ReadPC()
{
    T value = *reinterpret_cast<T*>(m_PC);
    m_PC += sizeof(T);
    return value;
}

template <typename T>
void VirtualMachine::WriteBus()
{
    const auto ptr = ReadSP<uint8_t*>();
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "WriteBus: " << reinterpret_cast<uint64_t>(ptr) << " = " << static_cast<uint64_t>(value) << std::endl;
#endif
    m_Bus.Write(ptr, static_cast<uint64_t>(value));
}

template <typename T>
void VirtualMachine::ReadBus()
{
    const auto ptr = ReadSP<uint8_t*>();
#ifdef _DEBUG
    std::cout << "ReadBus: " << reinterpret_cast<uint64_t>(ptr) << std::endl;
#endif

    T value = static_cast<T>(m_Bus.Read(ptr));
    WriteSP<T>(value);
}


inline void VirtualMachine::PushSF()
{
#ifdef _DEBUG
    std::cout << "PushSF: " << reinterpret_cast<uint64_t>(m_StackFrames[m_StackFrames.size() - 1]) << std::endl;
#endif

    WriteSP(m_StackFrames[m_StackFrames.size() - 1]);
}


template <typename T>
T VirtualMachine::ReadSP()
{
    m_SP -= sizeof(T);
    T value = *reinterpret_cast<T*>(m_SP);
    return value;
}

template <typename T>
void VirtualMachine::WriteSP(T value)
{
    *reinterpret_cast<T*>(m_SP) = value;
    m_SP += sizeof(T);
}

template <typename T>
void VirtualMachine::Add()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a + b;
#ifdef _DEBUG
    std::cout << "Add: " << static_cast<uint64_t>(a) << " + " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

template <typename T>
void VirtualMachine::Sub()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a - b;
#ifdef _DEBUG
    std::cout << "Sub: " << static_cast<uint64_t>(a) << " - " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}


template <typename T>
void VirtualMachine::Mul()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a * b;
#ifdef _DEBUG
    std::cout << "Mul: " << static_cast<uint64_t>(a) << " * " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}


template <typename T>
void VirtualMachine::Div()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a / b;
#ifdef _DEBUG
    std::cout << "Div: " << static_cast<uint64_t>(a) << " / " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}


template <typename T>
void VirtualMachine::Mod()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a % b;
#ifdef _DEBUG
    std::cout << "Mod: " << static_cast<uint64_t>(a) << " % " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}


template <typename T>
void VirtualMachine::And()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a & b;
#ifdef _DEBUG
    std::cout << "And: " << static_cast<uint64_t>(a) << " & " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

template <typename T>
void VirtualMachine::Or()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a | b;
#ifdef _DEBUG
    std::cout << "Or: " << static_cast<uint64_t>(a) << " | " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}


template <typename T>
void VirtualMachine::XOr()
{
    T b = ReadSP<T>();
    T a = ReadSP<T>();
    T r = a ^ b;
#ifdef _DEBUG
    std::cout << "XOr: " << static_cast<uint64_t>(a) << " ^ " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

template <typename T>
void VirtualMachine::Not()
{
    T a = ReadSP<T>();
    T r = ~a;
#ifdef _DEBUG
    std::cout << "Not: " << static_cast<uint64_t>(a) << " ~ " << static_cast<uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

template <typename T>
void VirtualMachine::Shl()
{
    T a = ReadSP<T>();
    T b = ReadSP<T>();
    T r = a << b;
#ifdef _DEBUG
    std::cout << "Shl: " << static_cast<uint64_t>(a) << " << " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

template <typename T>
void VirtualMachine::Shr()
{
    T a = ReadSP<T>();
    T b = ReadSP<T>();
    T r = a >> b;
#ifdef _DEBUG
    std::cout << "Shr: " << static_cast<uint64_t>(a) << " >> " << static_cast<uint64_t>(b) << " = " << static_cast<
        uint64_t>(r) << std::endl;
#endif
    WriteSP<T>(r);
}

void VirtualMachine::JumpRel()
{
    const auto offset = ReadSP<uint64_t>();
#ifdef _DEBUG
    std::cout << "JumpRel: " << reinterpret_cast<uint64_t>(m_PC + offset) << std::endl;
#endif
    m_PC += offset;
}

inline void VirtualMachine::Jump()
{
    auto address = ReadSP<uint8_t*>();
#ifdef _DEBUG
    std::cout << "Jump: " << reinterpret_cast<uint64_t>(address) << std::endl;
#endif
    m_PC = address;
}

template <typename T>
void VirtualMachine::JumpIfZero()
{
    auto ptr = ReadSP<uint8_t*>();
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "JumpIfZero: " << static_cast<uint64_t>(value) << " == 0 ? " << reinterpret_cast<uint64_t>(ptr) <<
        std::endl;
#endif

    if (value == 0)
    {
        m_PC = ptr;
    }
}

template <typename T>
void VirtualMachine::JumpIfNotZero()
{
    auto ptr = ReadSP<uint8_t*>();
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "JumpIfNotZero: " << static_cast<uint64_t>(value) << " != 0 ? " << reinterpret_cast<uint64_t>(ptr) <<
        std::endl;
#endif
    if (value != 0)
    {
        m_PC = ptr;
    }
}

template <typename T>
void VirtualMachine::Load()
{
    T* ptr = ReadSP<T*>();
    T value = *ptr;
#ifdef _DEBUG
    std::cout << "Load: " << reinterpret_cast<uint64_t>(ptr) << " = " << static_cast<uint64_t>(value) << std::endl;
#endif
    WriteSP<T>(value);
}

void VirtualMachine::LoadN()
{
    uint64_t size = ReadPC<uint64_t>();
    auto ptr = ReadSP<uint8_t*>();

#ifdef _DEBUG
    std::cout << "LoadN: " << reinterpret_cast<uint64_t>(ptr) << " = " << " Size: " << size << std::endl;
#endif
    for (int i = 0; i < size; ++i)
    {
        WriteSP<uint8_t>(*ptr++);
    }
}


inline void VirtualMachine::MoveSP()
{
    const auto offset = ReadPC<uint32_t>();
#ifdef _DEBUG
    std::cout << "MoveSP: " << offset << std::endl;
#endif
    m_SP += offset;
}

template <typename T>
void VirtualMachine::Store()
{
    T* ptr = ReadSP<T*>();
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "Store: " << reinterpret_cast<uint64_t>(ptr) << " = " << static_cast<uint64_t>(value) << std::endl;
#endif
    *ptr = value;
}

void VirtualMachine::StoreN()
{
    uint64_t size = ReadPC<uint64_t>();
    auto ptr = ReadSP<uint8_t*>();
    uint8_t* source = m_SP - size;

#ifdef _DEBUG
    std::cout << "StoreN: " << reinterpret_cast<uint64_t>(ptr) << " = " << reinterpret_cast<uint64_t>(source) <<
        " Size: " << size << std::endl;
#endif
    for (uint64_t i = 0; i < size; ++i)
    {
        *ptr++ = *source++;
    }
    //Pop size of the stack
    m_SP -= size;
}


template <typename T>
void VirtualMachine::Assign()
{
    T value = ReadSP<T>();
    T* ptr = ReadSP<T*>();
#ifdef _DEBUG
    std::cout << "Assign: " << reinterpret_cast<uint64_t>(ptr) << " = " << static_cast<uint64_t>(value) << std::endl;
#endif
    *ptr = value;
}

void VirtualMachine::AssignN()
{
    uint64_t size = ReadPC<uint64_t>();
    uint8_t* ptrPosition = m_SP - size - sizeof(uint8_t*);
    uint8_t* dst = *reinterpret_cast<uint8_t**>(ptrPosition);
    uint8_t* src = m_SP - size;

#ifdef _DEBUG
    std::cout << "AssignN: " << reinterpret_cast<uint64_t>(dst) << " = " << reinterpret_cast<uint64_t>(dst) << " Size: "
        << size << std::endl;
#endif

    for (uint64_t i = 0; i < size; ++i)
    {
        *dst++ = *src++;
    }

    //Pop size of the stack
    m_SP -= size;
    m_SP -= sizeof(uint8_t*);
}

template <typename T>
void VirtualMachine::Pop()
{
#ifdef _DEBUG
    std::cout << "Pop: " << sizeof(T) << std::endl;
#endif
    T value = ReadSP<T>();
}

template <typename T>
void VirtualMachine::Push()
{
    T value = ReadPC<T>();
#ifdef _DEBUG
    std::cout << "Push: " << static_cast<uint64_t>(value) << std::endl;
#endif
    WriteSP(value);
}

template <typename T>
void VirtualMachine::Dup()
{
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "Dup: " << static_cast<uint64_t>(value) << std::endl;
#endif
    WriteSP(value);
    WriteSP(value);
}

template <typename T>
void VirtualMachine::Swap()
{
    T value1 = ReadSP<T>();
    T value2 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "Swap: " << static_cast<uint64_t>(value1) << " <-> " << static_cast<uint64_t>(value2) << std::endl;
#endif
    WriteSP(value1);
    WriteSP(value2);
}

template <typename T>
void VirtualMachine::TestZero()
{
    T value = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestZero: " << static_cast<uint64_t>(value) << " == 0 ? " << std::endl;
#endif
    WriteSP<T>(value == 0 ? 1 : 0);
}

template <typename T>
void VirtualMachine::TestEqual()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestEqual: " << static_cast<uint64_t>(value1) << " == " << static_cast<uint64_t>(value2) << " ? " <<
        std::endl;
#endif

    WriteSP<T>(value1 == value2 ? 1 : 0);
}


template <typename T>
void VirtualMachine::TestNotEqual()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestNotEqual: " << static_cast<uint64_t>(value1) << " != " << static_cast<uint64_t>(value2) << " ? "
        << std::endl;
#endif

    T result = value1 != value2 ? 1 : 0;
    WriteSP<T>(result);
}

template <typename T>
void VirtualMachine::TestLess()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestLess: " << static_cast<uint64_t>(value1) << " < " << static_cast<uint64_t>(value2) << " ? " <<
        std::endl;
#endif

    WriteSP<T>(value1 < value2 ? 1 : 0);
}

template <typename T>
void VirtualMachine::TestGreater()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestGreater: " << static_cast<uint64_t>(value1) << " > " << static_cast<uint64_t>(value2) << " ? " <<
        std::endl;
#endif
    WriteSP<T>(value1 > value2 ? 1 : 0);
}

template <typename T>
void VirtualMachine::TestGreaterEqual()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestGreaterEqual: " << static_cast<uint64_t>(value1) << " >= " << static_cast<uint64_t>(value2) <<
        " ? " << std::endl;
#endif
    WriteSP<T>(value1 >= value2 ? 1 : 0);
}


template <typename T>
void VirtualMachine::TestLessEqual()
{
    T value2 = ReadSP<T>();
    T value1 = ReadSP<T>();
#ifdef _DEBUG
    std::cout << "TestLessEqual: " << static_cast<uint64_t>(value1) << " <= " << static_cast<uint64_t>(value2) << " ? "
        << std::endl;
#endif
    WriteSP<T>(value1 <= value2 ? 1 : 0);
}

inline void VirtualMachine::CallRel()
{
    m_CallStack.push_back(m_PC);
    auto offset = ReadSP<uint64_t>();
    auto address = m_PC + offset;
    m_StackFrames.push_back(m_SP);
    m_CallTargets.push_back(address);
#ifdef _DEBUG
    if (m_SymbolServer->HasSymbol(address))
    {
        auto sym = m_SymbolServer->GetSymbol(address);
        std::cout << "CallRel: " << sym->SectionName << "::" << sym->SymbolName << std::endl;
    }
    else
    {
        std::cout << "CallRel: " << reinterpret_cast<uint64_t>(address) << std::endl;
    }
#endif
    m_PC = address;
}


inline void VirtualMachine::Call()
{
    m_CallStack.push_back(m_PC);
    auto address = ReadSP<uint8_t*>();
    m_StackFrames.push_back(m_SP);
    m_CallTargets.push_back(address);
#ifdef _DEBUG
    if (m_SymbolServer->HasSymbol(address))
    {
        auto sym = m_SymbolServer->GetSymbol(address);
        std::cout << "Call: " << sym->SectionName << "::" << sym->SymbolName << std::endl;
    }
    else
    {
        std::cout << "Call: " << reinterpret_cast<uint64_t>(address) << std::endl;
    }
#endif
    m_PC = address;
}


inline void VirtualMachine::Return()
{
    m_PC = m_CallStack[m_CallStack.size() - 1];
    m_CallStack.pop_back();
    m_StackFrames.pop_back();
    m_CallTargets.pop_back();
#ifdef _DEBUG
    if (!m_CallTargets.empty() && m_SymbolServer->HasSymbol(m_CallTargets[m_CallTargets.size() - 1]))
    {
        auto sym = m_SymbolServer->GetSymbol(m_CallTargets[m_CallTargets.size() - 1]);
        std::cout << "Return: " << sym->SectionName << "::" << sym->SymbolName << " + 0x" << reinterpret_cast<void*>(
            reinterpret_cast<uint64_t>(m_PC) - sym->SectionOffset) << std::endl;
    }
    else
    {
        std::cout << "Return: " << reinterpret_cast<void*>(m_PC) << std::endl;
    }
#endif
}

inline void VirtualMachine::Alloc()
{
    const auto size = ReadSP<uint32_t>();
    auto ptr = static_cast<uint8_t*>(malloc(size));
#ifdef _DEBUG
    std::cout << "Alloc: " << reinterpret_cast<uint64_t>(ptr) << " with size " << static_cast<uint64_t>(size) <<
        std::endl;
#endif
    WriteSP<uint8_t*>(ptr);
}

inline void VirtualMachine::Free()
{
    auto ptr = ReadSP<uint8_t*>();
#ifdef _DEBUG
    std::cout << "Free: " << reinterpret_cast<uint64_t>(ptr) << std::endl;
#endif
    free(ptr);
}


inline void VirtualMachine::Halt()
{
#ifdef _DEBUG
    std::cout << "Halt" << std::endl;
#endif

    m_Halted = true;
}

void VirtualMachine::Cycle()
{
    const auto instr = ReadPC<OpCodes>();
    switch (instr)
    {
    case Nop: break;
    case Push8:
        Push<uint8_t>();
        break;
    case Push16:
        Push<uint16_t>();
        break;
    case Push32:
        Push<uint32_t>();
        break;
    case Push64:
        Push<uint64_t>();
        break;
    case OpCodes::PushSF:
        PushSF();
        break;
    case OpCodes::MoveSP:
        MoveSP();
        break;
    case Pop8:
        Pop<uint8_t>();
        break;
    case Pop16:
        Pop<uint16_t>();
        break;
    case Pop32:
        Pop<uint32_t>();
        break;
    case Pop64:
        Pop<uint64_t>();
        break;
    case Dup8:
        Dup<uint8_t>();
        break;
    case Dup16:
        Dup<uint16_t>();
        break;
    case Dup32:
        Dup<uint32_t>();
        break;
    case Dup64:
        Dup<uint64_t>();
        break;
    case Swap8:
        Swap<uint8_t>();
        break;
    case Swap16:
        Swap<uint16_t>();
        break;
    case Swap32:
        Swap<uint32_t>();
        break;
    case Swap64:
        Swap<uint64_t>();
        break;
    case Add8:
        Add<uint8_t>();
        break;
    case Add16:
        Add<uint16_t>();
        break;
    case Add32:
        Add<uint32_t>();
        break;
    case Add64:
        Add<uint64_t>();
        break;
    case Sub8:
        Sub<uint8_t>();
        break;
    case Sub16:
        Sub<uint16_t>();
        break;
    case Sub32:
        Sub<uint32_t>();
        break;
    case Sub64:
        Sub<uint64_t>();
        break;
    case Mul8:
        Mul<uint8_t>();
        break;
    case Mul16:
        Mul<uint16_t>();
        break;
    case Mul32:
        Mul<uint32_t>();
        break;
    case Mul64:
        Mul<uint64_t>();
        break;
    case Div8:
        Div<uint8_t>();
        break;
    case Div16:
        Div<uint16_t>();
        break;
    case Div32:
        Div<uint32_t>();
        break;
    case Div64:
        Div<uint64_t>();
        break;
    case Mod8:
        Mod<uint8_t>();
        break;
    case Mod16:
        Mod<uint16_t>();
        break;
    case Mod32:
        Mod<uint32_t>();
        break;
    case Mod64:
        Mod<uint64_t>();
        break;
    case And8:
        And<uint8_t>();
        break;
    case And16:
        And<uint16_t>();
        break;
    case And32:
        And<uint32_t>();
        break;
    case And64:
        And<uint64_t>();
        break;
    case Or8:
        Or<uint8_t>();
        break;
    case Or16:
        Or<uint16_t>();
        break;
    case Or32:
        Or<uint32_t>();
        break;
    case Or64:
        Or<uint64_t>();
        break;
    case XOr8:
        XOr<uint8_t>();
        break;
    case XOr16:
        XOr<uint16_t>();
        break;
    case XOr32:
        XOr<uint32_t>();
        break;
    case XOr64:
        XOr<uint64_t>();
        break;
    case Not8:
        Not<uint8_t>();
        break;
    case Not16:
        Not<uint16_t>();
        break;
    case Not32:
        Not<uint32_t>();
        break;
    case Not64:
        Not<uint64_t>();
        break;
    case Shl8:
        Shl<uint8_t>();
        break;
    case Shl16:
        Shl<uint16_t>();
        break;
    case Shl32:
        Shl<uint32_t>();
        break;
    case Shl64:
        Shl<uint64_t>();
        break;
    case Shr8:
        Shr<uint8_t>();
        break;
    case Shr16:
        Shr<uint16_t>();
        break;
    case Shr32:
        Shr<uint32_t>();
        break;
    case Shr64:
        Shr<uint64_t>();
        break;
    case OpCodes::Jump:
        Jump();
        break;
    case OpCodes::JumpRel:
        JumpRel();
        break;
    case JumpZero8:
        JumpIfZero<uint8_t>();
        break;
    case JumpZero16:
        JumpIfZero<uint16_t>();
        break;
    case JumpZero32:
        JumpIfZero<uint32_t>();
        break;
    case JumpZero64:
        JumpIfZero<uint64_t>();
        break;
    case JumpNotZero8:
        JumpIfNotZero<uint8_t>();
        break;
    case JumpNotZero16:
        JumpIfNotZero<uint16_t>();
        break;
    case JumpNotZero32:
        JumpIfNotZero<uint32_t>();
        break;
    case JumpNotZero64:
        JumpIfNotZero<uint64_t>();
        break;
    case Load8:
        Load<uint8_t>();
        break;
    case Load16:
        Load<uint16_t>();
        break;
    case Load32:
        Load<uint32_t>();
        break;
    case Load64:
        Load<uint64_t>();
        break;
    case OpCodes::LoadN:
        LoadN();
        break;
    case Store8:
        Store<uint8_t>();
        break;
    case Store16:
        Store<uint16_t>();
        break;
    case Store32:
        Store<uint32_t>();
        break;
    case Store64:
        Store<uint64_t>();
        break;
    case OpCodes::StoreN:
        StoreN();
        break;
    case Assign8:
        Assign<uint8_t>();
        break;
    case Assign16:
        Assign<uint16_t>();
        break;
    case Assign32:
        Assign<uint32_t>();
        break;
    case Assign64:
        Assign<uint64_t>();
        break;
    case OpCodes::AssignN:
        AssignN();
        break;
    case OpCodes::Call:
        Call();
        break;
    case OpCodes::CallRel:
        CallRel();
        break;
    case OpCodes::Return:
        Return();
        break;
    case OpCodes::Alloc:
        Alloc();
        break;
    case OpCodes::Free:
        Free();
        break;
    case OpCodes::Halt:
        Halt();
        break;
    case WriteBus8:
        WriteBus<uint8_t>();
        break;
    case WriteBus16:
        WriteBus<uint16_t>();
        break;
    case WriteBus32:
        WriteBus<uint32_t>();
        break;
    case WriteBus64:
        WriteBus<uint64_t>();
        break;
    case ReadBus8:
        ReadBus<uint8_t>();
        break;
    case ReadBus16:
        ReadBus<uint16_t>();
        break;
    case ReadBus32:
        ReadBus<uint32_t>();
        break;
    case ReadBus64:
        ReadBus<uint64_t>();
        break;
    case OpCodes::Error:
        Error();
        break;
    case TestZero8:
        TestZero<uint8_t>();
        break;
    case TestZero16:
        TestZero<uint16_t>();
        break;
    case TestZero32:
        TestZero<uint32_t>();
        break;
    case TestZero64:
        TestZero<uint64_t>();
        break;
    case TestLT8:
        TestLess<uint8_t>();
        break;
    case TestLT16:
        TestLess<uint16_t>();
        break;
    case TestLT32:
        TestLess<uint32_t>();
        break;
    case TestLT64:
        TestLess<uint64_t>();
        break;
    case TestGT8:
        TestGreater<uint8_t>();
        break;
    case TestGT16:
        TestGreater<uint16_t>();
        break;
    case TestGT32:
        TestGreater<uint32_t>();
        break;
    case TestGT64:
        TestGreater<uint64_t>();
        break;
    case TestLE8:
        TestLessEqual<uint8_t>();
        break;
    case TestLE16:
        TestLessEqual<uint16_t>();
        break;
    case TestLE32:
        TestLessEqual<uint32_t>();
        break;
    case TestLE64:
        TestLessEqual<uint64_t>();
        break;
    case TestGE8:
        TestGreaterEqual<uint8_t>();
        break;
    case TestGE16:
        TestGreaterEqual<uint16_t>();
        break;
    case TestGE32:
        TestGreaterEqual<uint32_t>();
        break;
    case TestGE64:
        TestGreaterEqual<uint64_t>();
        break;
    case TestEq8:
        TestEqual<uint8_t>();
        break;
    case TestEq16:
        TestEqual<uint16_t>();
        break;
    case TestEq32:
        TestEqual<uint32_t>();
        break;
    case TestEq64:
        TestEqual<uint64_t>();
        break;
    case TestNEq8:
        TestNotEqual<uint8_t>();
        break;
    case TestNEq16:
        TestNotEqual<uint16_t>();
        break;
    case TestNEq32:
        TestNotEqual<uint32_t>();
        break;
    case TestNEq64:
        TestNotEqual<uint64_t>();
        break;
    default:
        throw std::invalid_argument("Invalid OpCode");
    }
}

void VirtualMachine::Run()
{
    while (!m_Halted)
    {
        Cycle();
    }
}
