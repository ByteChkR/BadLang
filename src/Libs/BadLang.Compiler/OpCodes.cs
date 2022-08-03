// ReSharper disable InconsistentNaming

namespace LF.Compiler;

public enum OpCodes
{
    Nop = 0x0000,
    Halt = 0x0001,


    // Stack

    Push8 = 0x1000,
    Push16 = 0x1001,
    Push32 = 0x1002,
    Push64 = 0x1003,
    PushSF = 0x1004,
    MoveSP = 0x1005,

    Pop8 = 0x1100,
    Pop16 = 0x1101,
    Pop32 = 0x1102,
    Pop64 = 0x1103,

    Dup8 = 0x1200,
    Dup16 = 0x1201,
    Dup32 = 0x1202,
    Dup64 = 0x1203,

    Swap8 = 0x1300,
    Swap16 = 0x1301,
    Swap32 = 0x1302,
    Swap64 = 0x1303,


    // Math

    Add8 = 0x2000,
    Add16 = 0x2001,
    Add32 = 0x2002,
    Add64 = 0x2003,

    Sub8 = 0x2100,
    Sub16 = 0x2101,
    Sub32 = 0x2102,
    Sub64 = 0x2103,

    Mul8 = 0x2200,
    Mul16 = 0x2201,
    Mul32 = 0x2202,
    Mul64 = 0x2203,

    Div8 = 0x2300,
    Div16 = 0x2301,
    Div32 = 0x2302,
    Div64 = 0x2303,

    Mod8 = 0x2400,
    Mod16 = 0x2401,
    Mod32 = 0x2402,
    Mod64 = 0x2403,


    // Logic

    And8 = 0x3000,
    And16 = 0x3001,
    And32 = 0x3002,
    And64 = 0x3003,

    Or8 = 0x3100,
    Or16 = 0x3101,
    Or32 = 0x3102,
    Or64 = 0x3103,

    XOr8 = 0x3200,
    XOr16 = 0x3201,
    XOr32 = 0x3202,
    XOr64 = 0x3203,

    Not8 = 0x3300,
    Not16 = 0x3301,
    Not32 = 0x3302,
    Not64 = 0x3303,

    Shl8 = 0x3400,
    Shl16 = 0x3401,
    Shl32 = 0x3402,
    Shl64 = 0x3403,

    Shr8 = 0x3500,
    Shr16 = 0x3501,
    Shr32 = 0x3502,
    Shr64 = 0x3503,


    // Branching

    Jump = 0x4000,

    JumpZero8 = 0x4001,
    JumpZero16 = 0x4002,
    JumpZero32 = 0x4003,
    JumpZero64 = 0x4004,

    JumpNotZero8 = 0x4005,
    JumpNotZero16 = 0x4006,
    JumpNotZero32 = 0x4007,
    JumpNotZero64 = 0x4008,
    JumpRel = 0x4009,

    Call = 0x4100,
    Return = 0x4101,
    CallRel = 0x4102,


    // Memory

    Load8 = 0x5000,
    Load16 = 0x5001,
    Load32 = 0x5002,
    Load64 = 0x5003,
    LoadN = 0x5004,

    Store8 = 0x5100,
    Store16 = 0x5101,
    Store32 = 0x5102,
    Store64 = 0x5103,
    Assign8 = 0x5104,
    Assign16 = 0x5105,
    Assign32 = 0x5106,
    Assign64 = 0x5107,
    StoreN = 0x5108,
    AssignN = 0x5109,

    Alloc = 0x5200,
    Free = 0x5201,

    WriteBus8 = 0x5300,
    WriteBus16 = 0x5301,
    WriteBus32 = 0x5302,
    WriteBus64 = 0x5303,

    ReadBus8 = 0x5304,
    ReadBus16 = 0x5305,
    ReadBus32 = 0x5306,
    ReadBus64 = 0x5307,


    TestZero8 = 0x5400,
    TestZero16 = 0x5401,
    TestZero32 = 0x5402,
    TestZero64 = 0x5403,

    TestLT8 = 0x5500,
    TestLT16 = 0x5501,
    TestLT32 = 0x5502,
    TestLT64 = 0x5503,

    TestGT8 = 0x5600,
    TestGT16 = 0x5601,
    TestGT32 = 0x5602,
    TestGT64 = 0x5603,

    TestLE8 = 0x5700,
    TestLE16 = 0x5701,
    TestLE32 = 0x5702,
    TestLE64 = 0x5703,

    TestGE8 = 0x5800,
    TestGE16 = 0x5801,
    TestGE32 = 0x5802,
    TestGE64 = 0x5803,

    TestEq8 = 0x5900,
    TestEq16 = 0x5901,
    TestEq32 = 0x5902,
    TestEq64 = 0x5903,

    TestNEq8 = 0x5A00,
    TestNEq16 = 0x5A01,
    TestNEq32 = 0x5A02,
    TestNEq64 = 0x5A03,

    Error = 0xFFFF,
}