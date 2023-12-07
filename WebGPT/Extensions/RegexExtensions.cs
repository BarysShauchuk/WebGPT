using System.Text.RegularExpressions;

namespace WebGPT.Extensions
{
    public static partial class RegexExtensions
    {
        public static string ReplaceRegex(this string input, Regex regex, string replacement = " ")
        {
            return regex.Replace(input, replacement);
        }

        private const RegexOptions defaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        [GeneratedRegex(@"<\s*body[^>]*>(.*?)<\s*/\s*body\s*>", defaultRegexOptions)]
        public static partial Regex BodyRegex();


        [GeneratedRegex(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", defaultRegexOptions)]
        public static partial Regex ScriptRegex();


        [GeneratedRegex(@"<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>", defaultRegexOptions)]
        public static partial Regex StyleRegex();


        [GeneratedRegex(@"<[^>]+>", defaultRegexOptions)]
        public static partial Regex HtmlTagsRegex();


        [GeneratedRegex(@"\s+")]
        public static partial Regex WhiteSpaceRegex();
    }
}
