using ClosedXML.Excel;
using Warehouse.BusinessLayer.Settings;
using static Warehouse.BusinessLayer.Services.Export.ExportFormatting;

namespace Warehouse.BusinessLayer.Services.Export;

internal static class ExcelDocuments
{
    private const double MaxColumnWidth = 60;

    public static byte[] Tabular(TabularReport report, CompanySettings company)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(SheetName(report.Title));

        var row = 1;

        sheet.Cell(row, 1).Value = report.Title;
        sheet.Cell(row, 1).Style.Font.SetBold().Font.SetFontSize(14);
        row++;

        if (!string.IsNullOrWhiteSpace(report.Subtitle))
        {
            sheet.Cell(row, 1).Value = report.Subtitle;
            sheet.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#5A6169"));
            row++;
        }

        sheet.Cell(row, 1).Value = $"{company.Name} · generisano {System.DateTime.Now:dd.MM.yyyy. HH:mm}";
        sheet.Cell(row, 1).Style.Font.SetFontColor(XLColor.FromHtml("#5A6169")).Font.SetFontSize(9);
        row += 2;

        foreach (var (label, value) in report.Summary)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 1).Style.Font.SetBold();
            sheet.Cell(row, 2).Value = value;
            row++;
        }

        if (report.Summary.Count > 0)
            row++;

        var singleSection = report.Sections.Count == 1;

        foreach (var section in report.Sections)
        {
            if (section.Title is not null)
            {
                sheet.Cell(row, 1).Value = section.Title;
                sheet.Cell(row, 1).Style.Font.SetBold().Font.SetFontSize(12);
                row++;
            }

            var headerRow = row;

            for (var c = 0; c < section.Columns.Count; c++)
            {
                var cell = sheet.Cell(headerRow, c + 1);
                cell.Value = section.Columns[c].Header;
                cell.Style.Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#F2F4F5"))
                    .Border.SetBottomBorder(XLBorderStyleValues.Thin);

                if (section.Columns[c].IsNumeric)
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            }

            row++;

            foreach (var values in section.Rows)
            {
                for (var c = 0; c < section.Columns.Count; c++)
                    Write(sheet.Cell(row, c + 1), c < values.Length ? values[c] : null, section.Columns[c].Kind);
                row++;
            }

            if (singleSection && section.Rows.Count > 0)
            {
                sheet.Range(headerRow, 1, row - 1, section.Columns.Count).SetAutoFilter();
                sheet.SheetView.FreezeRows(headerRow);
            }

            row += 2;
        }

        if (report.Footnote is not null)
        {
            sheet.Cell(row, 1).Value = report.Footnote;
            sheet.Cell(row, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.FromHtml("#5A6169"));
        }

        sheet.Columns().AdjustToContents();
        foreach (var column in sheet.ColumnsUsed())
        {
            if (column.Width > MaxColumnWidth)
                column.Width = MaxColumnWidth;
        }

        sheet.Column(1).Width = Math.Max(sheet.Column(1).Width, 14);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void Write(IXLCell cell, object? value, ColumnKind kind)
    {
        switch (value)
        {
            case null:
                return;
            case decimal number:
                cell.Value = (double)number;
                cell.Style.NumberFormat.Format = kind == ColumnKind.Money ? "#,##0.00 \"RSD\"" : "#,##0.###";
                break;
            case int number:
                cell.Value = number;
                cell.Style.NumberFormat.Format = "#,##0";
                break;
            case double number:
                cell.Value = number;
                cell.Style.NumberFormat.Format = "#,##0.0";
                break;
            case System.DateTime date:
                cell.Value = Local(date);
                cell.Style.NumberFormat.Format = "dd.mm.yyyy hh:mm";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string SheetName(string title)
    {
        var invalid = new[] { '[', ']', ':', '*', '?', '/', '\\' };
        var clean = new string(title.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
        if (clean.Length == 0) clean = "Izvestaj";
        return clean.Length > 31 ? clean[..31] : clean;
    }
}
