using System.Text.RegularExpressions;

namespace LogsShared
{
    public static class PatternConverter
    {
        public static string ConvertWildcardToRegex(string wildcardPattern)
        {
            if (wildcardPattern == null) throw new ArgumentNullException(nameof(wildcardPattern));

            string regexPattern = Regex.Escape(wildcardPattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".")
                .Replace("\\", "");

            regexPattern = ConvertAndToLookaheads(regexPattern);

            regexPattern = regexPattern
                .Replace(" or ", "|")
                .Replace("\\(", "(")
                .Replace("\\)", ")");

            regexPattern = OptimizeRegexPattern(regexPattern);
            return regexPattern;
        }

        private static string ConvertAndToLookaheads(string pattern)
        {
            var parts = pattern.Split(new[] { " and " }, StringSplitOptions.None);

            if (parts.Length == 1)
            {
                return pattern;
            }

            var lookaheads = parts
                .Select(part => $"(?=.*{part.Trim()})")
                .ToArray();

            return string.Join(string.Empty, lookaheads);
        }

        private static string OptimizeRegexPattern(string pattern)
        {
            return Regex.Replace(pattern, @"(\.\*)(?=\.\*)", "");
        }

        public static string ConvertToRegexIfNeeded(string searchPattern)
        {
            return IsValidRegexPattern(searchPattern)
                ? searchPattern
                : ConvertWildcardToRegex(searchPattern);
        }

        public static bool IsValidRegexPattern(string pattern)
        {
            try
            {
                Regex regex = new Regex(pattern);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}