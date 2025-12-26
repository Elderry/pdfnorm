namespace PdfNorm.Services;

public static class TextUtils
{
    private static readonly char[] TrimChars = [' ', '\r', '\n'];

    public static bool CanBeTrimmed(string target)
    {
        return target != target.Trim(TrimChars);
    }

    public static string Trim(string target)
    {
        return target.Trim(TrimChars);
    }

    public static string EscapeEOL(string target)
    {
        return target.Replace("\r", "\\r").Replace("\n", "\\n");
    }
}
