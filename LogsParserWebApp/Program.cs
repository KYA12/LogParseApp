using LogsShared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<LogSearcher>();

try
{
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    var configuration = builder.Configuration;
    ValidateConfiguration(configuration);
}
catch (Exception ex)
{
    var loggerFactory = LoggerFactory.Create(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
    });

    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogCritical(ex, "Critical error during configuration validation.");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Search");
    await Task.CompletedTask;
});

app.Run();

void ValidateConfiguration(IConfiguration configuration)
{
    var logDirectoryPath = configuration["LogDirectoryPath"];
    var bufferSizeValue = configuration["BufferSize"];

    if (string.IsNullOrEmpty(logDirectoryPath))
    {
        throw new ApplicationException("LogDirectoryPath is not configured or is empty in appsettings.json.");
    }

    if (!Directory.Exists(logDirectoryPath))
    {
        throw new DirectoryNotFoundException($"The directory '{logDirectoryPath}' does not exist.");
    }

    if (string.IsNullOrEmpty(bufferSizeValue) || !int.TryParse(bufferSizeValue, out var bufferSize) || bufferSize <= 0)
    {
        throw new ApplicationException("BufferSize is not a valid integer or is less than or equal to zero.");
    }
}