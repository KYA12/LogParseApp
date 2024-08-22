using LogsShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace LogSearchApp.Pages
{
    public class LogViewerModel : PageModel
    {
        private readonly LogSearcher _logSearcher;
        public LogViewerModel(LogSearcher logSearcher)
        {
            _logSearcher = logSearcher;
        }

        public string FilePath { get; private set; }
        public int LineNumber { get; private set; }
        public string FileContent { get; private set; }
        public string HighlightedContent { get; private set; }
        public string RegexPattern { get; private set; }

        public async Task<IActionResult> OnGetAsync(string filePath, int lineNumber, string regexPattern)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            RegexPattern = Uri.UnescapeDataString(regexPattern);

            if (string.IsNullOrEmpty(FilePath) || LineNumber <= 0)
            {
                return NotFound();
            }

            try
            {
                FileContent = await System.IO.File.ReadAllTextAsync(FilePath);
                HighlightedContent = HighlightLine(FileContent, LineNumber, RegexPattern);
                return Page();
            }
            catch
            {
                return NotFound();
            }
        }
        private string HighlightLine(string content, int lineNumber, string regexPattern)
        {

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lineNumber <= 0 || lineNumber > lines.Length || string.IsNullOrEmpty(regexPattern))
            {
                return content;
            }

            var regex = new Regex(regexPattern);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i + 1 == lineNumber && regex.IsMatch(lines[i]))
                {
                    lines[i] = $"<span class='highlight'>{lines[i]}</span>";
                }
            }

            return string.Join("\n", lines);
        }
    }
}