using System.IO;
using System.Text.Json;
using PdfNorm.Models;

namespace PdfNorm.Services
{
    public class ConfigService
    {
        public static PdfConfig? LoadConfig(string? configPath)
        {
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                return null;
            }

            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<PdfConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
