using System.Collections.Generic;
using iText.Kernel.Pdf;
using PdfNorm.Common;
using PdfNorm.Interfaces;
using PdfNorm.Models;

namespace PdfNorm.Services
{
    public class PdfDocProcessor : IPdfDocProcessor
    {
        private readonly IEnumerable<IPdfNorm> _norms;

        public PdfDocProcessor(IEnumerable<IPdfNorm> norms)
        {
            _norms = norms;
        }

        public void SetConfig(PdfConfig? config)
        {
            foreach (var norm in _norms)
            {
                norm.SetConfig(config);
            }
        }

        public List<FixRecord> Process(string pdfPath, string tempPath, string pdfName, bool dryRun)
        {
            List<FixRecord> fixRecords = [];

            using PdfDocument pdfDoc = dryRun
                ? new PdfDocument(new PdfReader(pdfPath))
                : new PdfDocument(new PdfReader(pdfPath), new PdfWriter(tempPath));

            foreach (var norm in _norms)
            {
                norm.Normalize(pdfDoc, pdfName, dryRun, fixRecords);
            }

            return fixRecords;
        }
    }
}
