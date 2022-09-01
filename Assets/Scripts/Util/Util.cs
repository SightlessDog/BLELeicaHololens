using System;
using System.Linq;
using System.Runtime.Serialization;
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
    
    public static string GetEnumMemberAttrValue(Type enumType, object enumVal)
    {
        var memInfo = enumType.GetMember(enumVal.ToString());
        var attr = memInfo[0].GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
        if(attr != null)
        {
            return attr.Value;
        }

        return null;
    }
}