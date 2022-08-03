using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LF.Compiler.Reader;

public abstract class LFCodeReader
{
    private readonly string m_FileName;

    protected LFCodeReader(string source, string workingDirectory, string fileName)
    {
        Source = source;
        WorkingDirectory = workingDirectory;
        m_FileName = fileName;
    }

    public string CurrentChar => Current().ToString();
    public int CurrentIndex { get; private set; }

    public string WorkingDirectory { get; }

    public string Source { get; private set; }

    public LFSourcePosition CreatePosition()
    {
        return new LFSourcePosition(Source, m_FileName, CurrentIndex, CurrentIndex + 1);
    }

    public void ReplaceCharacter(int index, char c)
    {
        Source = Source.Remove(index, 1).Insert(index, c.ToString());
    }

    public void SetIndex(int index)
    {
        CurrentIndex = index;
    }


    protected abstract void Read(LFReaderResult result);

    public void ReadToEnd(LFReaderResult result)
    {
        while (!IsEOF())
        {
            Read(result);
        }
    }

    public char Current(int offset = 0)
    {
        return IsEOF(offset) ? '\0' : Source[CurrentIndex + offset];
    }

    // ReSharper disable once InconsistentNaming
    public bool IsEOF(int offset = 0)
    {
        return CurrentIndex + offset >= Source.Length;
    }

    public bool Is(string s, int offset = 0)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (!Is(s[i], offset + i))
            {
                return false;
            }
        }

        return true;
    }
    public bool Is(char c, int offset = 0)
    {
        return !IsEOF(offset) && Current(offset) == c;
    }

    public void Eat(char c)
    {
        if (Is(c))
        {
            MoveNext();
        }
        else
        {
            throw new Exception($"Expected '{c}' at {CreatePosition()}");
        }
    }

    public void MoveNext(int count = 1)
    {
        CurrentIndex += count;
    }

    public void MovePrevious(int count = 1)
    {
        MoveNext(-count);
    }


    public void SkipToNewLine()
    {
        while (!IsEOF() && !Is('\n'))
        {
            MoveNext();
        }
    }

    public bool IsWhiteSpace(int offset = 0)
    {
        return !IsEOF(offset) && char.IsWhiteSpace(Current(offset));
    }

    public virtual bool IsWordStart(int offset = 0)
    {
        return !IsEOF(offset) &&
               (char.IsLetter(Current(offset)) ||
                Current(offset) == '_');
    }

    public virtual bool IsWordMiddle(int offset = 0)
    {
        return !IsEOF(offset) &&
               (char.IsLetterOrDigit(Current(offset)) ||
                Current(offset) == '_');
    }

    public bool IsDigit(int offset = 0)
    {
        return !IsEOF(offset) && char.IsDigit(Current(offset));
    }

    public void SkipWhiteSpace()
    {
        while (IsWhiteSpace())
        {
            MoveNext();
        }
    }

    public string ReadUntilWordOrWhiteSpace()
    {
        StringBuilder sb = new StringBuilder();
        while (!IsEOF() && !IsWhiteSpace() && !IsWordStart() && !IsDigit() && !Is(',') && !Is(';') && !Is(')') && !Is(']') && !Is('}') && !Is(':'))
        {
            sb.Append(Current());
            MoveNext();
        }

        return sb.ToString();
    }

    public ulong ReadNumber()
    {
        if (Is('\''))
        {
            Eat('\'');
            StringBuilder sb = new StringBuilder();
            while (!IsEOF() && !Is('\''))
            {
                sb.Append(CurrentChar);
                MoveNext();
            }

            Eat('\'');
            string chr = Regex.Unescape(sb.ToString());
            if (chr.Length != 1)
            {
                throw new Exception($"Invalid character literal at {CreatePosition()}");
            }

            return chr[0];
        }

        if (Is('0') && Is('x', 1))
        {
            Eat('0');
            Eat('x');
            StringBuilder sb = new StringBuilder();
            while (!IsEOF() && !IsWhiteSpace())
            {
                sb.Append(CurrentChar);
                MoveNext();
            }

            return ulong.Parse(sb.ToString(), NumberStyles.HexNumber);
        }

        return ReadDecimalNumber();
    }

    public ulong ReadDecimalNumber()
    {
        StringBuilder sb = new StringBuilder();

        while (IsDigit())
        {
            sb.Append(Current());
            MoveNext();
        }

        string str = sb.ToString();

        return ulong.Parse(str);
    }

    public string ReadWord()
    {
        if (!IsWordStart())
        {
            throw new Exception($"Expected word start at {CreatePosition()}");
        }

        StringBuilder sb = new StringBuilder(Current().ToString());
        MoveNext();
        while (IsWordMiddle())
        {
            sb.Append(Current());
            MoveNext();
        }

        return sb.ToString();
    }

    public string ReadString()
    {
        Eat('"');
        StringBuilder sb = new StringBuilder();
        while (!IsEOF() && !Is('"'))
        {
            sb.Append(Current());
            MoveNext();
        }

        Eat('"');

        return sb.ToString();
    }
}