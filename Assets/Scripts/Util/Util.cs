using System.Text.RegularExpressions;

public class Util
{
    public static string RemoveWhiteChars(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string text = string.Join(string.Empty, Regex.Split(value, "(?:\\r\\n|\\n|\\r|\\t)"));
        return text.Trim();
    }
}