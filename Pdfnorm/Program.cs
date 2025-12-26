using System.Collections.Generic;
using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using PdfNorm.Interfaces;
using PdfNorm.Services;
using PdfNorm.Services.Norms;

// Setup DI Container
ServiceCollection services = new();

// Register services
services.AddSingleton<IFileService, FileService>();
services.AddSingleton<IProgressReporter, ConsoleProgressReporter>();
services.AddSingleton<IssueReporter>();

// Register norms
services.AddSingleton<IPdfNorm, MetadataNorm>();
services.AddSingleton<IPdfNorm, ViewNorm>();
services.AddSingleton<IPdfNorm, OutlineNorm>();

// Register processors
services.AddSingleton<IPdfDocProcessor, PdfDocProcessor>();
services.AddSingleton<IPdfNormService, PdfNormService>();

ServiceProvider serviceProvider = services.BuildServiceProvider();

// Setup CLI
Argument<List<string>> pathsArgument = new("paths")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "Path(s) of PDF files or directories. If a directory is provided, all PDF files inside (only top level) will be normalized."
};

Option<string> configOption = new("--config", "-c")
{
    Description = "Path to JSON configuration file"
};

Option<bool> dryRunOption = new("--dry-run", "-n")
{
    Description = "Preview changes without modifying files"
};

RootCommand rootCommand = new()
{
    Description = "Normalize PDF files with configuration."
};
rootCommand.Options.Add(dryRunOption);
rootCommand.Options.Add(configOption);
rootCommand.Arguments.Add(pathsArgument);

rootCommand.SetAction(parseResult =>
{
    List<string> rawPaths = parseResult.GetValue(pathsArgument);
    bool dryRun = parseResult.GetValue(dryRunOption);
    string configPath = parseResult.GetValue(configOption);

    PdfNorm.Models.PdfConfig? config = ConfigService.LoadConfig(configPath);

    IFileService fileService = serviceProvider.GetRequiredService<IFileService>();
    IEnumerable<string> pdfPaths = fileService.GetPdfPaths(rawPaths);

    IPdfNormService service = serviceProvider.GetRequiredService<IPdfNormService>();
    service.NormalizeAll(pdfPaths, dryRun, config);
});

rootCommand.Parse(args).Invoke();
