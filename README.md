# 📄 PdfNorm

![Tests](https://github.com/Elderry/pdfnorm/actions/workflows/tests.yml/badge.svg)

A command-line tool for batch normalizing PDF files using JSON configuration. Built with [iText](https://itextpdf.com/) for .NET.

## ✨ Features

- **Batch Processing** - Process multiple PDFs or entire directories at once
- **JSON Configuration** - Fully customizable via JSON config file (see [CONFIG.md](CONFIG.md))
- **Metadata Normalization** - Set title, author with dynamic tokens like `{file_name}`
- **Initial View Settings** - Configure page mode, layout, zoom level, and open-to page
- **Bookmark Management** - Normalize bookmark titles and destination zoom modes
- **Dry Run Mode** - Preview changes without modifying files
- **Detailed Reporting** - See exactly what issues were found and fixed

## 🔧 Installation

```bash
dotnet build -c Release
```

The executable will be in `PdfNorm/bin/Release/net10.0/`

## 🚀 Usage

```bash
# Normalize single PDF with defaults
pdfnorm path/to/file.pdf

# Normalize multiple PDFs
pdfnorm file1.pdf file2.pdf file3.pdf

# Normalize all PDFs in a directory
pdfnorm path/to/directory

# Preview changes without modifying files
pdfnorm file.pdf --dry-run

# Use custom configuration file
pdfnorm file.pdf --config config.json
pdfnorm *.pdf -c myconfig.json

# Combine options
pdfnorm path/to/directory --config config.json --dry-run
```

## 🔄 What Gets Normalized

Without configuration, PdfNorm applies sensible defaults:

### Metadata
- **Title** - Trims leading/trailing whitespace
- **Author** - Trims author names
- **Display Settings** - Shows document title in window title bar

### Initial View
- **Page Mode** - `UseOutlines` (show bookmarks panel)
- **Page Layout** - `TwoPageRight` (two-page view, odd pages on right)
- **Open Page** - Opens to page 1

### Bookmarks
- **Title Cleanup** - Trims whitespace from bookmark titles
- **Destination Mode** - Normalizes zoom to "Fit Page"

## ⚙️ Configuration

PdfNorm supports JSON configuration files for full control. See [CONFIG.md](CONFIG.md) for complete documentation.

**Example config.json:**
```json
{
  "Title": "{file_name}",
  "Author": "Your Name",
  "DisplayDocTitle": true,
  "PageMode": "Bookmarks",
  "PageLayout": "TwoColumnRight",
  "OpenToPage": 1,
  "BookmarkZoom": "FitWidth"
}
```

**Configuration features:**
- Set metadata with `{file_name}` token for dynamic values
- Configure viewer preferences (page mode, layout)
- Set initial view (page number, zoom level)
- Control bookmark zoom behavior
- All fields optional - omit to use defaults or trim existing values

## 📜 License

See [LICENSE](LICENSE) file for details.
