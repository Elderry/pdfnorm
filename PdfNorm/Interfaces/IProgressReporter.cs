namespace PdfNorm.Interfaces;

public interface IProgressReporter
{
    void ReportProgress(int current, int total, string fileName);
    void ReportIssue(string fileName, string message);
    void ReportFix(string fileName, string message);
}
