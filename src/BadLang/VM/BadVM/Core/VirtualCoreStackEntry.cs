namespace BadVM.Core;

public readonly struct VirtualCoreStackEntry
{

    public readonly long ReturnAddress;
    public readonly long StackFrameAddress;

    public VirtualCoreStackEntry( long returnAddress, long stackFrameAddress )
    {
        ReturnAddress = returnAddress;
        StackFrameAddress = stackFrameAddress;
    }

}
