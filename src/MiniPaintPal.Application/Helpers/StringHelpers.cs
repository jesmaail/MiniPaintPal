using System.Text.RegularExpressions;
namespace MiniPaintPal.Application.Helpers;

public static class StringHelpers
{
    public static string[] SplitCamelCase(this string source, char separator)
        => Regex.Replace(source, "([A-Z])", " $1", RegexOptions.Compiled)
                .Trim()
                .Split(separator);
}
