using System.Collections.Generic;

using iText.Kernel.Pdf;

using PdfNorm.Common;
using PdfNorm.Interfaces;
using PdfNorm.Models;

namespace PdfNorm.Services.Norms;

public class ViewNorm(IssueReporter issueReporter) : IPdfNorm
{
    private readonly IssueReporter _issueReporter = issueReporter;
    private PdfConfig? _config;

    public void SetConfig(PdfConfig? config)
    {
        _config = config;
    }

    public void Normalize(PdfDocument pdfDoc, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        PdfCatalog catalog = pdfDoc.GetCatalog();
        PdfViewerPreferences view = catalog.GetViewerPreferences();

        NormalizeViewerPreferences(catalog, view, pdfName, dryRun, fixRecords);
        NormalizePageMode(catalog, pdfName, dryRun, fixRecords);
        NormalizePageLayout(catalog, pdfName, dryRun, fixRecords);
        NormalizeOpenAction(catalog, pdfDoc, pdfName, dryRun, fixRecords);
    }

    private void NormalizeViewerPreferences(PdfCatalog catalog, PdfViewerPreferences view, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        if (view == null)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                "Display document title is set false because view preferences is null.",
                "Fix by creating view preferences and setting display document title to true.",
                () =>
                {
                    view = new PdfViewerPreferences();
                    view.SetDisplayDocTitle(true);
                    catalog.SetViewerPreferences(view);
                },
                fixRecords,
                dryRun);
        }

        bool displayDocTitle = view!.GetPdfObject().GetAsBool(PdfName.DisplayDocTitle) ?? false;
        bool targetValue = _config?.DisplayDocTitle ?? true; // Default to true if not configured

        if (displayDocTitle != targetValue)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"Display document title is set to {displayDocTitle}.",
                $"Fix by setting display document title to {targetValue}.",
                () => view.SetDisplayDocTitle(targetValue),
                fixRecords,
                dryRun);
        }
    }

    private void NormalizePageMode(PdfCatalog catalog, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        PdfName pageMode = catalog.GetPageMode();
        PdfName targetPageMode = GetTargetPageMode();

        if (pageMode != targetPageMode)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"In initial view, page mode is not set to [red]{targetPageMode}[/], but [red]{pageMode}[/].",
                $"Fix by setting the page mode to [red]{targetPageMode}[/].",
                () => catalog.SetPageMode(targetPageMode),
                fixRecords,
                dryRun);
        }
    }

    private PdfName GetTargetPageMode()
    {
        if (string.IsNullOrEmpty(_config?.PageMode))
        {
            return PdfName.UseOutlines; // Default
        }

        return _config.PageMode switch
        {
            "PageOnly" => PdfName.UseNone,
            "Bookmarks" => PdfName.UseOutlines,
            "Pages" => PdfName.UseThumbs,
            "Attachments" => PdfName.UseAttachments,
            "Layers" => PdfName.UseOC,
            _ => PdfName.UseOutlines // Default fallback
        };
    }

    private void NormalizePageLayout(PdfCatalog catalog, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        PdfName pageLayout = catalog.GetPageLayout();
        PdfName targetPageLayout = GetTargetPageLayout();

        if (pageLayout != targetPageLayout)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"In initial view, page layout is not set to [red]{targetPageLayout}[/], but [red]{pageLayout}[/].",
                $"Fix by setting the page layout to [red]{targetPageLayout}[/].",
                () => catalog.SetPageLayout(targetPageLayout),
                fixRecords,
                dryRun);
        }
    }

    private PdfName GetTargetPageLayout()
    {
        if (string.IsNullOrEmpty(_config?.PageLayout))
        {
            return PdfName.TwoPageRight; // Default
        }

        return _config.PageLayout switch
        {
            "SinglePage" => PdfName.SinglePage,
            "OneColumn" => PdfName.OneColumn,
            "TwoColumnLeft" => PdfName.TwoColumnLeft,
            "TwoColumnRight" => PdfName.TwoColumnRight,
            "TwoPageLeft" => PdfName.TwoPageLeft,
            "TwoPageRight" => PdfName.TwoPageRight,
            _ => PdfName.TwoPageRight // Default fallback
        };
    }

    private void NormalizeOpenAction(PdfCatalog catalog, PdfDocument pdfDoc, string pdfName, bool dryRun, List<FixRecord> fixRecords)
    {
        int targetPage = _config?.OpenToPage ?? 1;

        // Validate page number
        if (targetPage < 1 || targetPage > pdfDoc.GetNumberOfPages())
        {
            targetPage = 1;
        }

        // If OpenAction doesn't exist, create it
        if (catalog.GetPdfObject().Get(PdfName.OpenAction) is not PdfDictionary openActionDict)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                "PDF does not have an open action set.",
                $"Fix by creating open action to page {targetPage} with Fit zoom.",
                () =>
                {
                    PdfArray destArray = [pdfDoc.GetPage(targetPage).GetPdfObject(), PdfName.Fit];

                    PdfDictionary newOpenAction = new();
                    newOpenAction.Put(PdfName.S, PdfName.GoTo);
                    newOpenAction.Put(PdfName.D, destArray);
                    catalog.GetPdfObject().Put(PdfName.OpenAction, newOpenAction);
                },
                fixRecords,
                dryRun);
            return;
        }

        if (openActionDict.Get(PdfName.D) is not PdfArray openDestArray || openDestArray.Size() < 2)
        {
            _issueReporter.Report(pdfName, "PDF open action destination is invalid.");
            return;
        }

        if (openDestArray.Get(0) is not PdfDictionary pageDict)
        {
            _issueReporter.Report(pdfName, "PDF open action page reference is invalid.");
            return;
        }

        int openDestNumber = pdfDoc.GetPageNumber(pageDict);

        if (openDestNumber != targetPage)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"In initial view, page number is not set to [red]{targetPage}[/], but [red]{openDestNumber}[/].",
                $"Fix by setting the page number to [red]{targetPage}[/].",
                () => openDestArray.Set(0, pdfDoc.GetPage(targetPage).GetPdfObject()),
                fixRecords,
                dryRun);
        }

        PdfName? destLocation = openDestArray!.Get(1) as PdfName;
        if (destLocation != PdfName.Fit)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                "PDF open destination is not valid. " +
                    $"Expected: [red]{PdfName.Fit}[/], actual: [red]{destLocation}[/]",
                "Fix by updating the location to \"Fit Page\".",
                () => NormalizeDestArray(openDestArray),
                fixRecords,
                dryRun);
        }
    }

    private static void NormalizeDestArray(PdfArray destArray)
    {
        PdfName? destLocation = destArray!.Get(1) as PdfName;

        if (destLocation == PdfName.FitH || destLocation == PdfName.FitBH)
        {
            destArray.Set(1, PdfName.Fit);
            destArray.Remove(2);
        }
        else if (destLocation == PdfName.XYZ)
        {
            destArray.Set(1, PdfName.Fit);
            destArray.Remove(2);
            destArray.Remove(2);
            destArray.Remove(2);
        }
    }
}
