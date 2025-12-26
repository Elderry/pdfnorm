# pdfnorm

A command-line tool for batch normalizing PDF files using JSON configuration. Built with [iText](https://itextpdf.com/) for .NET.

## Features

- **Batch Processing** - Process multiple PDFs or entire directories at once
- **JSON Configuration** - Customize metadata with templates and tokens (see [CONFIG.md](CONFIG.md))
- **Metadata Normalization** - Clean up and standardize PDF metadata (title, author)
- **Initial View Settings** - Configure how PDFs open (page mode, layout, zoom)
- **Bookmark Management** - Normalize PDF bookmarks/outlines and destinations
- **Dry Run Mode** - Preview changes without modifying files
- **Detailed Reporting** - See exactly what issues were found and fixed

## Installation

```bash
dotnet build -c Release
```

The executable will be in `Pdfnorm/bin/Release/net9.0/`

## Usage

```bash
# Normalize single PDF
pdfnorm path/to/file.pdf

# Normalize multiple PDFs
pdfnorm file1.pdf file2.pdf file3.pdf

# Normalize all PDFs in a directory
pdfnorm path/to/directory

# Dry run (check without modifying)
pdfnorm file.pdf --dry-run

# With custom configuration
pdfnorm file.pdf --config config.json
pdfnorm *.pdf -c config.json
```

## What Gets Normalized

### Metadata
- **Title** - Removes leading/trailing whitespace
- **Author** - Trims author names
- **Display Settings** - Sets display document title preference

### Initial View
- **Page Mode** - Sets to `UseOutlines` (show bookmarks panel)
- **Page Layout** - Sets to `TwoPageRight` (two-page view, odd pages on right)
- **Open Page** - Opens to page 1
- **Zoom Level** - Sets to "Fit Page"

### Bookmarks
- **Title Cleanup** - Trims bookmark titles
- **Destination Type** - Converts all destinations to "Fit Page" explicit destinations
- **Named Destinations** - Converts named destinations to explicit destinations
onfiguration

PdfNorm supports JSON configuration files for customizing metadata. See [CONFIG.md](CONFIG.md) for detailed documentation.

**Quick example:**
```json
{
  "Title": "{file_name}",
  "Author": "Your Name"
}
```

This sets each PDF's title to its filename and author to "Your Name"
Currently hardcoded to normalize PDFs to a specific standard (useful for ebook/document collections). Future versions will support JSON configuration files to customize normalization rules.

## Requirements

- .NET 10.0
- iText 9.4.0

## License

See [LICENSE](LICENSE) file for details.
