
public static class StringExtension
{
    // Helper for repeating strings
    public static string Repeat_BES(this string s, int n)
    {
        if (n <= 0) return "";
        return string.Concat(Enumerable.Repeat(s, n));
    }
}