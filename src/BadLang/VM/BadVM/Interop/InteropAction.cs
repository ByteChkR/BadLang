using BadVM.Core;
using BadVM.Interop.Internal;
using BadVM.Shared.Memory;

namespace BadVM.Interop;

public class InteropAction : InnerInteropFunction
{

    private readonly Action m_Action;

    #region Public

    public InteropAction( string name, Action action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return $"void {Name}()";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        m_Action?.Invoke();
    }

    #endregion

}

public class InteropAction < TArg0 > : InnerInteropFunction
{

    private readonly Action < TArg0 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0, TArg1 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0, TArg1, TArg2 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0, TArg1, TArg2, TArg3 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0, TArg1, TArg2, TArg3, TArg4 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5 > m_Action;

    #region Public

    public InteropAction( string name, Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 );
    }

    #endregion

}

public class
    InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 );
    }

    #endregion

}

public class
    InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
                    TArg10 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 );
    }

    #endregion

}

public class
    InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10,
                    TArg11 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11 >
        m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11 > action ) :
        base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg11 arg11 = ReadArgumentValue < TArg11 >( core, bus );
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                             TArg12 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
        TArg12 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12 > action )
        : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg12 arg12 = ReadArgumentValue < TArg12 >( core, bus );
        TArg11 arg11 = ReadArgumentValue < TArg11 >( core, bus );
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                             TArg12, TArg13 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
        TArg12, TArg13 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12,
            TArg13 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg13 arg13 = ReadArgumentValue < TArg13 >( core, bus );
        TArg12 arg12 = ReadArgumentValue < TArg12 >( core, bus );
        TArg11 arg11 = ReadArgumentValue < TArg11 >( core, bus );
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );
        m_Action?.Invoke( arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                             TArg12, TArg13, TArg14 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
        TArg12, TArg13, TArg14 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12,
            TArg13, TArg14 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13, {InteropHelper.ConvertType < TArg14 >()} arg14)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg14 arg14 = ReadArgumentValue < TArg14 >( core, bus );
        TArg13 arg13 = ReadArgumentValue < TArg13 >( core, bus );
        TArg12 arg12 = ReadArgumentValue < TArg12 >( core, bus );
        TArg11 arg11 = ReadArgumentValue < TArg11 >( core, bus );
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        m_Action?.Invoke(
                         arg0,
                         arg1,
                         arg2,
                         arg3,
                         arg4,
                         arg5,
                         arg6,
                         arg7,
                         arg8,
                         arg9,
                         arg10,
                         arg11,
                         arg12,
                         arg13,
                         arg14
                        );
    }

    #endregion

}

public class InteropAction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                             TArg12, TArg13, TArg14, TArg15 > : InnerInteropFunction
{

    private readonly Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
        TArg12, TArg13, TArg14, TArg15 > m_Action;

    #region Public

    public InteropAction(
        string name,
        Action < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12,
            TArg13, TArg14, TArg15 > action ) : base( name )
    {
        m_Action = action;
    }

    public override string GetSignature()
    {
        return
            $"void {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13, {InteropHelper.ConvertType < TArg14 >()} arg14, {InteropHelper.ConvertType < TArg15 >()} arg15)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg15 arg15 = ReadArgumentValue < TArg15 >( core, bus );
        TArg14 arg14 = ReadArgumentValue < TArg14 >( core, bus );
        TArg13 arg13 = ReadArgumentValue < TArg13 >( core, bus );
        TArg12 arg12 = ReadArgumentValue < TArg12 >( core, bus );
        TArg11 arg11 = ReadArgumentValue < TArg11 >( core, bus );
        TArg10 arg10 = ReadArgumentValue < TArg10 >( core, bus );
        TArg9 arg9 = ReadArgumentValue < TArg9 >( core, bus );
        TArg8 arg8 = ReadArgumentValue < TArg8 >( core, bus );
        TArg7 arg7 = ReadArgumentValue < TArg7 >( core, bus );
        TArg6 arg6 = ReadArgumentValue < TArg6 >( core, bus );
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        m_Action?.Invoke(
                         arg0,
                         arg1,
                         arg2,
                         arg3,
                         arg4,
                         arg5,
                         arg6,
                         arg7,
                         arg8,
                         arg9,
                         arg10,
                         arg11,
                         arg12,
                         arg13,
                         arg14,
                         arg15
                        );
    }

    #endregion

}
