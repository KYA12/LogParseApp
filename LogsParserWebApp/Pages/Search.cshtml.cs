using LogsShared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogSearchApp.Pages
{
    public class SearchModel : PageModel
    {
        private readonly LogSearcher _logSearcher;
        private readonly IConfiguration _configuration;

        public SearchModel(LogSearcher logSearcher, IConfiguration configuration)
        {
            _logSearcher = logSearcher;
            _configuration = configuration;
        }

        [BindProperty]
        public string SearchPattern { get; set; }
        public string RegexPattern { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public List<(string FilePath, string Line, int LineNumber)> MatchedLines { get; private set; }

        public async Task OnPostAsync()
        {
            HasError = false;
            ErrorMessage = null;

            if (string.IsNullOrEmpty(SearchPattern))
            {
                HasError = true;
                ErrorMessage = "Please enter a search pattern.";
                return;
            }

            try
            {
                RegexPattern = PatternConverter.ConvertToRegexIfNeeded(SearchPattern);

                if (!PatternConverter.IsValidRegexPattern(RegexPattern))
                {
                    HasError = true;
                    ErrorMessage = "Invalid regex pattern.";
                    return;
                }

                var directoryPath = _configuration["LogDirectoryPath"];
                var bufferSize = int.Parse(_configuration["BufferSize"]);
                MatchedLines = await _logSearcher.SearchLogsAsync(directoryPath, RegexPattern, bufferSize);
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }
    }
}