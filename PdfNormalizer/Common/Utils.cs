using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfNormalizer.Common
{
    internal class Utils
    {
        private static readonly char[] trimChars = [' ', '\r', '\n'];

        public static IEnumerable<string> GetPDFPaths(IEnumerable<string> paths) => paths
            .Where(Path.Exists)
            .Select(Path.GetFullPath)
            .Distinct().SelectMany(p =>
        {
            return File.GetAttributes(p).HasFlag(FileAttributes.Directory)
            ? Directory.GetFiles(p, "*.pdf", SearchOption.TopDirectoryOnly)
            : [p];
        });

        public static bool CanBeTrimmed(string target) => target != target.Trim(trimChars);

        public static string Trim(string target) => target.Trim(trimChars);

        public static string EscapeEOL(string target) => target.Replace("\r", "\\r").Replace("\n", "\\n");
    }
}
