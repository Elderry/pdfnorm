using iText.Kernel.Pdf;
using iText.Kernel.XMP;
using iText.Kernel.XMP.Impl;
using iText.Kernel.XMP.Options;

using PdfNorm.Common;
using PdfNorm.Models;
using PdfNorm.Services;
using PdfNorm.Services.Norms;

namespace PdfNorm.Tests
{
    [TestClass]
    public class MetadataNormTests
    {
        private string _testPdfPath = null!;
        private string _outputPdfPath = null!;

        [TestInitialize]
        public void Setup()
        {
            _testPdfPath = Path.Combine(Path.GetTempPath(), "test_input.pdf");
            _outputPdfPath = Path.Combine(Path.GetTempPath(), "test_output.pdf");

            // Create a simple test PDF with initial title
            CreateTestPdf(_testPdfPath, "Original Title", "Original Author");
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
        public void Normalize_ShouldChangeTitle_WhenConfigProvided()
        {
            // Arrange
            PdfConfig config = new()
            {
                Title = "New Test Title"
            };

            IssueReporter issueReporter = new(new ConsoleProgressReporter());
            MetadataNorm metadataNorm = new(issueReporter);
            metadataNorm.SetConfig(config);

            List<FixRecord> fixRecords = [];

            // Act
            using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
            {
                metadataNorm.Normalize(pdfDoc, "test", false, fixRecords);
            }

            // Assert
            string actualTitle = GetPdfTitle(_outputPdfPath);
            Assert.AreEqual("New Test Title", actualTitle);
            Assert.IsNotEmpty(fixRecords, "Should have recorded the fix");
        }

        [TestMethod]
        public void Normalize_ShouldUseFileNameToken_WhenConfigContainsToken()
        {
            // Arrange
            PdfConfig config = new()
            {
                Title = "Document - {file_name}"
            };

            IssueReporter issueReporter = new(new ConsoleProgressReporter());
            MetadataNorm metadataNorm = new(issueReporter);
            metadataNorm.SetConfig(config);

            List<FixRecord> fixRecords = [];

            // Act
            using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
            {
                metadataNorm.Normalize(pdfDoc, "mydoc", false, fixRecords);
            }

            // Assert
            string actualTitle = GetPdfTitle(_outputPdfPath);
            Assert.AreEqual("Document - mydoc", actualTitle);
        }

        [TestMethod]
        public void Normalize_ShouldChangeAuthor_WhenConfigProvided()
        {
            // Arrange
            PdfConfig config = new()
            {
                Author = "New Author Name"
            };

            IssueReporter issueReporter = new(new ConsoleProgressReporter());
            MetadataNorm metadataNorm = new(issueReporter);
            metadataNorm.SetConfig(config);

            List<FixRecord> fixRecords = [];

            // Act
            using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
            {
                metadataNorm.Normalize(pdfDoc, "test", false, fixRecords);
            }

            // Assert
            string actualAuthor = GetPdfAuthor(_outputPdfPath);
            Assert.AreEqual("New Author Name", actualAuthor);
        }

        [TestMethod]
        public void Normalize_ShouldTrimTitle_WhenNoConfigAndTitleHasWhitespace()
        {
            // Arrange
            CreateTestPdf(_testPdfPath, "  Title With Spaces  ", "Author");

            IssueReporter issueReporter = new(new ConsoleProgressReporter());
            MetadataNorm metadataNorm = new(issueReporter);
            metadataNorm.SetConfig(null); // No config

            List<FixRecord> fixRecords = [];

            // Act
            using (PdfDocument pdfDoc = new(new PdfReader(_testPdfPath), new PdfWriter(_outputPdfPath)))
            {
                metadataNorm.Normalize(pdfDoc, "test", false, fixRecords);
            }

            // Assert
            string actualTitle = GetPdfTitle(_outputPdfPath);
            Assert.AreEqual("Title With Spaces", actualTitle);
        }

        private void CreateTestPdf(string path, string title, string author)
        {
            using PdfDocument pdfDoc = new(new PdfWriter(path));
            pdfDoc.AddNewPage();

            // Set metadata via XMP
            XMPMeta xmp = XMPMetaFactory.Create();
            xmp.SetArrayItem(XMPConst.NS_DC, "title", 1, title);

            PropertyOptions options = new();
            options.SetArray(true);
            xmp.AppendArrayItem(XMPConst.NS_DC, "creator", options, author, null);
            pdfDoc.SetXmpMetadata(xmp);
        }

        private string GetPdfTitle(string path)
        {
            using PdfDocument pdfDoc = new(new PdfReader(path));
            XMPMeta xmp = XMPMetaParser.Parse(pdfDoc.GetXmpMetadata(), new ParseOptions());
            return xmp.GetArrayItem(XMPConst.NS_DC, "title", 1)?.GetValue() ?? string.Empty;
        }

        private string GetPdfAuthor(string path)
        {
            using PdfDocument pdfDoc = new(new PdfReader(path));
            XMPMeta xmp = XMPMetaParser.Parse(pdfDoc.GetXmpMetadata(), new ParseOptions());
            return xmp.GetArrayItem(XMPConst.NS_DC, "creator", 1)?.GetValue() ?? string.Empty;
        }
    }
}
