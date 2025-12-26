using System;

using PdfNorm.Interfaces;

using Spectre.Console;

namespace PdfNorm.Services;

public class ConsoleProgressReporter : IProgressReporter
{
    public void ReportProgress(int current, int total, string fileName)
    {
        AnsiConsole.MarkupLine($"[yellow][[{DateTime.Now:HH:mm:ss}]][/] [cyan]{current}/{total}[/] [magenta]{fileName}[/]");
    }

    public void ReportIssue(string fileName, string message)
    {
        AnsiConsole.MarkupLine($"[yellow][[{DateTime.Now:HH:mm:ss}]][/] [magenta]{fileName}[/] {message.EscapeMarkup()}");
    }

    public void ReportFix(string fileName, string message)
    {
        AnsiConsole.MarkupLine($"[yellow][[{DateTime.Now:HH:mm:ss}]][/] [magenta]{fileName}[/] {message.EscapeMarkup()}");
    }
}
