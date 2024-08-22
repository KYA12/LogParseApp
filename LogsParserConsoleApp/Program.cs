using Microsoft.Extensions.Configuration;
using LogsShared;

class Program
{
    static async Task Main(string[] args)
    {
        var config = LoadConfiguration();
        var directoryPath = ValidateDirectoryPath(config["LogDirectoryPath"]);
        var bufferSize = ValidateBufferSize(config["BufferSize"]);

        while (true)
        {
            try
            {
                Console.Write("Enter the search pattern (or type 'exit' to quit): ");
                var searchPattern = Console.ReadLine();

                if (string.Equals(searchPattern, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (string.IsNullOrEmpty(searchPattern))
                {
                    Console.WriteLine("Search pattern cannot be empty. Please try again.");
                    continue;
                }

                var regexPattern = PatternConverter.ConvertToRegexIfNeeded(searchPattern);
                var isRegexValid = PatternConverter.IsValidRegexPattern(regexPattern);

                if (!isRegexValid)
                {
                    Console.WriteLine("Invalid regex pattern. Please try again.");
                    continue;
                }

                Console.WriteLine($"Using regex pattern: {regexPattern}");

                var logSearcher = new LogSearcher();
                var matchedLines = await logSearcher.SearchLogsAsync(directoryPath, regexPattern, bufferSize);

                DisplayResults(matchedLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    static string ValidateDirectoryPath(string? directoryPath)
    {
        var path = ValidateConfigValue(directoryPath, "LogDirectoryPath");
   
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
        }
      
        return path;
    }

    static int ValidateBufferSize(string? bufferSizeValue)
    {
        var value = ValidateConfigValue(bufferSizeValue, "BufferSize");
    
        if (int.TryParse(value, out int bufferSize) && bufferSize > 0)
        {
            return bufferSize;
        }
        else
        {
            throw new ApplicationException("BufferSize is not a valid integer or is less than or equal to zero.");
        }
    }

    static string ValidateConfigValue(string? configValue, string configKey)
    {
        if (string.IsNullOrEmpty(configValue))
        {
            throw new ApplicationException($"{configKey} is not configured or is empty in appsettings.json.");
        }
    
        return configValue;
    }

    static void DisplayResults(List<(string FilePath, string Line, int LineNumber)> matchedLines)
    {
        if (matchedLines.Count == 0)
        {
            Console.WriteLine("No matches found.");
        }
        else
        {
            foreach (var match in matchedLines)
            {
                Console.WriteLine($"Match found in {match.FilePath} at line {match.LineNumber}: {match.Line}");
            }
        }
    }
}