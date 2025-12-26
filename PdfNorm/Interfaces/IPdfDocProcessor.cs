using System.Collections.Generic;
using PdfNorm.Common;
using PdfNorm.Models;

namespace PdfNorm.Interfaces
{
    public interface IPdfDocProcessor
    {
        void SetConfig(PdfConfig? config);
        List<FixRecord> Process(string pdfPath, string tempPath, string pdfName, bool dryRun);
    }
}
