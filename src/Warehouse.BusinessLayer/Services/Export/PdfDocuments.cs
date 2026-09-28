using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.Settings;
using Warehouse.Domain.Enums;
using static Warehouse.BusinessLayer.Services.Export.ExportFormatting;

namespace Warehouse.BusinessLayer.Services.Export;

internal static class PdfDocuments
{
    private const string Ink = "#16191C";
    private const string Muted = "#5A6169";
    private const string Line = "#DDE2E5";
    private const string Surface = "#F2F4F5";
    private const string Accent = "#1F5F4B";
    private const string WarnSurface = "#FAF0E9";
    private const string Warn = "#B4541A";

    public static byte[] Order(
        OrderDetailDto detail, PartyInfo from, PartyInfo to, CompanySettings company)
    {
        var order = detail.Order;
        var title = order.OrderType == OrderType.Inbound ? "PRIJEMNICA" : "OTPREMNICA";

        var banner = order.Status switch
        {
            OrderStatus.Completed => null,
            OrderStatus.Cancelled => "Nalog je otkazan — dokument nema važnost.",
            _ => "Nalog još nije realizovan — dokument nije konačan i služi samo za pregled."
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, landscape: false);
                page.Header().Element(c => CompanyHeader(c, company, title, order.OrderNumber));

                page.Content().PaddingTop(18).Column(col =>
                {
                    col.Spacing(14);

                    if (banner is not null)
                        col.Item().Background(WarnSurface).Padding(8).Text(banner).FontColor(Warn).SemiBold();

                    col.Item().Row(row =>
                    {
                        row.Spacing(12);
                        row.RelativeItem().Element(c => Party(c, from));
                        row.RelativeItem().Element(c => Party(c, to));
                    });

                    col.Item().Element(c => MetaGrid(c, new List<(string, string)>
                    {
                        ("Broj naloga", order.OrderNumber),
                        ("Datum naloga", Date(order.CreatedAt)),
                        ("Status", Status(order.Status)),
                        ("Kreirao", order.CreatedByName),
                        ("Odobrio", order.ApprovedByName is null
                            ? "—"
                            : $"{order.ApprovedByName}, {Date(order.ApprovedAt!.Value)}"),
                        ("Realizovan", order.CompletedAt.HasValue ? Timestamp(order.CompletedAt.Value) : "—"),
                    }));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(22);
                            columns.ConstantColumn(72);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(38);
                            columns.ConstantColumn(64);
                            columns.ConstantColumn(72);
                            columns.ConstantColumn(84);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("#").SemiBold();
                            header.Cell().Element(HeaderCell).Text("Šifra").SemiBold();
                            header.Cell().Element(HeaderCell).Text("Naziv").SemiBold();
                            header.Cell().Element(HeaderCell).Text("JM").SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Količina").SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Cena").SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Iznos").SemiBold();
                        });

                        var index = 1;
                        foreach (var item in detail.Items)
                        {
                            table.Cell().Element(BodyCell).Text(index++.ToString()).FontColor(Muted);
                            table.Cell().Element(BodyCell).Text(item.ProductSku);
                            table.Cell().Element(BodyCell).Text(item.ProductName);
                            table.Cell().Element(BodyCell).Text(Unit(item.UnitOfMeasure));
                            table.Cell().Element(BodyCell).AlignRight().Text(Quantity(item.Quantity));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(item.UnitPrice));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(item.LineTotal));
                        }
                    });

                    col.Item().AlignRight().Width(240).Background(Surface).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text("Ukupno").SemiBold();
                        row.RelativeItem().AlignRight().Text(Money(order.TotalValue)).SemiBold().FontSize(11);
                    });

                    if (!string.IsNullOrWhiteSpace(order.Note))
                    {
                        col.Item().Column(note =>
                        {
                            note.Item().Text("Napomena").FontColor(Muted).FontSize(8);
                            note.Item().Text(order.Note);
                        });
                    }

                    col.Item().PaddingTop(36).Row(row =>
                    {
                        row.RelativeItem().Element(c => Signature(c, "Izdao"));
                        row.ConstantItem(60);
                        row.RelativeItem().Element(c => Signature(c, "Primio"));
                    });
                });

                page.Footer().Element(c => Footer(c, company));
            });
        }).GeneratePdf();
    }

    public static byte[] Tabular(TabularReport report, CompanySettings company)
    {
        var widest = report.Sections.Count == 0 ? 0 : report.Sections.Max(s => s.Columns.Count);
        var landscape = widest > 7;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, landscape);
                page.Header().Element(c => CompanyHeader(c, company, report.Title, report.Subtitle));

                page.Content().PaddingTop(16).Column(col =>
                {
                    col.Spacing(14);

                    foreach (var chunk in report.Summary.Chunk(4))
                    {
                        col.Item().Row(row =>
                        {
                            row.Spacing(8);
                            foreach (var (label, value) in chunk)
                            {
                                row.RelativeItem().Border(1).BorderColor(Line).Padding(8).Column(cell =>
                                {
                                    cell.Item().Text(label).FontSize(8).FontColor(Muted);
                                    cell.Item().Text(value).FontSize(11).SemiBold();
                                });
                            }
                        });
                    }

                    foreach (var section in report.Sections)
                    {
                        col.Item().Column(block =>
                        {
                            if (section.Title is not null)
                                block.Item().PaddingBottom(6).Text(section.Title).FontSize(11).SemiBold();

                            if (section.Rows.Count == 0)
                            {
                                block.Item().Border(1).BorderColor(Line).Padding(12)
                                    .Text("Nema podataka za izabrane filtere.").FontColor(Muted);
                                return;
                            }

                            block.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (var column in section.Columns)
                                        columns.RelativeColumn(column.Width);
                                });

                                table.Header(header =>
                                {
                                    foreach (var column in section.Columns)
                                    {
                                        var cell = header.Cell().Element(HeaderCell);
                                        (column.IsNumeric ? cell.AlignRight() : cell).Text(column.Header).SemiBold();
                                    }
                                });

                                foreach (var row in section.Rows)
                                {
                                    for (var i = 0; i < section.Columns.Count; i++)
                                    {
                                        var column = section.Columns[i];
                                        var cell = table.Cell().Element(BodyCell);
                                        var text = Cell(i < row.Length ? row[i] : null, column.Kind);
                                        (column.IsNumeric ? cell.AlignRight() : cell).Text(text);
                                    }
                                }
                            });
                        });
                    }

                    if (report.Footnote is not null)
                        col.Item().Text(report.Footnote).FontSize(8).FontColor(Muted);
                });

                page.Footer().Element(c => Footer(c, company));
            });
        }).GeneratePdf();
    }

    private static void ConfigurePage(PageDescriptor page, bool landscape)
    {
        page.Size(landscape ? PageSizes.A4.Landscape() : PageSizes.A4);
        page.Margin(34);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(style => style.FontSize(9).FontColor(Ink));
    }

    private static void CompanyHeader(IContainer container, CompanySettings company, string title, string? subtitle)
    {
        container.BorderBottom(1.5f).BorderColor(Accent).PaddingBottom(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(company.Name).FontSize(13).SemiBold().FontColor(Accent);
                col.Item().Text($"{company.Address}, {company.City}").FontColor(Muted);
                col.Item().Text($"PIB {company.TaxNumber} · MB {company.RegistrationNumber} · {company.Phone}").FontColor(Muted);
            });

            row.ConstantItem(260).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text(title).FontSize(15).SemiBold();
                if (!string.IsNullOrWhiteSpace(subtitle))
                    col.Item().AlignRight().Text(subtitle).FontSize(10).FontColor(Muted);
            });
        });
    }

    private static void Party(IContainer container, PartyInfo party)
    {
        container.Border(1).BorderColor(Line).Padding(10).Column(col =>
        {
            col.Item().Text(party.Title.ToUpperInvariant()).FontSize(7).FontColor(Muted).SemiBold();
            col.Item().PaddingTop(3).Text(party.Name).FontSize(11).SemiBold();
            foreach (var line in party.Lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                col.Item().Text(line).FontColor(Muted);
        });
    }

    private static void MetaGrid(IContainer container, IReadOnlyList<(string Label, string Value)> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            foreach (var (label, value) in items)
            {
                table.Cell().PaddingBottom(6).Column(cell =>
                {
                    cell.Item().Text(label).FontSize(7.5f).FontColor(Muted);
                    cell.Item().Text(value);
                });
            }
        });
    }

    private static void Signature(IContainer container, string label)
    {
        container.Column(col =>
        {
            col.Item().Height(28).BorderBottom(1).BorderColor(Ink);
            col.Item().PaddingTop(4).Text(label).FontColor(Muted);
        });
    }

    private static void Footer(IContainer container, CompanySettings company)
    {
        container.PaddingTop(8).Row(row =>
        {
            row.RelativeItem()
                .Text($"{company.Name} · generisano {System.DateTime.Now:dd.MM.yyyy. HH:mm}")
                .FontSize(7).FontColor(Muted);

            row.ConstantItem(120).AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(7).FontColor(Muted));
                text.Span("Strana ");
                text.CurrentPageNumber();
                text.Span(" od ");
                text.TotalPages();
            });
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Surface).BorderBottom(1).BorderColor(Line).PaddingVertical(5).PaddingHorizontal(4);

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(4);
}
