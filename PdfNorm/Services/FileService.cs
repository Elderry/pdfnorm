using System.Collections.Generic;
using System.IO;
using System.Linq;

using PdfNorm.Interfaces;

namespace PdfNorm.Services;

public class FileService : IFileService
{
    private readonly string _tempDir;

    public FileService()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PDFHelper");
        if (!Directory.Exists(_tempDir))
        {
            Directory.CreateDirectory(_tempDir);
        }
    }

    public IEnumerable<string> GetPdfPaths(IEnumerable<string> paths)
    {
        return paths
            .Where(Path.Exists)
            .Select(Path.GetFullPath)
            .Distinct()
            .SelectMany(p =>
            {
                return File.GetAttributes(p).HasFlag(FileAttributes.Directory)
                    ? Directory.GetFiles(p, "*.pdf", SearchOption.TopDirectoryOnly)
                    : [p];
            });
    }

    public string CreateTempFilePath(string originalPath)
    {
        return Path.Combine(_tempDir, Path.GetFileName(originalPath));
    }

    public void MoveTempToOriginal(string tempPath, string originalPath)
    {
        File.Move(tempPath, originalPath, true);
    }

    public void DeleteTempFile(string tempPath)
    {
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
    }

    public bool TempFileExists(string tempPath)
    {
        return File.Exists(tempPath);
    }
}
