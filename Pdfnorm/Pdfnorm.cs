using System.Collections.Generic;
using System.IO;
using System.Linq;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.XMP;
using iText.Kernel.XMP.Impl;
using iText.Kernel.XMP.Options;

using Pdfnorm.Common;

using Utils = Pdfnorm.Common.Utils;

namespace Pdfnorm
{
    internal class PDFNormalizer
    {
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), "PDFHelper");
        private readonly IEnumerable<string> pdfPaths;
        private readonly bool dry;

        private string currentPdfName = "";
        private string currentTempPath = "";

        public PDFNormalizer(IEnumerable<string> pdfPaths, bool dry)
        {
            this.pdfPaths = pdfPaths;
            this.dry = dry;
            if (!Path.Exists(tempDir)) { Directory.CreateDirectory(tempDir); }
        }

        public void Normalize()
        {
            int index = 1;
            foreach (string pdfPath in pdfPaths)
            {
                currentPdfName = Path.GetFileNameWithoutExtension(pdfPath);
                currentTempPath = Path.Combine(tempDir, Path.GetFileName(pdfPath));
                PrintUtils.WriteLine($"[<DarkCyan>{index}/{pdfPaths.Count()}</DarkCyan>] [<DarkMagenta>{currentPdfName}</DarkMagenta>]");

                List<FixRecord> fixRecords = Normalize(pdfPath);
                if (!dry && fixRecords.Count > 0)
                {
                    File.Move(currentTempPath, pdfPath, true);
                }
                if (File.Exists(currentTempPath))
                {
                    File.Delete(currentTempPath);
                }

                index++;
            }
        }

        private List<FixRecord> Normalize(string pdfPath)
        {
            List<FixRecord> fixRecords = [];

            using PdfDocument pdfDoc = dry
                ? new PdfDocument(new PdfReader(pdfPath))
                : new PdfDocument(new PdfReader(pdfPath), new PdfWriter(currentTempPath));

            NormalizeXMPMetadata(fixRecords, pdfDoc);

            NormalizeInitialView(fixRecords, pdfDoc);

            NormalizeOutline(fixRecords, pdfDoc);

            return fixRecords;
        }

        private void NormalizeOutline(List<FixRecord> fixRecords, PdfDocument pdfDoc)
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
                NormalizeSingleOutline(fixRecords, pdfDoc, bookmark, dry);

                if (bookmark.GetAllChildren().Count > 0)
                {
                    foreach (PdfOutline child in bookmark.GetAllChildren())
                    {
                        bookmarkQueue.Enqueue(child);
                    }
                }
            }
        }

        private void NormalizeInitialView(List<FixRecord> fixRecords, PdfDocument pdfDoc)
        {
            PdfCatalog catalog = pdfDoc.GetCatalog();
            PdfViewerPreferences view = catalog.GetViewerPreferences();

            IssueUtils.Fix(
                view == null,
                $"Display document title is set false because view preferences is null.",
                currentPdfName,
                dry,
                fixRecords,
                () =>
                {
                    view = new();
                    view.SetDisplayDocTitle(true);
                    catalog.SetViewerPreferences(view);
                },
                $"Fix by creating view preferences and setting display document title to true.");

            bool displayDocTitle = view!.GetPdfObject().GetAsBool(PdfName.DisplayDocTitle) ?? false;
            IssueUtils.Fix(
                !displayDocTitle,
                $"Display document title is set false.",
                currentPdfName,
                dry,
                fixRecords,
                () => view.SetDisplayDocTitle(true),
                $"Fix by setting display document title to true.");

            PdfName pageMode = catalog.GetPageMode();
            IssueUtils.Fix(
                pageMode != PdfName.UseOutlines,
                $"In initial view, page mode is not set to [<DarkRed>{PdfName.UseOutlines}</DarkRed>], but [<DarkRed>{pageMode}</DarkRed>].",
                currentPdfName,
                dry,
                fixRecords,
                () => catalog.SetPageMode(PdfName.UseOutlines),
                $"Fix by setting the page mode to [<DarkRed>{PdfName.UseOutlines}</DarkRed>].");

            PdfName pageLayout = catalog.GetPageLayout();
            IssueUtils.Fix(
                pageLayout != PdfName.TwoPageRight,
                $"In initial view, page layout is not set to [<DarkRed>{PdfName.TwoPageRight}</DarkRed>], but [<DarkRed>{pageLayout}</DarkRed>].",
                currentPdfName,
                dry,
                fixRecords,
                () => catalog.SetPageLayout(PdfName.TwoPageRight),
                $"Fix by setting the page layout to [<DarkRed>{PdfName.TwoPageRight}</DarkRed>].");

            PdfArray openDestArray = (catalog.GetPdfObject().Get(PdfName.OpenAction) as PdfDictionary)!.Get(PdfName.D) as PdfArray;
            int openDestNumber = pdfDoc.GetPageNumber(openDestArray!.Get(0) as PdfDictionary);
            IssueUtils.Fix(
                openDestNumber != 1,
                $"In initial view, page number is not set to [<DarkRed>1</DarkRed>], but [<DarkRed>{openDestNumber}</DarkRed>].",
                currentPdfName,
                dry,
                fixRecords,
                () => openDestArray.Set(0, pdfDoc.GetPage(1).GetPdfObject()),
                $"Fix by setting the page number to [<DarkRed>1</DarkRed>].");

            PdfName destLocation = openDestArray!.Get(1) as PdfName;
            IssueUtils.Fix(
                destLocation != PdfName.Fit,
                "PDF open destination is not valid. " +
                    $"Expected: [<DarkRed>{PdfName.Fit}</DarkRed>], actual: [<DarkRed>{destLocation}</DarkRed>]",
                currentPdfName,
                dry,
                fixRecords,
                () => NormalizeDestArray(openDestArray),
                "Fix by updating the location to \"Fit Page\".");
        }

        private void NormalizeXMPMetadata(List<FixRecord> fixRecords, PdfDocument pdfDoc)
        {
            XMPMeta xmp = XMPMetaParser.Parse(pdfDoc.GetXmpMetadata(), new ParseOptions());
            string title = xmp.GetArrayItem(XMPConst.NS_DC, "title", 1)?.GetValue() ?? string.Empty;
            IssueUtils.Fix(string.IsNullOrEmpty(title), "PDF title is empty.", currentPdfName);
            IssueUtils.Fix(
                Utils.CanBeTrimmed(title),
                $"PDF title [<DarkBlue>{title}</DarkBlue>] can be trimmed.",
                currentPdfName,
                dry,
                fixRecords,
                () => xmp.SetArrayItem(XMPConst.NS_DC, "title", 1, Utils.Trim(title)),
                $"Fix by trimming the title to [<DarkBlue>{Utils.Trim(title)}</DarkBlue>]");

            int authorCount = xmp.CountArrayItems(XMPConst.NS_DC, "creator");
            IssueUtils.Fix(authorCount == 0, "PDF does not have an author.", currentPdfName);
            for (int index = 1; index <= authorCount; index++)
            {
                string author = xmp.GetArrayItem(XMPConst.NS_DC, "creator", index).GetValue();
                IssueUtils.Fix(string.IsNullOrEmpty(author), $"PDF author [{index}] is empty.", currentPdfName);
                IssueUtils.Fix(
                    Utils.CanBeTrimmed(author),
                    $"PDF author [{index}] [<DarkBlue>{author}</DarkBlue>] can be trimmed.",
                    currentPdfName,
                    dry,
                    fixRecords,
                    () => xmp.SetArrayItem(XMPConst.NS_DC, "creator", index, Utils.Trim(author)),
                    $"Fix by trimming author [{index}] to [<DarkBlue>{Utils.Trim(title)}</DarkBlue>]");
            }

            // Update XMP create time to make it the preferable option for Acrobat
            xmp.SetPropertyDate(XMPConst.NS_XMP, "MetadataDate", XMPDateTimeFactory.GetCurrentDateTime());
            pdfDoc.SetXmpMetadata(xmp);
        }

        private void NormalizeSingleOutline(List<FixRecord> fixRecords, PdfDocument pdfDoc, PdfOutline pdfOutline, bool dry)
        {
            string bookMarkTitle = pdfOutline.GetTitle();
            IssueUtils.Fix(
                Utils.CanBeTrimmed(bookMarkTitle),
                $"PDF bookmark Title [<DarkBlue>{Utils.EscapeEOL(bookMarkTitle)}</DarkBlue>] can be trimmed.",
                currentPdfName,
                dry,
                fixRecords,
                () => pdfOutline.SetTitle(Utils.Trim(bookMarkTitle)),
                $"Fix by trimming the bookmark title to [<DarkBlue>{Utils.Trim(bookMarkTitle)}</DarkBlue>]");

            PdfObject bookMarkDest = pdfOutline.GetDestination().GetPdfObject();
            PdfArray bookMarkDestArray = bookMarkDest as PdfArray;
            PdfString bookMarkDestString = bookMarkDest as PdfString;

            bool bookMarkDestIsInvalid = (bookMarkDestArray == null || bookMarkDestArray.Size() < 2 || bookMarkDestArray.Get(1) == null) && bookMarkDestString == null;
            IssueUtils.Fix(
                bookMarkDestIsInvalid,
                $"PDF bookmark [<DarkBlue>{Utils.EscapeEOL(bookMarkTitle)}</DarkBlue>] does not has valid destination.",
                currentPdfName);
            if (bookMarkDestIsInvalid) { return; }

            if (bookMarkDestArray != null)
            {
                PdfName destLocation = bookMarkDestArray!.Get(1) as PdfName;

                IssueUtils.Fix(
                    destLocation != PdfName.Fit,
                    $"PDF bookmark [<DarkBlue>{Utils.EscapeEOL(bookMarkTitle)}</DarkBlue>] does not has valid destination location. " +
                    $"Expected: [<DarkRed>{PdfName.Fit}</DarkRed>], actual: [<DarkRed>{destLocation}</DarkRed>]",
                    currentPdfName,
                    dry,
                    fixRecords,
                    () => NormalizeDestArray(bookMarkDestArray),
                    "Fix by updating the location to \"Fit Page\".");
            }
            else if (bookMarkDestString != null)
            {
                IssueUtils.Fix(
                    true,
                    $"PDF bookmark [<DarkBlue>{Utils.EscapeEOL(bookMarkTitle)}</DarkBlue>] has a named destination.",
                    currentPdfName,
                    dry,
                    fixRecords,
                    () => NormalizeSingleOutline(pdfDoc, pdfOutline, bookMarkDestString),
                    "Fix by updating the destination to explicit destination.");
            }
        }

        private static void NormalizeSingleOutline(PdfDocument pdfDoc, PdfOutline pdfOutline, PdfString bookMarkDestString)
        {
            IDictionary<PdfString, PdfObject> names = pdfDoc.GetCatalog().GetNameTree(PdfName.Dests).GetNames();
            PdfArray namedDest = names[bookMarkDestString] as PdfArray;
            PdfArray bookMarkDestArray = [namedDest!.Get(0), PdfName.Fit];
            pdfOutline.AddDestination(PdfDestination.MakeDestination(bookMarkDestArray));
        }

        private static void NormalizeDestArray(PdfArray destArray)
        {
            PdfName destLocation = destArray!.Get(1) as PdfName;

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
}
