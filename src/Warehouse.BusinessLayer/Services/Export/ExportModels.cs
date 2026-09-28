namespace Warehouse.BusinessLayer.Services.Export;

public enum ExportFormat
{
    Xlsx = 1,
    Pdf = 2
}

public enum ColumnKind
{
    Text,
    Mono,
    Integer,
    Quantity,
    Money,
    DateTime
}

public record ExportFile(byte[] Content, string ContentType, string FileName);

public record ReportColumn(string Header, ColumnKind Kind = ColumnKind.Text, float Width = 1f)
{
    public bool IsNumeric => Kind is ColumnKind.Integer or ColumnKind.Quantity or ColumnKind.Money;
}

public record TableSection(string? Title, IReadOnlyList<ReportColumn> Columns, IReadOnlyList<object?[]> Rows);

public record TabularReport(
    string Title,
    string? Subtitle,
    IReadOnlyList<(string Label, string Value)> Summary,
    IReadOnlyList<TableSection> Sections,
    string? Footnote = null);

public record PartyInfo(string Title, string Name, IReadOnlyList<string> Lines);
