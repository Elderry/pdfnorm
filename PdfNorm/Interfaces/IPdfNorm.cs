using System.Collections.Generic;
using iText.Kernel.Pdf;
using PdfNorm.Common;
using PdfNorm.Models;

namespace PdfNorm.Interfaces
{
    public interface IPdfNorm
    {
        void SetConfig(PdfConfig? config);
        void Normalize(PdfDocument pdfDoc, string pdfName, bool dryRun, List<FixRecord> fixRecords);
    }
}
