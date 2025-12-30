# Configuration Guide

PdfNorm supports JSON configuration files to customize PDF normalization behavior.

## Usage

```bash
pdfnorm file.pdf --config config.json
pdfnorm *.pdf -c config.json
```

## Configuration Format

Configuration files are JSON format with the following structure:

```json
{
  "Title": "string",
  "Author": "string",
  "DisplayDocTitle": true,
  "PageMode": "Bookmarks",
  "PageLayout": "TwoPageRight",
  "OpenToPage": 1,
  "BookmarkZoom": "FitPage"
}
```

## Available Options

### Metadata Configuration

#### `Title`
Sets the PDF document title. When specified, all processed PDFs will have their title updated to this value.

**Type:** `string`
**Optional:** Yes
**Supports tokens:** Yes

**Example:**
```json
{
  "Title": "My Document"
}
```

#### `Author`
Sets the PDF document author. When specified, all processed PDFs will have their author updated to this value.

**Type:** `string`
**Optional:** Yes
**Supports tokens:** No

**Example:**
```json
{
  "Author": "John Doe"
}
```

### Window Options

#### `DisplayDocTitle`
Controls what is shown in the PDF viewer's window title bar.

**Type:** `boolean`
**Optional:** Yes
**Default:** `true` (show document title)
**Supports tokens:** No

**Values:**
- `true` - Display the document title in the window title bar
- `false` - Display the filename in the window title bar

**Example:**
```json
{
  "DisplayDocTitle": true
}
```

**Note:** This corresponds to "Window Options - Show" in Adobe Acrobat (Document Title vs. File Name).

#### `PageMode`
Controls which panels are displayed when the PDF is opened (Navigation tab in Acrobat).

**Type:** `string`
**Optional:** Yes
**Default:** `"Bookmarks"` (Bookmarks Panel and Page)
**Supports tokens:** No

**Values:**
- `"PageOnly"` - Page Only (no side panels)
- `"Bookmarks"` - Bookmarks Panel and Page
- `"Pages"` - Pages Panel and Page (page thumbnails)
- `"Attachments"` - Attachments Panel and Page
- `"Layers"` - Layers Panel and Page (optional content)

**Example:**
```json
{
  "PageMode": "Bookmarks"
}
```

**Note:** This corresponds to "Layout and Magnification - Navigation tab" in Adobe Acrobat.

#### `PageLayout`
Controls how pages are displayed (single page, continuous scroll, facing pages, etc.).

**Type:** `string`
**Optional:** Yes
**Default:** `"TwoPageRight"` (two pages side-by-side, odd pages on right)
**Supports tokens:** No

**Values:**
- `"SinglePage"` - Display one page at a time
- `"OneColumn"` - Continuous scroll, one page wide
- `"TwoColumnLeft"` - Continuous scroll, two pages side-by-side, odd pages on left
- `"TwoColumnRight"` - Continuous scroll, two pages side-by-side, odd pages on right
- `"TwoPageLeft"` - Two pages at a time, odd pages on left
- `"TwoPageRight"` - Two pages at a time, odd pages on right (book-like facing pages)

**Example:**
```json
{
  "PageLayout": "TwoPageRight"
}
```

**Note:** This corresponds to "Layout and Magnification - Page layout" in Adobe Acrobat.

#### `OpenToPage`
Sets which page the PDF opens to when first opened.

**Type:** `integer`
**Optional:** Yes
**Default:** `1` (first page)
**Supports tokens:** No

**Example:**
```json
{
  "OpenToPage": 5
}
```

**Note:** 
- Page numbers are 1-based (first page is 1, not 0)
- If the specified page number is invalid (less than 1 or greater than total pages), defaults to page 1
- This corresponds to "Layout and Magnification - Open to page" in Adobe Acrobat

#### `BookmarkZoom`
Sets the zoom/fit mode for bookmark destinations. Controls how the page displays when clicking a bookmark.

**Type:** `string`
**Optional:** Yes
**Default:** `"FitPage"` (fit entire page in window)
**Supports tokens:** No

**Values:**
- `"FitPage"` - Fit entire page in window
- `"FitWidth"` - Fit page width to window
- `"FitVisible"` - Fit visible content (excluding margins)
- `"ActualSize"` - Display at 100% zoom (actual size)
- `"InheritZoom"` - Maintain current zoom level (jump to page without changing zoom)

**Example:**
```json
{
  "BookmarkZoom": "FitWidth"
}
```

**Note:** This setting applies to all bookmarks/outlines in the PDF when they are normalized.

## Template Tokens

The `Title` field supports dynamic tokens that are replaced at runtime:

| Token | Description | Example Input | Example Output |
|-------|-------------|---------------|----------------|
| `{file_name}` | PDF filename without extension | `report.pdf` | `report` |

### Token Examples

**Set title to filename:**
```json
{
  "Title": "{file_name}"
}
```

For `mydocument.pdf`, title becomes: `mydocument`

**Combine text with tokens:**
```json
{
  "Title": "Report - {file_name}"
}
```

For `quarterly-2024.pdf`, title becomes: `Report - quarterly-2024`

**Static values:**
```json
{
  "Title": "Company Report",
  "Author": "Acme Corporation"
}
```

All PDFs get the exact same title and author.

## Configuration Behavior

### Priority Order

When a configuration file is provided:

1. **Config values take precedence** - If `Title` or `Author` is specified in config, it overrides existing PDF metadata
2. **Validation still runs** - If no config provided, normal trimming and validation rules apply

### Without Configuration

If no `--config` option is provided, PdfNorm applies default normalization:
- Trims leading/trailing whitespace from title and author
- Reports empty or missing metadata
- Applies standard view and bookmark normalization

### With Configuration

When `--config` is provided:
- Config values override existing metadata
- Tokens are replaced with actual values
- Any metadata not specified in config follows default normalization rules

## Complete Example

**config.json:**
```json
{
  "Title": "{file_name}",
  "Author": "Digital Library"
}
```

**Command:**
```bash
pdfnorm ebooks/*.pdf --config config.json
```

**Result:**
- `introduction.pdf` → Title: "introduction", Author: "Digital Library"
- `chapter-01.pdf` → Title: "chapter-01", Author: "Digital Library"
- `appendix.pdf` → Title: "appendix", Author: "Digital Library"

## Notes

- Configuration files must be valid JSON
- Unknown fields in the config are ignored
- Both `Title` and `Author` are optional - you can specify just one
- Tokens are case-sensitive: use `{file_name}`, not `{File_Name}`
- If a config file path is provided but the file doesn't exist, normalization continues without configuration

## Future Enhancements

Additional configuration options planned for future releases:
- View settings (page mode, layout, zoom)
- Bookmark behavior customization
- Subject and Keywords metadata
- Additional tokens (date, directory name, etc.)
- Conditional rules based on filename patterns
