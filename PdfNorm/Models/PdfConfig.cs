namespace PdfNorm.Models;

public class PdfConfig
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public bool? DisplayDocTitle { get; set; }
    public string? PageMode { get; set; }
    public string? PageLayout { get; set; }
    public int? OpenToPage { get; set; }
    public string? BookmarkZoom { get; set; }
}
