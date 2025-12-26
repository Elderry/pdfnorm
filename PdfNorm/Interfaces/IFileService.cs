using System.Collections.Generic;

namespace PdfNorm.Interfaces;

public interface IFileService
{
    IEnumerable<string> GetPdfPaths(IEnumerable<string> paths);
    string CreateTempFilePath(string originalPath);
    void MoveTempToOriginal(string tempPath, string originalPath);
    void DeleteTempFile(string tempPath);
    bool TempFileExists(string tempPath);
}
