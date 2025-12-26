using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using PdfNorm.Common;
using PdfNorm.Interfaces;
using PdfNorm.Models;

namespace PdfNorm.Services.Norms
{
    public class OutlineNorm(IssueReporter issueReporter) : IPdfNorm
    {
        private readonly IssueReporter _issueReporter = issueReporter;
        private PdfConfig? _config;

        public void SetConfig(PdfConfig? config)
        {
            _config = config;
        }

        public void Normalize(PdfDocument pdfDoc, string pdfName, bool dryRun, List<FixRecord> fixRecords)
        {
            PdfOutline outline = pdfDoc.GetOutlines(false);
            IList<PdfOutline> bookmarks = outline.GetAllChildren();
            Queue<PdfOutline> bookmarkQueue = new();
            
            foreach (PdfOutline bookmark in bookmarks)
            {
                bookmarkQueue.Enqueue(bookmark);
            }

            while (bookmarkQueue.Count > 0)
            {
                PdfOutline bookmark = bookmarkQueue.Dequeue();
                NormalizeSingleOutline(pdfDoc, bookmark, pdfName, dryRun, fixRecords);

                if (bookmark.GetAllChildren().Count > 0)
                {
                    foreach (PdfOutline child in bookmark.GetAllChildren())
                    {
                        bookmarkQueue.Enqueue(child);
                    }
                }
            }
        }

        private void NormalizeSingleOutline(PdfDocument pdfDoc, PdfOutline pdfOutline, string pdfName, bool dryRun, List<FixRecord> fixRecords)
        {
            string bookMarkTitle = pdfOutline.GetTitle();
            
            if (TextUtils.CanBeTrimmed(bookMarkTitle))
            {
                _issueReporter.ReportAndFix(
                    pdfName,
                    $"PDF bookmark Title [blue]{TextUtils.EscapeEOL(bookMarkTitle)}[/] can be trimmed.",
                    $"Fix by trimming the bookmark title to [blue]{TextUtils.Trim(bookMarkTitle)}[/]",
                    () => pdfOutline.SetTitle(TextUtils.Trim(bookMarkTitle)),
                    fixRecords,
                    dryRun);
            }

            PdfObject bookMarkDest = pdfOutline.GetDestination().GetPdfObject();
            PdfArray bookMarkDestArray = bookMarkDest as PdfArray;
            PdfString bookMarkDestString = bookMarkDest as PdfString;

            bool bookMarkDestIsInvalid = (bookMarkDestArray == null || bookMarkDestArray.Size() < 2 || bookMarkDestArray.Get(1) == null) && bookMarkDestString == null;
            
            if (bookMarkDestIsInvalid)
            {
                _issueReporter.Report(pdfName, $"PDF bookmark [blue]{TextUtils.EscapeEOL(bookMarkTitle)}[/] does not has valid destination.");
                return;
            }

            if (bookMarkDestArray != null)
            {
                NormalizeArrayDestination(bookMarkDestArray, bookMarkTitle, pdfName, dryRun, fixRecords);
            }
            else if (bookMarkDestString != null)
            {
                NormalizeNamedDestination(pdfDoc, pdfOutline, bookMarkDestString, bookMarkTitle, pdfName, dryRun, fixRecords);
            }
        }

        private void NormalizeArrayDestination(PdfArray bookMarkDestArray, string bookMarkTitle, string pdfName, bool dryRun, List<FixRecord> fixRecords)
        {
            PdfName destLocation = bookMarkDestArray!.Get(1) as PdfName;
            PdfName targetZoom = GetTargetBookmarkZoom();

            if (destLocation != targetZoom)
            {
                _issueReporter.ReportAndFix(
                    pdfName,
                    $"PDF bookmark [blue]{TextUtils.EscapeEOL(bookMarkTitle)}[/] does not has valid destination location. " +
                    $"Expected: [red]{targetZoom}[/], actual: [red]{destLocation}[/]",
                    $"Fix by updating the location to \"{GetZoomDescription(targetZoom)}\".",
                    () => NormalizeDestArray(bookMarkDestArray, targetZoom),
                    fixRecords,
                    dryRun);
            }
        }

        private void NormalizeNamedDestination(PdfDocument pdfDoc, PdfOutline pdfOutline, PdfString bookMarkDestString, string bookMarkTitle, string pdfName, bool dryRun, List<FixRecord> fixRecords)
        {
            _issueReporter.ReportAndFix(
                pdfName,
                $"PDF bookmark [blue]{TextUtils.EscapeEOL(bookMarkTitle)}[/] has a named destination.",
                "Fix by updating the destination to explicit destination.",
                () => ConvertNamedDestination(pdfDoc, pdfOutline, bookMarkDestString, GetTargetBookmarkZoom()),
                fixRecords,
                dryRun);
        }

        private void ConvertNamedDestination(PdfDocument pdfDoc, PdfOutline pdfOutline, PdfString bookMarkDestString, PdfName targetZoom)
        {
            IDictionary<PdfString, PdfObject> names = pdfDoc.GetCatalog().GetNameTree(PdfName.Dests).GetNames();
            PdfArray namedDest = names[bookMarkDestString] as PdfArray;
            PdfArray bookMarkDestArray = [namedDest!.Get(0), targetZoom];
            pdfOutline.AddDestination(PdfDestination.MakeDestination(bookMarkDestArray));
        }

        private static void NormalizeDestArray(PdfArray destArray, PdfName targetZoom)
        {
            PdfName destLocation = destArray!.Get(1) as PdfName;

            if (targetZoom == PdfName.XYZ)
            {
                // For XYZ, keep current page but set zoom parameters
                // [page /XYZ left top zoom] - null zoom means inherit, 0 means actual size
                if (destArray.Size() > 2)
                {
                    destArray.Set(1, PdfName.XYZ);
                    // Keep existing parameters or set defaults
                }
                else
                {
                    destArray.Set(1, PdfName.XYZ);
                    destArray.Add(new PdfNumber(0)); // left
                    destArray.Add(new PdfNumber(0)); // top  
                    destArray.Add(new PdfNumber(0)); // zoom (0 = actual size)
                }
            }
            else if (destLocation == PdfName.FitH || destLocation == PdfName.FitBH)
            {
                destArray.Set(1, targetZoom);
                if (targetZoom != PdfName.FitH && destArray.Size() > 2)
                {
                    destArray.Remove(2);
                }
            }
            else if (destLocation == PdfName.XYZ)
            {
                destArray.Set(1, targetZoom);
                while (destArray.Size() > 2)
                {
                    destArray.Remove(2);
                }
            }
            else
            {
                destArray.Set(1, targetZoom);
            }
        }

        private PdfName GetTargetBookmarkZoom()
        {
            if (string.IsNullOrEmpty(_config?.BookmarkZoom))
            {
                return PdfName.Fit; // Default
            }

            return _config.BookmarkZoom switch
            {
                "FitPage" => PdfName.Fit,
                "FitWidth" => PdfName.FitH,
                "FitVisible" => PdfName.FitB,
                "ActualSize" => PdfName.XYZ,
                "InheritZoom" => PdfName.XYZ,
                _ => PdfName.Fit // Default fallback
            };
        }

        private string GetZoomDescription(PdfName zoom)
        {
            if (zoom == PdfName.Fit) return "Fit Page";
            if (zoom == PdfName.FitH) return "Fit Width";
            if (zoom == PdfName.FitB) return "Fit Visible";
            if (zoom == PdfName.XYZ) return "XYZ";
            return "Fit Page";
        }
    }
}
