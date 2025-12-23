using System.Collections.Generic;
using System.CommandLine;

using Pdfnorm;
using Pdfnorm.Common;

Argument<List<string>> pathArgument = new("path")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "Path(s) of PDF files, if a directory path provided, all PDF files inside (only top level) will be normalized."
};

Option<string> configurationOption = new("--configuration", "-c")
{
    Arity = ArgumentArity.ExactlyOne,
    Description = "Configuration file that the normalizer follow."
};

Option<bool> dryOption = new("--dry", "-d")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Whether to actually fix PDF file violations."
};

RootCommand rootCommand = new()
{
    Description = "Normalize PDF files with configuration."
};
rootCommand.Options.Add(dryOption);
rootCommand.Arguments.Add(pathArgument);

rootCommand.SetAction(parseResult =>
{
    List<string> rawPaths = parseResult.GetValue(pathArgument);
    bool dry = parseResult.GetValue(dryOption);
    string configurationPath = parseResult.GetValue(configurationOption);
    PDFNormalizer normalizer = new(Utils.GetPDFPaths(rawPaths), dry);
    normalizer.Normalize();
});

rootCommand.Parse(args).Invoke();

List<string> ParseAndValidatePath(List<string> paths)
{

}
