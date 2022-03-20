using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat.Serialization.Sections.Formats.Writers;

namespace BadC.Functions
{

    public static class FunctionEpilogueWriter
    {

        #region Public

        public static void WriteEpilouge( this FunctionInfo func, CodeSectionWriter writer )
        {
            int popBytes = 0;

            if ( func.ParameterSize.Length == 0 ) // No arguments
            {
                if ( func.LocalSize.Length == 0 ) // No Locals
                {
                    //Do not pop values. Return is already on top and there should be no values on the stack except it
                }
                else
                {
                    if ( func.ReturnType.Size != 0 )
                    {
                        popBytes = func.TotalLocalSize - func.ReturnType.Size;

                        WriteMovSF( writer, func.ReturnType.Size, func.ReturnType.Size );

                        //Move Return to local0
                        //Calculate extra bytes that need to be popped.
                    }
                    else
                    {
                        //Do nothing, Return is already on top
                    }
                }
            }
            else
            {
                popBytes = func.TotalParameterSize + func.TotalLocalSize - func.ReturnType.Size;

                if ( func.ReturnType.Size != 0 )
                {
                    int offset = func.TotalParameterSize - func.ReturnType.Size;
                    WriteMovSF( writer, offset, func.ReturnType.Size );

                    //Calculate extra bytes that need to be popped.
                }
            }

            if ( popBytes < 0 )
            {
                writer.Emit( OpCode.MovSP, BitConverter.GetBytes( -popBytes ) );
            }
            else
            {
                writer.Pop( popBytes );
            }

            writer.Emit( OpCode.Return, Array.Empty < byte >() );
        }

        #endregion

        #region Private

        private static void WriteMovSF( CodeSectionWriter writer, int off, int size )
        {
            if ( size == 1 )
            {
                writer.Emit( OpCode.MovSFI8, BitConverter.GetBytes( off ) );
            }
            else if ( size == 2 )
            {
                writer.Emit( OpCode.MovSFI16, BitConverter.GetBytes( off ) );
            }
            else if ( size == 4 )
            {
                writer.Emit( OpCode.MovSFI32, BitConverter.GetBytes( off ) );
            }
            else if ( size == 8 )
            {
                writer.Emit( OpCode.MovSFI64, BitConverter.GetBytes( off ) );
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion

    }

}
