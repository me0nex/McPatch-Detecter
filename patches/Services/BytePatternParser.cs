namespace patches.Services;

public static class BytePatternParser
{
    public static byte?[] ParsePattern(string pattern)
    {
        return pattern
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token is "?" or "??" ? (byte?)null : Convert.ToByte(token, 16))
            .ToArray();
    }

    public static byte[] ParseExactBytes(string bytes)
    {
        return bytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => Convert.ToByte(token, 16))
            .ToArray();
    }
}
