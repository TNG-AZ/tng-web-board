using System.Text.RegularExpressions;

namespace TNG.Web.Board.Utilities
{
    public static class StringTools
    {
        public static string RemoveHtml(string input)
            => Regex.Replace(input, "<.*?>", string.Empty);
    }
}
