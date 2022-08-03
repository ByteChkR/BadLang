using System.Text;

using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Code.Instruction;
using LF.Compiler.Reader;

namespace LF.Compiler.Assembly;

public class LFAssemblyReader : LFCodeReader
{
    public LFAssemblyReader(string source, string workingDir, string fileName) : base(source, workingDir, fileName) { }

    private void SkipNonToken()
    {
        while (!IsEOF())
        {
            if (Is(';'))
            {
                SkipToNewLine();
            }
            else if (IsWhiteSpace())
            {
                SkipWhiteSpace();
            }
            else
            {
                break;
            }
        }
    }


    private LFAssemblyInstructionArgument ReadArgument()
    {
        if (IsWordStart())
        {
            List<string> parts = new List<string> { ReadWord() };

            while (Is(':') && Is(':', 1))
            {
                Eat(':');
                Eat(':');
                parts.Add(ReadWord());
            }

            return new LFLiteralInstructionArgument(new LFAssemblySymbol(parts.ToArray()));
        }

        ulong value = ReadNumber();

        return new LFValueInstructionArgument(value);
    }


    private void ReadDataSection(LFReaderDataSection section)
    {
        SkipNonToken();

        if (Is('i'))
        {
            LFSourcePosition typeStart = CreatePosition();
            Eat('i');
            ulong size = ReadNumber();
            SkipNonToken();
            string name = ReadWord();
            SkipNonToken();
            ulong value = ReadNumber();
            SkipNonToken();

            byte[] data;
            switch (size)
            {
                case 8:
                    data = new[] { (byte)value };

                    break;
                case 16:
                    data = BitConverter.GetBytes((ushort)value);

                    break;
                case 32:
                    data = BitConverter.GetBytes((uint)value);

                    break;
                case 64:
                    data = BitConverter.GetBytes(value);

                    break;
                default:
                    throw new Exception($"Invalid data type: i{size} at {typeStart.Combine(CreatePosition())}");
            }

            section.EmitData(name, data);

            return;
        }

        LFSourcePosition pos = CreatePosition();
        string key = ReadWord();
        if (key == "cstr")
        {
            SkipNonToken();
            string name = ReadWord();
            SkipNonToken();
            string data = $"{ReadString()}\0";
            SkipNonToken();
            section.EmitData(name, Encoding.UTF8.GetBytes(data));
        }
        else
        {
            throw new Exception($"Invalid Data Section key: '{key}' at {pos.Combine(CreatePosition())}");
        }
    }


    private void ReadCodeSection(LFReaderCodeSection section)
    {
        if (Is('.'))
        {
            Eat('.');
            LFSourcePosition pos = CreatePosition();
            string key = ReadWord();
            SkipNonToken();
            if (key == "export")
            {
                string name = ReadWord();
                section.AddExport(name);
            }
            else if (key == "import")
            {
                string name = ReadWord();
                section.AddImport(name);
            }
            else
            {
                throw new Exception($"Unknown code section key '{key}' at {pos.Combine(CreatePosition())}");
            }
        }
        else
        {
            LFSourcePosition pos = CreatePosition();
            string word = ReadWord();
            if (Is(':'))
            {
                Eat(':');
                section.CreateLabel(word, pos.Combine(CreatePosition()));
            }
            else
            {
                section.Emit(ReadInstruction(word, pos));
            }
        }

        SkipNonToken();
    }


    protected override void Read(LFReaderResult result)
    {
        SkipNonToken();
        Eat('.');
        string key = ReadWord();

        if (key == "code")
        {
            SkipNonToken();
            string name = ReadWord();
            SkipNonToken();
            Eat('{');
            SkipNonToken();
            LFReaderCodeSection section = result.CreateCodeSection(name);
            while (!Is('}'))
            {
                ReadCodeSection(section);
                SkipNonToken();
            }

            Eat('}');
        }
        else if (key == "data")
        {
            SkipNonToken();
            string name = ReadWord();
            SkipNonToken();
            Eat('{');
            SkipNonToken();
            LFReaderDataSection section = result.CreateDataSection(name);

            while (!Is('}'))
            {
                ReadDataSection(section);
            }

            Eat('}');
        }
        else if (key == "include")
        {
            SkipNonToken();
            string file = ReadString();

            if (!Path.IsPathRooted(file))
            {
                file = Path.Combine(WorkingDirectory, file);
            }

            result.AddInclude(file);
            SkipNonToken();
        }
        else if (key == "require")
        {
            SkipNonToken();
            if (Is('"'))
            {
                string file = ReadString();
                result.AddFileRequire(file);
            }
            else if (Is('<'))
            {
                Eat('<');
                SkipNonToken();
                string name = ReadWord();
                SkipNonToken();
                Eat('>');
                SkipNonToken();
                result.AddFileRequire(CoreLibraryHelper.GetCoreLibrary(name));
            }
            else
            {
                string name = ReadWord();
                result.AddNameRequire(name);
            }
        }
        else
        {
            throw new Exception($"Unknown Section Type: {key} at {CreatePosition()}");
        }

        SkipNonToken();
    }

    private LFAssemblyInstruction ReadInstruction(string word, LFSourcePosition position)
    {
        OpCodes opCode = Enum.Parse<OpCodes>(word, true);

        int argC = LFAssemblyInstructionInfo.GetArgumentCount(opCode);

        List<LFAssemblyInstructionArgument> args = new List<LFAssemblyInstructionArgument>();
        for (int i = 0; i < argC; i++)
        {
            SkipWhiteSpace();
            args.Add(ReadArgument());
        }

        return new LFAssemblyInstruction(position.Combine(CreatePosition()), opCode, args.ToArray());
    }
}