using BadVM.Core;
using BadVM.Interop.Internal;
using BadVM.Shared.Memory;

namespace BadVM.Interop;

public class InteropFunction < TRet > : InnerInteropFunction
{

    private readonly Func < TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return $"{InteropHelper.ConvertType < TRet >()} {Name}()";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        WriteReturnValue( core, bus, m_Function()! );
    }

    #endregion

}

public class InteropFunction < TArg0, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        WriteReturnValue( core, bus, m_Function( ReadArgumentValue < TArg0 >( core, bus ) )! );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TArg1, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TArg1, TArg2, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TArg1, TArg2, TArg3, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TArg1, TArg2, TArg3, TArg4, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TRet > m_Function;

    #region Public

    public InteropFunction( string name, Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5)";
    }

    public override void Invoke( VirtualCore core, MemoryBus bus )
    {
        TArg5 arg5 = ReadArgumentValue < TArg5 >( core, bus );
        TArg4 arg4 = ReadArgumentValue < TArg4 >( core, bus );
        TArg3 arg3 = ReadArgumentValue < TArg3 >( core, bus );
        TArg2 arg2 = ReadArgumentValue < TArg2 >( core, bus );
        TArg1 arg1 = ReadArgumentValue < TArg1 >( core, bus );
        TArg0 arg0 = ReadArgumentValue < TArg0 >( core, bus );

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4,
                                    arg5
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4,
                                    arg5,
                                    arg6
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4,
                                    arg5,
                                    arg6,
                                    arg7
                                   )!
                        );
    }

    #endregion

}

public class
    InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4,
                                    arg5,
                                    arg6,
                                    arg7,
                                    arg8
                                   )!
                        );
    }

    #endregion

}

public class
    InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9,
                      TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TRet > func ) : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
                                    arg0,
                                    arg1,
                                    arg2,
                                    arg3,
                                    arg4,
                                    arg5,
                                    arg6,
                                    arg7,
                                    arg8,
                                    arg9
                                   )!
                        );
    }

    #endregion

}

public class
    InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10,
                      TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TRet >
        m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TRet > func ) :
        base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                    arg10
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10,
                               TArg11, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
        TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TRet > func ) :
        base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                    arg11
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                               TArg12, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12
       ,
        TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TRet >
            func ) :
        base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                    arg12
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                               TArg12, TArg13, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12
      , TArg13, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13,
            TRet > func )
        : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                    arg13
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                               TArg12, TArg13, TArg14, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12
      , TArg13, TArg14, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13,
            TArg14, TRet > func )
        : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13, {InteropHelper.ConvertType < TArg14 >()} arg14)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                   )!
                        );
    }

    #endregion

}

public class InteropFunction < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11,
                               TArg12, TArg13, TArg14, TArg15, TRet > : InnerInteropFunction
{

    private readonly Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12
      , TArg13, TArg14, TArg15, TRet > m_Function;

    #region Public

    public InteropFunction(
        string name,
        Func < TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13,
            TArg14, TArg15, TRet > func )
        : base( name )
    {
        m_Function = func ?? throw new ArgumentNullException( nameof( func ) );
    }

    public override string GetSignature()
    {
        return
            $"{InteropHelper.ConvertType < TRet >()} {Name}({InteropHelper.ConvertType < TArg0 >()} arg0, {InteropHelper.ConvertType < TArg1 >()} arg1, {InteropHelper.ConvertType < TArg2 >()} arg2, {InteropHelper.ConvertType < TArg3 >()} arg3, {InteropHelper.ConvertType < TArg4 >()} arg4, {InteropHelper.ConvertType < TArg5 >()} arg5, {InteropHelper.ConvertType < TArg6 >()} arg6, {InteropHelper.ConvertType < TArg7 >()} arg7, {InteropHelper.ConvertType < TArg8 >()} arg8, {InteropHelper.ConvertType < TArg9 >()} arg9, {InteropHelper.ConvertType < TArg10 >()} arg10, {InteropHelper.ConvertType < TArg11 >()} arg11, {InteropHelper.ConvertType < TArg12 >()} arg12, {InteropHelper.ConvertType < TArg13 >()} arg13, {InteropHelper.ConvertType < TArg14 >()} arg14, {InteropHelper.ConvertType < TArg15 >()} arg15)";
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

        WriteReturnValue(
                         core,
                         bus,
                         m_Function(
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
                                   )!
                        );
    }

    #endregion

}
