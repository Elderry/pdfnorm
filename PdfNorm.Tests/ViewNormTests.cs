using iText.Kernel.Pdf;

using PdfNorm.Common;
using PdfNorm.Models;
using PdfNorm.Services;
using PdfNorm.Services.Norms;

namespace PdfNorm.Tests;

[TestClass]
public class ViewNormTests
{
    private string _testPdfPath = null!;
    private string _outputPdfPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testPdfPath = Path.Combine(Path.GetTempPath(), "test_view_input.pdf");
        _outputPdfPath = Path.Combine(Path.GetTempPath(), "test_view_output.pdf");

        CreateTestPdf(_testPdfPath);
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
    public void Normalize_ShouldSetDisplayDocTitleToFalse_WhenConfigSpecifies()
    {
        // Arrange
        PdfConfig config = new()
        {
            DisplayDocTitle = false
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        ViewNorm viewNorm = new(issueReporter);
        viewNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            viewNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        bool displayDocTitle = GetDisplayDocTitle(_outputPdfPath);
        Assert.IsFalse(displayDocTitle);
    }

    [TestMethod]
    public void Normalize_ShouldSetPageMode_WhenConfigSpecifies()
    {
        // Arrange
        PdfConfig config = new()
        {
            PageMode = "Pages"
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        ViewNorm viewNorm = new(issueReporter);
        viewNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            viewNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string pageMode = GetPageMode(_outputPdfPath);
        Assert.AreEqual("UseThumbs", pageMode);
    }

    [TestMethod]
    public void Normalize_ShouldSetPageLayout_WhenConfigSpecifies()
    {
        // Arrange
        PdfConfig config = new()
        {
            PageLayout = "OneColumn"
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        ViewNorm viewNorm = new(issueReporter);
        viewNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            viewNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        string pageLayout = GetPageLayout(_outputPdfPath);
        Assert.AreEqual("OneColumn", pageLayout);
    }

    [TestMethod]
    public void Normalize_ShouldSetOpenToPage_WhenConfigSpecifies()
    {
        // Arrange
        CreateTestPdfWithMultiplePages(_testPdfPath, 10);

        PdfConfig config = new()
        {
            OpenToPage = 5
        };

        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        ViewNorm viewNorm = new(issueReporter);
        viewNorm.SetConfig(config);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            viewNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        int openPage = GetOpenToPage(_outputPdfPath);
        Assert.AreEqual(5, openPage);
    }

    [TestMethod]
    public void Normalize_ShouldSetDefaultValues_WhenNoConfigProvided()
    {
        // Arrange
        IssueReporter issueReporter = new(new ConsoleProgressReporter());
        ViewNorm viewNorm = new(issueReporter);
        viewNorm.SetConfig(null);

        List<FixRecord> fixRecords = [];

        // Act
        using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
        {
            viewNorm.Normalize(pdfDoc, "test", false, fixRecords);
        }

        // Assert
        bool displayDocTitle = GetDisplayDocTitle(_outputPdfPath);
        string pageMode = GetPageMode(_outputPdfPath);
        string pageLayout = GetPageLayout(_outputPdfPath);

        Assert.IsTrue(displayDocTitle, "Default DisplayDocTitle should be true");
        Assert.AreEqual("UseOutlines", pageMode, "Default PageMode should be UseOutlines");
        Assert.AreEqual("TwoPageRight", pageLayout, "Default PageLayout should be TwoPageRight");
    }

    private void CreateTestPdf(string path)
    {
        using PdfDocument pdfDoc = new(new PdfWriter(path));
        pdfDoc.AddNewPage();
    }

    private void CreateTestPdfWithMultiplePages(string path, int pageCount)
    {
        using PdfDocument pdfDoc = new(new PdfWriter(path));
        for (int i = 0; i < pageCount; i++)
        {
            pdfDoc.AddNewPage();
        }
    }

    private bool GetDisplayDocTitle(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        PdfViewerPreferences? prefs = pdfDoc.GetCatalog().GetViewerPreferences();
        return prefs?.GetPdfObject().GetAsBool(PdfName.DisplayDocTitle) ?? false;
    }

    private string GetPageMode(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        return pdfDoc.GetCatalog().GetPageMode()?.GetValue() ?? string.Empty;
    }

    private string GetPageLayout(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        return pdfDoc.GetCatalog().GetPageLayout()?.GetValue() ?? string.Empty;
    }

    private int GetOpenToPage(string path)
    {
        using PdfDocument pdfDoc = new(new PdfReader(path));
        return pdfDoc.GetCatalog().GetPdfObject().Get(PdfName.OpenAction) is not PdfDictionary openAction
            ? 1
            : openAction.Get(PdfName.D) is not PdfArray destArray
            ? 1
            : destArray.Get(0) is not PdfDictionary pageDict ? 1 : pdfDoc.GetPageNumber(pageDict);
    }
}
