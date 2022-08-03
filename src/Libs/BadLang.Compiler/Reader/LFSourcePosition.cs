namespace LF.Compiler.Reader;

public readonly struct LFSourcePosition : IEquatable<LFSourcePosition>
{
    public static LFSourcePosition Unknown = new LFSourcePosition();
    public readonly string Source;
    public readonly string File;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public string Content => Source.Substring(StartIndex, EndIndex - StartIndex - 1);
    public int Length => EndIndex - StartIndex;

    public LFSourcePosition(string source, string file, int startIndex, int endIndex)
    {
        if (startIndex > source.Length)
        {
            throw new ArgumentException("startIndex must be less than source.Length");
        }

        if (startIndex > endIndex)
        {
            endIndex = startIndex + 1;
        }


        Source = source;
        StartIndex = startIndex;
        EndIndex = endIndex;
        File = file;
    }

    public LFSourcePosition Combine(LFSourcePosition right)
    {
        if (right == Unknown)
        {
            return this;
        }

        if (this == Unknown)
        {
            return right;
        }

        return new LFSourcePosition(Source, File, StartIndex, right.EndIndex);
    }

    public override string ToString()
    {
        if (StartIndex >= Source.Length || EndIndex >= Source.Length)
        {
            return $"{File}:<invalid>";
        }

        int line = Source.Take(StartIndex).Count(x => x == '\n') + 1;
        int lastNewLine = Source.LastIndexOf('\n', StartIndex, StartIndex);
        int column;
        if (lastNewLine == -1)
        {
            column = StartIndex;
        }
        else
        {
            column = StartIndex - lastNewLine + 1;
        }

        return $"{File} ({line},{column})";
    }

    public bool Equals(LFSourcePosition other)
    {
        return Source == other.Source && File == other.File && StartIndex == other.StartIndex && EndIndex == other.EndIndex;
    }

    public override bool Equals(object? obj)
    {
        return obj is LFSourcePosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, File, StartIndex, EndIndex);
    }

    public static bool operator ==(LFSourcePosition left, LFSourcePosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LFSourcePosition left, LFSourcePosition right)
    {
        return !(left == right);
    }
}