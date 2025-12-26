using System.Collections.Generic;
using System.IO;
using System.Linq;

using PdfNorm.Common;
using PdfNorm.Interfaces;
using PdfNorm.Models;

namespace PdfNorm.Services;

public class PdfNormService(
    IFileService fileService,
    IProgressReporter progressReporter,
    IPdfDocProcessor docProcessor) : IPdfNormService
{
    private readonly IFileService _fileService = fileService;
    private readonly IProgressReporter _progressReporter = progressReporter;
    private readonly IPdfDocProcessor _docProcessor = docProcessor;

    public void NormalizeAll(IEnumerable<string> pdfPaths, bool dryRun, PdfConfig? config = null)
    {
        _docProcessor.SetConfig(config);

        int total = pdfPaths.Count();
        int current = 1;

        foreach (string pdfPath in pdfPaths)
        {
            string pdfName = Path.GetFileNameWithoutExtension(pdfPath);
            string tempPath = _fileService.CreateTempFilePath(pdfPath);

            _progressReporter.ReportProgress(current, total, pdfName);

            List<FixRecord> fixRecords = _docProcessor.Process(pdfPath, tempPath, pdfName, dryRun);

            if (!dryRun && fixRecords.Count > 0)
            {
                _fileService.MoveTempToOriginal(tempPath, pdfPath);
            }

            _fileService.DeleteTempFile(tempPath);

            current++;
        }
    }
}
