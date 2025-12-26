using System.Collections.Generic;
using PdfNorm.Models;

namespace PdfNorm.Interfaces
{
    public interface IPdfNormService
    {
        void NormalizeAll(IEnumerable<string> pdfPaths, bool dryRun, PdfConfig? config = null);
    }
}
