using System.Collections.Generic;

using iText.Kernel.Pdf;
using iText.Kernel.XMP;
using iText.Kernel.XMP.Options;

using PdfNorm.Common;
using PdfNorm.Interfaces;
using PdfNorm.Models;

namespace PdfNorm.Services.Norms;

public class MetadataNorm(IssueReporter issueReporter) : IPdfNorm
{
    private readonly IssueReporter _issueReporter = issueReporter;
    private PdfConfig? _config;

    public void SetConfig(PdfConfig? config)
    {
        _config = config;
    }

    public void Normalize(PdfDocument pdfDoc, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        XMPMeta xmp = pdfDoc.GetXmpMetadata(true);

        NormalizeTitle(xmp, pdfName, dryRun, fixRecords);
        NormalizeAuthors(xmp, pdfName, dryRun, fixRecords);

        // Update XMP create time to make it the preferable option for Acrobat
        xmp.SetPropertyDate(XMPConst.NS_XMP, "MetadataDate", XMPDateTimeFactory.GetCurrentDateTime());
        pdfDoc.SetXmpMetadata(xmp);
    }

    private void NormalizeTitle(XMPMeta xmp, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        string title = xmp.GetArrayItem(XMPConst.NS_DC, "title", 1)?.GetValue() ?? string.Empty;

        // Set title from config if provided
        if (!string.IsNullOrEmpty(_config?.Title))
        {
            string configTitle = _config.Title.Replace("{file_name}", pdfName);

            if (title != configTitle)
            {
                _issueReporter.ReportAndFix(
                    pdfName,
                    $"PDF title [blue]{title}[/] doesn't match config.",
                    $"Fix by setting title to [blue]{configTitle}[/]",
                    () => xmp.SetArrayItem(XMPConst.NS_DC, "title", 1, configTitle),
                    fixRecords,
                    dryRun);
                return;
            }
        }

        if (string.IsNullOrEmpty(title))
        {
            _issueReporter.Report(pdfName, "PDF title is empty.");
        }

        if (TextUtils.CanBeTrimmed(title))
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"PDF title [blue]{title}[/] can be trimmed.",
                $"Fix by trimming the title to [blue]{TextUtils.Trim(title)}[/]",
                () => xmp.SetArrayItem(XMPConst.NS_DC, "title", 1, TextUtils.Trim(title)),
                fixRecords,
                dryRun);
        }
    }

    private void NormalizeAuthors(XMPMeta xmp, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        int authorCount = xmp.CountArrayItems(XMPConst.NS_DC, "creator");

        // Set author from config if provided
        if (!string.IsNullOrEmpty(_config?.Author))
        {
            string currentAuthor = authorCount > 0 ? xmp.GetArrayItem(XMPConst.NS_DC, "creator", 1)?.GetValue() ?? string.Empty : string.Empty;

            if (currentAuthor != _config.Author)
            {
                _issueReporter.ReportAndFix(
                    pdfName,
                    $"PDF author [blue]{currentAuthor}[/] doesn't match config.",
                    $"Fix by setting author to [blue]{_config.Author}[/]",
                    () =>
                    {
                        // Clear existing authors and set the config one
                        for (int i = authorCount; i >= 1; i--)
                        {
                            xmp.DeleteArrayItem(XMPConst.NS_DC, "creator", i);
                        }
                        PropertyOptions options = new();
                        options.SetArray(true);
                        xmp.AppendArrayItem(XMPConst.NS_DC, "creator", options, _config.Author, null);
                    },
                    fixRecords,
                    dryRun);
                return;
            }
        }

        if (authorCount == 0)
        {
            _issueReporter.Report(pdfName, "PDF does not have an author.");
        }

        for (int index = 1; index <= authorCount; index++)
        {
            string author = xmp.GetArrayItem(XMPConst.NS_DC, "creator", index).GetValue();

            if (string.IsNullOrEmpty(author))
            {
                _issueReporter.Report(pdfName, $"PDF author [{index}] is empty.");
            }

            if (TextUtils.CanBeTrimmed(author))
            {
                _issueReporter.ReportAndFix(
                    pdfName,
                    $"PDF author [{index}] [blue]{author}[/] can be trimmed.",
                    $"Fix by trimming author [{index}] to [blue]{TextUtils.Trim(author)}[/]",
                    () => xmp.SetArrayItem(XMPConst.NS_DC, "creator", index, TextUtils.Trim(author)),
                    fixRecords,
                    dryRun);
            }
        }
    }
}
