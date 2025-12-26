using System.Collections.Generic;
using System.IO;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PdfNorm.Common;
using PdfNorm.Models;
using PdfNorm.Services;
using PdfNorm.Services.Norms;

namespace PdfNorm.Tests;

[TestClass]
public class OutlineNormTests
{
    private string _testPdfPath = null!;
    private string _outputPdfPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testPdfPath = Path.Combine(Path.GetTempPath(), "test_outline_input.pdf");
        _outputPdfPath = Path.Combine(Path.GetTempPath(), "test_outline_output.pdf");

        CreateTestPdfWithBookmarks(_testPdfPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testPdfPath))
        {
            File.Delete(_testPdfPath);
        }

        if (File.Exists(_outputPdfPath))
        {
            File.Delete(_outputPdfPath);
        }
    }

    [TestMethod]
    public void Normalize_ShouldTrimBookmarkTitle_WhenTitleHasWhitespace()
    {
        // Arrange
        CreateTestPdfWithBookmarks(_testPdfPath, "  Chapter 1  ");

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        OutlineNorm outlineNorm = new(issueReporter);
        outlineNorm.SetConfig(null);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            outlineNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string bookmarkTitle = GetFirstBookmarkTitle(_outputPdfPath);
        Assert.AreEqual("Chapter 1", bookmarkTitle);
        Assert.IsNotEmpty(fixRecords, "Should have recorded the fix");
    }

    [TestMethod]
    public void Normalize_ShouldSetBookmarkZoom_WhenConfigSpecifies()
    {
        // Arrange
        PdfConfig config = new()
        {
            BookmarkZoom = "FitWidth"
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        OutlineNorm outlineNorm = new(issueReporter);
        outlineNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            outlineNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string zoomMode = GetFirstBookmarkZoomMode(_outputPdfPath);
        Assert.AreEqual("FitH", zoomMode);
    }

    [TestMethod]
    public void Normalize_ShouldUseDefaultFitPage_WhenNoConfigProvided()
    {
        // Arrange
        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        OutlineNorm outlineNorm = new(issueReporter);
        outlineNorm.SetConfig(null);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            outlineNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string zoomMode = GetFirstBookmarkZoomMode(_outputPdfPath);
        Assert.AreEqual("Fit", zoomMode);
    }

    [TestMethod]
    public void Normalize_ShouldSetActualSizeZoom_WhenConfigSpecifies()
    {
        // Arrange
        PdfConfig config = new()
        {
            BookmarkZoom = "ActualSize"
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        OutlineNorm outlineNorm = new(issueReporter);
        outlineNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            outlineNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string zoomMode = GetFirstBookmarkZoomMode(_outputPdfPath);
        Assert.AreEqual("XYZ", zoomMode);
    }

    private void CreateTestPdfWithBookmarks(string path, string bookmarkTitle = "Chapter 1")
    {
        using PdfDocument pdfDoc = new(new PdfWriter(path));
        pdfDoc.AddNewPage();
        pdfDoc.AddNewPage();

        // Add outline/bookmark
        PdfOutline rootOutline = pdfDoc.GetOutlines(false);
        PdfOutline chapter1 = rootOutline.AddOutline(bookmarkTitle);
        chapter1.AddDestination(PdfExplicitDestination.CreateFit(pdfDoc.GetPage(1)));
    }

    private string GetFirstBookmarkTitle(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        PdfOutline rootOutline = pdfDoc.GetOutlines(false);
        IList<PdfOutline> children = rootOutline.GetAllChildren();
        return children.Count > 0 ? children[0].GetTitle() : string.Empty;
    }

    private string GetFirstBookmarkZoomMode(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        PdfOutline rootOutline = pdfDoc.GetOutlines(false);
        IList<PdfOutline> children = rootOutline.GetAllChildren();
        if (children.Count == 0)
        {
            return string.Empty;
        }

        PdfDestination? dest = children[0].GetDestination();
        if (dest == null)
        {
            return string.Empty;
        }

        if (dest.GetPdfObject() is not PdfArray destArray || destArray.Size() < 2)
        {
            return string.Empty;
        }

        PdfName? zoomMode = destArray.Get(1) as PdfName;
        return zoomMode?.GetValue() ?? string.Empty;
    }
}
