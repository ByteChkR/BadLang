namespace BadVM.Shared
{

    public enum OpCode
    {

        //Misc.
        Nop,
        Halt,
        InitCore,
        AbortCore,
        CoreStatus,
        CoreCount,

        //Stack
        Pop64,
        Pop32,
        Pop16,
        Pop8,

        PushI64,
        PushI32,
        PushI16,
        PushI8,
        PushSP,
        PushSF,

        MovSFI8,
        MovSFI16,
        MovSFI32,
        MovSFI64,
        MovSP,

        LoadI64,
        LoadI32,
        LoadI16,
        LoadI8,
        LoadN,

        StoreI64,
        StoreI32,
        StoreI16,
        StoreI8,
        StoreN,

        DupI8,
        DupI16,
        DupI32,
        DupI64,

        SwapI8,
        SwapI16,
        SwapI32,
        SwapI64,

        //Arithmetic
        AddI8,
        AddI16,
        AddI32,
        AddI64,

        SubI8,
        SubI16,
        SubI32,
        SubI64,

        MulI8,
        MulI16,
        MulI32,
        MulI64,

        DivI8,
        DivI16,
        DivI32,
        DivI64,

        ModI8,
        ModI16,
        ModI32,
        ModI64,

        AndI8,
        AndI16,
        AndI32,
        AndI64,

        OrI8,
        OrI16,
        OrI32,
        OrI64,

        XorI8,
        XorI16,
        XorI32,
        XorI64,

        NotI8,
        NotI16,
        NotI32,
        NotI64,

        ShlI8,
        ShlI16,
        ShlI32,
        ShlI64,

        ShrI8,
        ShrI16,
        ShrI32,
        ShrI64,

        //Control Flow

        Call,
        Jump,
        Return,

        //Branches

        BranchZeroI8,
        BranchZeroI16,
        BranchZeroI32,
        BranchZeroI64,

        BranchNotZeroI8,
        BranchNotZeroI16,
        BranchNotZeroI32,
        BranchNotZeroI64,

        BranchLessI8,
        BranchLessI16,
        BranchLessI32,
        BranchLessI64,

        BranchGreaterI8,
        BranchGreaterI16,
        BranchGreaterI32,
        BranchGreaterI64,

        BranchLessOrEqualI8,
        BranchLessOrEqualI16,
        BranchLessOrEqualI32,
        BranchLessOrEqualI64,

        BranchGreaterOrEqualI8,
        BranchGreaterOrEqualI16,
        BranchGreaterOrEqualI32,
        BranchGreaterOrEqualI64,

        InteropResolve,
        InteropCall,

    }

}
