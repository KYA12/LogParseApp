using System.Text;
using System.Text.RegularExpressions;

namespace LogsShared
{
    public class LogSearcher
    {
        public async Task<List<(string FilePath, string Line, int LineNumber)>> SearchLogsAsync(string directoryPath, string regexPattern, int bufferSize)
        {
            var matchedLines = new List<(string FilePath, string Line, int LineNumber)>();
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var tasks = new List<Task>();

            foreach (var file in files)
            {
                tasks.Add(Task.Run(() => ProcessFileAsync(file, regex, bufferSize, matchedLines)));
            }

            await Task.WhenAll(tasks);

            return matchedLines;
        }

        private async Task ProcessFileAsync(string filePath, Regex regex, int bufferSize, List<(string FilePath, string Line, int LineNumber)> matchedLines)
        {
            int lineNumber = 0;
            var buffer = new char[bufferSize];
            StringBuilder lineBuilder = new StringBuilder();

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                using (var bufferedStream = new BufferedStream(fileStream, bufferSize))
                using (var reader = new StreamReader(bufferedStream))
                {
                    int bytesRead;
                    while ((bytesRead = await reader.ReadAsync(buffer, 0, bufferSize)) > 0)
                    {
                        ProcessBuffer(buffer, bytesRead, ref lineBuilder, ref lineNumber, filePath, regex, matchedLines);
                    }

                    if (lineBuilder.Length > 0)
                    {
                        ProcessLine(lineBuilder.ToString(), filePath, regex, matchedLines, ref lineNumber);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
            }
        }

        private void ProcessBuffer(char[] buffer, int bytesRead, ref StringBuilder lineBuilder, ref int lineNumber, string filePath, Regex regex, List<(string FilePath, string Line, int LineNumber)> matchedLines)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                char c = buffer[i];
                if (c == '\r' || c == '\n')
                {
                    if (lineBuilder.Length > 0)
                    {
                        var line = lineBuilder.ToString();
                        ProcessLine(line, filePath, regex, matchedLines, ref lineNumber);
                        lineBuilder.Clear();
                    }

                    if (c == '\r' && i + 1 < bytesRead && buffer[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    lineBuilder.Append(c);
                }
            }

            if (lineBuilder.Length > 0)
            {
                var line = lineBuilder.ToString();
                ProcessLine(line, filePath, regex, matchedLines, ref lineNumber);
                lineBuilder.Clear();
            }
        }

        private void ProcessLine(string line, string filePath, Regex regex, List<(string FilePath, string Line, int LineNumber)> matchedLines, ref int lineNumber)
        {
            lineNumber++;
          
            if (regex.IsMatch(line))
            {
                matchedLines.Add((filePath, line, lineNumber));
            }
        }
    }
}