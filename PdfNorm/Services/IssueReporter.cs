using System;
using System.Collections.Generic;
using PdfNorm.Common;
using PdfNorm.Interfaces;

namespace PdfNorm.Services
{
    public class IssueReporter
    {
        private readonly IProgressReporter _progressReporter;

        public IssueReporter(IProgressReporter progressReporter)
        {
            _progressReporter = progressReporter;
        }

        public void Report(string pdfName, string issueMessage)
        {
            _progressReporter.ReportIssue(pdfName, issueMessage);
        }

        public void ReportAndFix(
            string pdfName,
            string issueMessage,
            string fixMessage,
            Action fixAction,
            List<FixRecord> fixRecords,
            bool dryRun)
        {
            _progressReporter.ReportIssue(pdfName, issueMessage);
            _progressReporter.ReportFix(pdfName, fixMessage);

            if (!dryRun)
            {
                fixRecords.Add(new FixRecord(issueMessage));
                fixAction.Invoke();
            }
        }
    }
}
