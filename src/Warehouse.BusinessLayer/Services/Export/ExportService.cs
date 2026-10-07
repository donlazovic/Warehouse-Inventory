using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.DTOs.Reports;
using Warehouse.BusinessLayer.Services.Inventory;
using Warehouse.BusinessLayer.Services.Orders;
using Warehouse.BusinessLayer.Services.Reports;
using Warehouse.BusinessLayer.Settings;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;
using static Warehouse.BusinessLayer.Services.Export.ExportFormatting;

namespace Warehouse.BusinessLayer.Services.Export;

public class ExportService : IExportService
{
    private const int MaxRows = 5000;
    private const string PdfType = "application/pdf";
    private const string XlsxType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IOrderService _orders;
    private readonly IStockService _stock;
    private readonly IReportService _reports;
    private readonly IUnitOfWork _uow;
    private readonly CompanySettings _company;

    public ExportService(
        IOrderService orders,
        IStockService stock,
        IReportService reports,
        IUnitOfWork uow,
        IOptions<CompanySettings> company)
    {
        _orders = orders;
        _stock = stock;
        _reports = reports;
        _uow = uow;
        _company = company.Value;
    }

    public async Task<ExportFile> OrderDocumentAsync(int orderId, CancellationToken ct = default)
    {
        var detail = await _orders.GetByIdAsync(orderId, ct);
        var order = detail.Order;

        var companyParty = new Func<string, string?, PartyInfo>((title, location) =>
            new PartyInfo(title, _company.Name, new[] { _company.Address, _company.City, location is null ? null! : $"Lokacija: {location}" }));

        PartyInfo from, to;

        if (order.OrderType == OrderType.Inbound)
        {
            var supplier = await _uow.Repository<Supplier>()
                .Query()
                .Where(x => x.Id == order.SupplierId)
                .Select(x => new { x.Name, x.Address, x.City, x.TaxNumber, x.Phone })
                .FirstOrDefaultAsync(ct);

            from = new PartyInfo(
                "Dobavljač",
                supplier?.Name ?? order.SupplierName ?? "—",
                new[]
                {
                    JoinParts(supplier?.Address, supplier?.City),
                    supplier?.TaxNumber is null ? null! : $"PIB {supplier.TaxNumber}",
                    supplier?.Phone!
                });

            to = companyParty("Primalac", order.DestinationLocationName);
        }
        else
        {
            var store = await _uow.Repository<Store>()
                .Query()
                .Where(x => x.Id == order.StoreId)
                .Select(x => new { x.Code, x.Name, x.Address, x.City, x.ManagerName })
                .FirstOrDefaultAsync(ct);

            from = companyParty("Pošiljalac", order.SourceLocationName);

            to = new PartyInfo(
                "Primalac",
                store is null ? order.StoreName ?? "—" : $"{store.Name} ({store.Code})",
                new[]
                {
                    JoinParts(store?.Address, store?.City),
                    store?.ManagerName is null ? null! : $"Odgovorno lice: {store.ManagerName}",
                    order.DestinationLocationName is null ? null! : $"Lokacija: {order.DestinationLocationName}"
                });
        }

        var content = PdfDocuments.Order(detail, from, to, _company);
        return new ExportFile(content, PdfType, $"{order.OrderNumber}.pdf");
    }

    public async Task<ExportFile> OrdersAsync(OrderFilterRequest filter, ExportFormat format, CancellationToken ct = default)
    {
        filter.ExpandForExport(MaxRows);
        var result = await _orders.GetPagedAsync(filter, ct);

        var report = new TabularReport(
            "Nalozi",
            await OrderFilterLabelAsync(filter, ct),
            new List<(string, string)>
            {
                ("Broj naloga", result.TotalCount.ToString("#,##0", Culture)),
                ("Ukupna vrednost", Money(result.Items.Sum(x => x.TotalValue))),
            },
            new[]
            {
                new TableSection(null,
                    new[]
                    {
                        new ReportColumn("Broj", ColumnKind.Mono, 1.4f),
                        new ReportColumn("Tip", ColumnKind.Text, 0.8f),
                        new ReportColumn("Status", ColumnKind.Text, 1.1f),
                        new ReportColumn("Druga strana", ColumnKind.Text, 1.8f),
                        new ReportColumn("Sa lokacije", ColumnKind.Text, 1.4f),
                        new ReportColumn("Na lokaciju", ColumnKind.Text, 1.4f),
                        new ReportColumn("Stavki", ColumnKind.Integer, 0.6f),
                        new ReportColumn("Vrednost", ColumnKind.Money, 1.3f),
                        new ReportColumn("Kreirao", ColumnKind.Text, 1.2f),
                        new ReportColumn("Kreiran", ColumnKind.DateTime, 1.2f),
                    },
                    result.Items.Select(x => new object?[]
                    {
                        x.OrderNumber,
                        OrderKind(x.OrderType),
                        Status(x.Status),
                        x.SupplierName ?? x.StoreName,
                        x.SourceLocationName,
                        x.DestinationLocationName,
                        x.ItemCount,
                        x.TotalValue,
                        x.CreatedByName,
                        x.CreatedAt,
                    }).ToList())
            },
            Truncated(result.TotalCount));

        return Render(report, format, $"nalozi-{Stamp()}");
    }

    public async Task<ExportFile> StockAsync(StockFilterRequest filter, ExportFormat format, CancellationToken ct = default)
    {
        filter.ExpandForExport(MaxRows);
        var result = await _stock.GetStockAsync(filter, ct);

        var report = new TabularReport(
            "Stanje zaliha",
            await LocationLabelAsync(filter.StorageLocationId, ct),
            new List<(string, string)>
            {
                ("Stavki", result.TotalCount.ToString("#,##0", Culture)),
                ("Ispod minimuma", result.Items.Count(x => x.IsBelowMinimum).ToString(Culture)),
            },
            new[]
            {
                new TableSection(null,
                    new[]
                    {
                        new ReportColumn("SKU", ColumnKind.Mono, 1f),
                        new ReportColumn("Proizvod", ColumnKind.Text, 2.2f),
                        new ReportColumn("Kategorija", ColumnKind.Text, 1.3f),
                        new ReportColumn("Lokacija", ColumnKind.Mono, 1.1f),
                        new ReportColumn("Objekat", ColumnKind.Text, 1.5f),
                        new ReportColumn("Količina", ColumnKind.Quantity, 1f),
                        new ReportColumn("JM", ColumnKind.Text, 0.5f),
                        new ReportColumn("Min", ColumnKind.Quantity, 0.8f),
                        new ReportColumn("Max", ColumnKind.Quantity, 0.8f),
                        new ReportColumn("Napomena", ColumnKind.Text, 1.2f),
                    },
                    result.Items.Select(x => new object?[]
                    {
                        x.ProductSku,
                        x.ProductName,
                        x.CategoryName,
                        x.LocationCode,
                        x.StoreName ?? "Centralni magacin",
                        x.Quantity,
                        Unit(x.UnitOfMeasure),
                        x.EffectiveMinStock,
                        x.EffectiveMaxStock,
                        x.Quantity == 0 ? "Nema na stanju" : x.IsBelowMinimum ? "Ispod minimuma" : null,
                    }).ToList())
            },
            Truncated(result.TotalCount));

        return Render(report, format, $"zalihe-{Stamp()}");
    }

    public async Task<ExportFile> MovementsAsync(StockMovementFilterRequest filter, ExportFormat format, CancellationToken ct = default)
    {
        filter.ExpandForExport(MaxRows);
        var result = await _stock.GetMovementsAsync(filter, ct);

        var report = new TabularReport(
            "Kretanje robe",
            await MovementFilterLabelAsync(filter, ct),
            new List<(string, string)> { ("Zapisa", result.TotalCount.ToString("#,##0", Culture)) },
            new[]
            {
                new TableSection(null,
                    new[]
                    {
                        new ReportColumn("Vreme", ColumnKind.DateTime, 1.3f),
                        new ReportColumn("Tip", ColumnKind.Text, 1f),
                        new ReportColumn("SKU", ColumnKind.Mono, 0.9f),
                        new ReportColumn("Proizvod", ColumnKind.Text, 1.8f),
                        new ReportColumn("Količina", ColumnKind.Quantity, 0.9f),
                        new ReportColumn("JM", ColumnKind.Text, 0.5f),
                        new ReportColumn("Sa", ColumnKind.Text, 1.4f),
                        new ReportColumn("Na", ColumnKind.Text, 1.4f),
                        new ReportColumn("Nalog", ColumnKind.Mono, 1.2f),
                        new ReportColumn("Razlog", ColumnKind.Text, 0.9f),
                        new ReportColumn("Korisnik", ColumnKind.Text, 1.2f),
                    },
                    result.Items.Select(x => new object?[]
                    {
                        x.CreatedAt,
                        Movement(x.MovementType),
                        x.ProductSku,
                        x.ProductName,
                        x.Quantity,
                        Unit(x.UnitOfMeasure),
                        x.FromLocationName ?? (x.MovementType == MovementType.Inbound ? "Dobavljač" : "—"),
                        x.ToLocationName ?? (x.MovementType == MovementType.Outbound ? "Van sistema" : "—"),
                        x.OrderNumber,
                        Reason(x.IssueReason),
                        x.UserName,
                    }).ToList())
            },
            Truncated(result.TotalCount));

        return Render(report, format, $"kretanje-robe-{Stamp()}");
    }

    public async Task<ExportFile> TurnoverAsync(TurnoverRequest request, ExportFormat format, CancellationToken ct = default)
    {
        var data = await _reports.GetTurnoverAsync(request, ct);
        var byLocation = data.LocationId.HasValue;
        var location = await LocationLabelAsync(data.LocationId, ct) ?? "Ceo lanac";

        var columns = new List<ReportColumn>
        {
            new("SKU", ColumnKind.Mono, 0.9f),
            new("Proizvod", ColumnKind.Text, 2f),
            new("JM", ColumnKind.Text, 0.5f),
            new("Početno", ColumnKind.Quantity, 0.9f),
            new("Ulaz", ColumnKind.Quantity, 0.9f),
        };

        if (byLocation)
        {
            columns.Add(new("Prenos +", ColumnKind.Quantity, 0.9f));
            columns.Add(new("Prenos −", ColumnKind.Quantity, 0.9f));
        }

        columns.Add(new("Izlaz", ColumnKind.Quantity, 0.9f));
        columns.Add(new("Korekcija", ColumnKind.Quantity, 0.9f));
        columns.Add(new("Završno", ColumnKind.Quantity, 0.9f));
        columns.Add(new("Vrednost izlaza", ColumnKind.Money, 1.2f));

        var rows = data.Rows.Select(x =>
        {
            var values = new List<object?> { x.Sku, x.Name, Unit(x.UnitOfMeasure), x.Opening, x.Received };
            if (byLocation)
            {
                values.Add(x.TransferredIn);
                values.Add(x.TransferredOut);
            }
            values.Add(x.Issued);
            values.Add(x.AdjustmentNet);
            values.Add(x.Closing);
            values.Add(x.IssuedValue);
            return values.ToArray();
        }).ToList();

        var report = new TabularReport(
            "Promet robe",
            $"{Date(data.From)} — {Date(data.To)} · {location}",
            new List<(string, string)>
            {
                ("Period", $"{Date(data.From)} — {Date(data.To)}"),
                ("Lokacija", location),
                ("Vrednost ulaza", Money(data.TotalReceivedValue)),
                ("Vrednost izlaza", Money(data.TotalIssuedValue)),
            },
            new[] { new TableSection(null, columns, rows) },
            byLocation
                ? "Završno = početno + ulaz + prenos u lokaciju − prenos iz lokacije − izlaz ± korekcija."
                : "Završno = početno + ulaz − izlaz ± korekcija. Prenosi unutar lanca ne menjaju ukupnu količinu.");

        return Render(report, format, $"promet-{Stamp(data.From)}-{Stamp(data.To)}");
    }

    public async Task<ExportFile> SnapshotAsync(SnapshotRequest request, ExportFormat format, CancellationToken ct = default)
    {
        var data = await _reports.GetSnapshotAsync(request, ct);
        var location = await LocationLabelAsync(request.LocationId, ct) ?? "Sve lokacije";

        var report = new TabularReport(
            "Stanje zaliha na dan",
            $"{Date(data.At)} · {location}",
            new List<(string, string)>
            {
                ("Stanje na dan", Date(data.At)),
                ("Lokacija", location),
                ("Stavki", data.Rows.Count.ToString(Culture)),
                ("Ukupna vrednost", Money(data.TotalValue)),
            },
            new[]
            {
                new TableSection(null,
                    new[]
                    {
                        new ReportColumn("Lokacija", ColumnKind.Mono, 1f),
                        new ReportColumn("SKU", ColumnKind.Mono, 0.9f),
                        new ReportColumn("Proizvod", ColumnKind.Text, 2f),
                        new ReportColumn("Kategorija", ColumnKind.Text, 1.2f),
                        new ReportColumn("Količina", ColumnKind.Quantity, 0.9f),
                        new ReportColumn("JM", ColumnKind.Text, 0.5f),
                        new ReportColumn("Cena", ColumnKind.Money, 1f),
                        new ReportColumn("Vrednost", ColumnKind.Money, 1.2f),
                    },
                    data.Rows.Select(x => new object?[]
                    {
                        x.LocationCode, x.Sku, x.Name, x.Category, x.Quantity, Unit(x.UnitOfMeasure), x.Price, x.Value,
                    }).ToList())
            },
            "Stanje je izračunato ponovnim sabiranjem svih kretanja robe do kraja izabranog dana. Vrednost po trenutnim cenama.");

        return Render(report, format, $"stanje-{Stamp(data.At)}");
    }

    public async Task<ExportFile> SupplierActivityAsync(
        int supplierId, SupplierActivityRequest request, ExportFormat format, CancellationToken ct = default)
    {
        var data = await _reports.GetSupplierActivityAsync(supplierId, request, ct);

        var report = new TabularReport(
            "Aktivnost dobavljača",
            $"{data.SupplierName} · {Date(data.From)} — {Date(data.To)}",
            new List<(string, string)>
            {
                ("Nalozi u periodu", data.TotalOrders.ToString(Culture)),
                ("Realizovano", data.CompletedOrders.ToString(Culture)),
                ("Otkazano", data.CancelledOrders.ToString(Culture)),
                ("Vrednost nabavki", Money(data.CompletedValue)),
                ("Prosečno vreme realizacije", data.AverageLeadDays.HasValue ? $"{data.AverageLeadDays:0.#} dana" : "—"),
            },
            new[]
            {
                new TableSection("Po mesecima",
                    new[]
                    {
                        new ReportColumn("Mesec", ColumnKind.Text, 1.5f),
                        new ReportColumn("Realizovanih naloga", ColumnKind.Integer, 1f),
                        new ReportColumn("Vrednost", ColumnKind.Money, 1.2f),
                    },
                    data.Monthly.Select(x => new object?[] { Month(x.Year, x.Month), x.Orders, x.Value }).ToList()),
                new TableSection("Najviše nabavljano",
                    new[]
                    {
                        new ReportColumn("SKU", ColumnKind.Mono, 0.9f),
                        new ReportColumn("Proizvod", ColumnKind.Text, 2f),
                        new ReportColumn("Količina", ColumnKind.Quantity, 1f),
                        new ReportColumn("JM", ColumnKind.Text, 0.5f),
                        new ReportColumn("Vrednost", ColumnKind.Money, 1.2f),
                    },
                    data.TopProducts.Select(x => new object?[]
                    {
                        x.Sku, x.Name, x.Quantity, Unit(x.UnitOfMeasure), x.Value,
                    }).ToList()),
            });

        return Render(report, format, $"dobavljac-{supplierId}-{Stamp(data.From)}-{Stamp(data.To)}");
    }

    private ExportFile Render(TabularReport report, ExportFormat format, string baseName)
        => format == ExportFormat.Pdf
            ? new ExportFile(PdfDocuments.Tabular(report, _company), PdfType, $"{baseName}.pdf")
            : new ExportFile(ExcelDocuments.Tabular(report, _company), XlsxType, $"{baseName}.xlsx");

    private async Task<string?> OrderFilterLabelAsync(OrderFilterRequest filter, CancellationToken ct)
    {
        var parts = new List<string?>
        {
            filter.OrderType.HasValue ? $"Tip: {OrderKind(filter.OrderType.Value)}" : null,
            filter.Status.HasValue ? $"Status: {Status(filter.Status.Value)}" : null,
            filter.SupplierId.HasValue
                ? "Dobavljač: " + await _uow.Repository<Supplier>().Query()
                    .Where(x => x.Id == filter.SupplierId).Select(x => x.Name).FirstOrDefaultAsync(ct)
                : null,
            filter.StoreId.HasValue
                ? "Objekat: " + await _uow.Repository<Store>().Query()
                    .Where(x => x.Id == filter.StoreId).Select(x => x.Name).FirstOrDefaultAsync(ct)
                : null,
            Period(filter.CreatedFrom, filter.CreatedTo),
            string.IsNullOrWhiteSpace(filter.Search) ? null : $"Pretraga: {filter.Search.Trim()}",
        };

        return JoinAll(parts);
    }

    private async Task<string?> MovementFilterLabelAsync(StockMovementFilterRequest filter, CancellationToken ct)
    {
        var parts = new List<string?>
        {
            await LocationLabelAsync(filter.LocationId, ct),
            filter.StoreId.HasValue
                ? "Objekat: " + await _uow.Repository<Store>().Query()
                    .Where(x => x.Id == filter.StoreId).Select(x => x.Name).FirstOrDefaultAsync(ct)
                : null,
            Period(filter.DateFrom, filter.DateTo),
        };

        return JoinAll(parts);
    }

    private static string? Period(System.DateTime? from, System.DateTime? to)
        => from.HasValue || to.HasValue
            ? $"Period: {(from.HasValue ? Date(from.Value) : "početak")} — {(to.HasValue ? Date(to.Value) : "danas")}"
            : null;

    private static string? JoinAll(IEnumerable<string?> parts)
    {
        var text = string.Join(" · ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        return text.Length == 0 ? null : text;
    }

    private async Task<string?> LocationLabelAsync(int? locationId, CancellationToken ct)
    {
        if (!locationId.HasValue)
            return null;

        return await _uow.Repository<StorageLocation>()
            .Query()
            .Where(x => x.Id == locationId)
            .Select(x => x.Code + " — " + x.Name)
            .FirstOrDefaultAsync(ct);
    }

    private static string? Truncated(int total)
        => total > MaxRows ? $"Prikazano prvih {MaxRows:#,##0} od {total:#,##0} redova. Suzite filtere za potpun izvoz." : null;

    private static string JoinParts(string? first, string? second)
        => string.Join(", ", new[] { first, second }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string Stamp() => Stamp(System.DateTime.UtcNow);

    private static string Stamp(System.DateTime value) => Local(value).ToString("yyyy-MM-dd");
}
