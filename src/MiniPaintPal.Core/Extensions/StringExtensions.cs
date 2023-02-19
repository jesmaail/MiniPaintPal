using System.Text.RegularExpressions;

namespace MiniPaintPal.Core.Extensions;

public static class StringExtensions
{
    public static string[] SplitCamelCase(this string source, char separator)
        => Regex.Replace(source, "([A-Z])", " $1", RegexOptions.Compiled)
                .Trim()
                .Split(separator);


    public static string ConvertToCapitalStartChar(this string source)
        => string.Concat(char.ToUpper(source[0]), source[1..]);
}
