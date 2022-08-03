namespace LF.Compiler;

public static class StreamExtensions
{
    public static ulong ReadUInt64(this Stream stream)
    {
        byte[] bytes = new byte[8];
        int read = stream.Read(bytes, 0, 8);

        if (read != bytes.Length)
        {
            throw new Exception("Stream read error");
        }

        return BitConverter.ToUInt64(bytes, 0);
    }

    public static int ReadInt32(this Stream stream)
    {
        byte[] bytes = new byte[4];
        int read = stream.Read(bytes, 0, 4);

        if (read != bytes.Length)
        {
            throw new Exception("Stream read error");
        }

        return BitConverter.ToInt32(bytes, 0);
    }

    public static uint ReadUInt32(this Stream stream)
    {
        byte[] bytes = new byte[4];
        int read = stream.Read(bytes, 0, 4);

        if (read != bytes.Length)
        {
            throw new Exception("Stream read error");
        }

        return BitConverter.ToUInt32(bytes, 0);
    }
}