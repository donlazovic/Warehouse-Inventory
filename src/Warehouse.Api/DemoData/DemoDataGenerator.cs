using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Warehouse.DataLayer.Context;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Notifications;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.Api.DemoData;

internal sealed class DemoDataGenerator
{
    private const string DemoPassword = "Demo1234!";
    private const string ResetDemoEmail = "lazovicilija02+demo@gmail.com";
    private const int Days = 90;

    private static readonly CultureInfo Sr = CultureInfo.GetCultureInfo("sr-Latn-RS");

    private readonly WarehouseDbContext _db;
    private readonly Random _rng = new(2026);
    private readonly DateTime _now = DateTime.Now;
    private readonly DateTime _firstDay;

    private User _admin = null!;
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Supplier> _suppliers = new();
    private readonly Dictionary<string, Store> _stores = new();
    private readonly Dictionary<string, StorageLocation> _locations = new();
    private readonly Dictionary<string, Category> _categories = new();
    private readonly Dictionary<string, Product> _products = new();

    private readonly Dictionary<(int Product, int Location), decimal> _stock = new();
    private readonly Dictionary<(int Product, int Location), DateTime> _lastMove = new();
    private readonly Dictionary<(int Product, int Location), decimal> _reserved = new();
    private readonly Dictionary<int, decimal> _incoming = new();
    private readonly List<MoveRecord> _moves = new();
    private readonly List<SimOrder> _orders = new();
    private readonly List<Transition> _queue = new();
    private readonly Dictionary<string, int> _numberCounters = new();
    private readonly HashSet<string> _retiredSkus = new();

    public DemoDataGenerator(WarehouseDbContext db)
    {
        _db = db;
        _firstDay = _now.Date.AddDays(-Days);
    }

    // ------------------------------------------------------------------ specifikacije

    private sealed record ProductSpec(
        string Sku, string Name, string Category, UnitOfMeasure Unit, decimal Price,
        decimal Min, decimal Max, decimal Demand, string Zone, string Supplier, int LeadDays);

    private sealed record StoreSpec(
        string Code, string Name, string Address, string City, string Manager, string Phone, decimal Size, bool Active);

    private static readonly (string Name, string? Parent, string Description, bool Active)[] CategorySpecs =
    {
        ("Mlecni proizvodi", null, "Mleko, jogurti, sirevi i kajmak", true),
        ("Mleko", "Mlecni proizvodi", "Sve vrste mleka", true),
        ("Jogurt i kefir", "Mlecni proizvodi", "Fermentisani mlecni proizvodi", true),
        ("Sirevi", "Mlecni proizvodi", "Sirevi i kajmak", true),
        ("Pica", null, "Bezalkoholna pica", true),
        ("Voda", "Pica", "Prirodna i mineralna voda", true),
        ("Sokovi", "Pica", "Vocni sokovi i ledeni caj", true),
        ("Gazirana pica", "Pica", "Gazirani napici", true),
        ("Pekara i testenine", null, "Hleb, testenine i pirinac", true),
        ("Konditorski proizvodi", null, "Cokolade, keks i grickalice", true),
        ("Meso i delikatesi", null, "Sveze meso i suhomesnato", true),
        ("Voce i povrce", null, "Sveze voce i povrce", true),
        ("Kucna hemija", null, "Sredstva za pranje i ciscenje", true),
        ("Licna higijena", null, "Proizvodi za licnu negu", true),
        ("Zimnica i konzerve", null, "Ajvar, turšija i konzervirana hrana", true),
        ("Sezonska ponuda", null, "Privremeni sezonski artikli — trenutno van ponude", false),
    };

    private static readonly ProductSpec[] ProductSpecs =
    {
        new("MLK-001", "Mleko 2,8% 1l", "Mleko", UnitOfMeasure.Liter, 139, 400, 2000, 22, "CW-B", "mlekara", 1),
        new("MLK-002", "Mleko 1,5% 1l", "Mleko", UnitOfMeasure.Liter, 129, 300, 1500, 14, "CW-B", "mlekara", 1),
        new("MLK-003", "Mleko bez laktoze 1l", "Mleko", UnitOfMeasure.Liter, 199, 80, 400, 4, "CW-B", "mlekara", 1),
        new("JOG-001", "Jogurt 2,8% 1l", "Jogurt i kefir", UnitOfMeasure.Liter, 149, 300, 1400, 16, "CW-B", "mlekara", 1),
        new("JOG-002", "Kiselo mleko 500g", "Jogurt i kefir", UnitOfMeasure.Piece, 89, 250, 1200, 14, "CW-B", "mlekara", 1),
        new("JOG-003", "Kefir 1l", "Jogurt i kefir", UnitOfMeasure.Liter, 169, 120, 600, 6, "CW-B", "mlekara", 1),
        new("JOG-004", "Vocni jogurt 150g", "Jogurt i kefir", UnitOfMeasure.Piece, 59, 200, 900, 10, "CW-B", "mlekara", 1),
        new("SIR-001", "Beli sir 1kg", "Sirevi", UnitOfMeasure.Kilogram, 899, 80, 400, 3.5m, "CW-B", "mlekara", 1),
        new("SIR-002", "Gauda u listicima 150g", "Sirevi", UnitOfMeasure.Piece, 299, 100, 500, 5, "CW-B", "mlekara", 1),
        new("SIR-003", "Kajmak 250g", "Sirevi", UnitOfMeasure.Piece, 449, 60, 300, 3, "CW-B", "mlekara", 1),
        new("VOD-001", "Prirodna voda 1,5l", "Voda", UnitOfMeasure.Piece, 69, 600, 3000, 30, "CW-A", "vodoizvor", 3),
        new("VOD-002", "Mineralna voda 1,5l", "Voda", UnitOfMeasure.Piece, 89, 400, 2000, 20, "CW-A", "vodoizvor", 3),
        new("VOD-003", "Voda 0,5l", "Voda", UnitOfMeasure.Piece, 49, 500, 2400, 25, "CW-A", "vodoizvor", 3),
        new("SOK-001", "Sok od pomorandze 1l", "Sokovi", UnitOfMeasure.Piece, 219, 150, 800, 8, "CW-A", "sokovi", 4),
        new("SOK-002", "Sok od jabuke 1l", "Sokovi", UnitOfMeasure.Piece, 189, 150, 800, 7, "CW-A", "sokovi", 4),
        new("SOK-003", "Ledeni caj breskva 1,5l", "Sokovi", UnitOfMeasure.Piece, 159, 120, 600, 6, "CW-A", "sokovi", 4),
        new("GAZ-001", "Kola 2l", "Gazirana pica", UnitOfMeasure.Piece, 199, 200, 1000, 12, "CW-A", "sokovi", 4),
        new("GAZ-002", "Limunada gazirana 1,5l", "Gazirana pica", UnitOfMeasure.Piece, 149, 100, 600, 6, "CW-A", "sokovi", 4),
        new("HLB-001", "Beli hleb 500g", "Pekara i testenine", UnitOfMeasure.Piece, 79, 200, 900, 18, "CW-A", "pekara", 1),
        new("HLB-002", "Integralni hleb 500g", "Pekara i testenine", UnitOfMeasure.Piece, 119, 100, 500, 8, "CW-A", "pekara", 1),
        new("TST-001", "Spagete 500g", "Pekara i testenine", UnitOfMeasure.Piece, 129, 200, 1000, 6, "CW-A", "pekara", 3),
        new("TST-002", "Pirinac 1kg", "Pekara i testenine", UnitOfMeasure.Kilogram, 189, 150, 800, 5, "CW-A", "pekara", 3),
        new("KON-001", "Mlecna cokolada 100g", "Konditorski proizvodi", UnitOfMeasure.Piece, 149, 200, 1200, 10, "CW-A", "konditor", 3),
        new("KON-002", "Keks sa kremom 300g", "Konditorski proizvodi", UnitOfMeasure.Piece, 179, 150, 900, 7, "CW-A", "konditor", 3),
        new("KON-003", "Napolitanke 500g", "Konditorski proizvodi", UnitOfMeasure.Piece, 249, 100, 600, 5, "CW-A", "konditor", 3),
        new("KON-004", "Kikiriki flips 150g", "Konditorski proizvodi", UnitOfMeasure.Piece, 129, 200, 1000, 9, "CW-A", "konditor", 3),
        new("MES-001", "Pileci file 1kg", "Meso i delikatesi", UnitOfMeasure.Kilogram, 799, 100, 500, 5, "CW-B", "mesara", 1),
        new("MES-002", "Pecenica u listicima 100g", "Meso i delikatesi", UnitOfMeasure.Piece, 249, 80, 400, 4, "CW-B", "mesara", 2),
        new("MES-003", "Pileca pasteta 100g", "Meso i delikatesi", UnitOfMeasure.Piece, 99, 150, 800, 6, "CW-A", "mesara", 2),
        new("VOC-001", "Jabuke ajdared 1kg", "Voce i povrce", UnitOfMeasure.Kilogram, 99, 400, 2000, 14, "CW-B", "vocar", 1),
        new("VOC-002", "Banane 1kg", "Voce i povrce", UnitOfMeasure.Kilogram, 189, 300, 1500, 12, "CW-B", "vocar", 2),
        new("VOC-003", "Krompir 1kg", "Voce i povrce", UnitOfMeasure.Kilogram, 79, 500, 2500, 15, "CW-A", "vocar", 2),
        new("VOC-004", "Crni luk 1kg", "Voce i povrce", UnitOfMeasure.Kilogram, 89, 200, 1000, 6, "CW-A", "vocar", 2),
        new("HEM-001", "Deterdzent za ves 3kg", "Kucna hemija", UnitOfMeasure.Piece, 899, 60, 300, 2, "CW-C", "hemija", 4),
        new("HEM-002", "Tecnost za sudove 1l", "Kucna hemija", UnitOfMeasure.Piece, 199, 100, 600, 4, "CW-C", "hemija", 4),
        new("HEM-003", "Omeksivac 2l", "Kucna hemija", UnitOfMeasure.Piece, 349, 60, 350, 2.5m, "CW-C", "hemija", 4),
        new("HIG-001", "Toalet papir 10/1", "Licna higijena", UnitOfMeasure.Package, 399, 100, 600, 5, "CW-C", "higija", 3),
        new("HIG-002", "Sampon 400ml", "Licna higijena", UnitOfMeasure.Piece, 329, 80, 400, 3, "CW-C", "higija", 3),
        new("HIG-003", "Pasta za zube 75ml", "Licna higijena", UnitOfMeasure.Piece, 189, 100, 500, 4, "CW-C", "higija", 3),
        new("HIG-004", "Sapun 100g", "Licna higijena", UnitOfMeasure.Piece, 79, 150, 800, 5, "CW-C", "higija", 3),
        new("AJV-001", "Ajvar ljuti 680g", "Zimnica i konzerve", UnitOfMeasure.Piece, 449, 50, 300, 0, "CW-A", "konditor", 3),
    };

    private static readonly StoreSpec[] StoreSpecs =
    {
        new("NIS-01", "Prodavnica Nis Centar", "Obrenoviceva 24", "Nis", "Vesna Ristic", "018 512 340", 1.3m, true),
        new("NIS-02", "Prodavnica Nis Medijana", "Bulevar Medijana 15", "Nis", "Goran Pesic", "018 233 410", 1.0m, true),
        new("NIS-03", "Prodavnica Nis Pantelej", "Vizantijski bulevar 102", "Nis", "Ivana Todorovic", "018 455 902", 0.9m, true),
        new("LES-01", "Prodavnica Leskovac", "Bulevar oslobodjenja 38", "Leskovac", "Sasa Velickovic", "016 242 118", 1.0m, true),
        new("PIR-01", "Prodavnica Pirot", "Srpskih vladara 61", "Pirot", "Dejan Manic", "010 321 554", 0.7m, true),
        new("VRA-01", "Prodavnica Vranje", "Kralja Stefana Prvovencanog 77", "Vranje", "Marija Zlatkovic", "017 414 226", 0.8m, true),
        new("ALE-01", "Prodavnica Aleksinac", "Knjaza Milosa 12", "Aleksinac", "Bojan Ristic", "018 804 330", 0.6m, false),
    };

    // ------------------------------------------------------------------ simulacioni tipovi

    private sealed record MoveRecord(
        DateTime At, MovementType Type, int ProductId, decimal Quantity, int? From, int? To,
        SimOrder? Order, int UserId, string? Note, IssueReason? Reason);

    private sealed class SimOrder
    {
        public required Order Entity { get; init; }
        public required string Store { get; init; }
        public required string Counterparty { get; init; }
        public DateTime PendingAt { get; set; }
    }

    private sealed record Transition(DateTime At, SimOrder Order, OrderStatus To, User By, string? Note);

    // ------------------------------------------------------------------ glavni tok

    public async Task<string> RunAsync()
    {
        _db.ChangeTracker.AutoDetectChangesEnabled = false;

        await CreateMasterDataAsync();
        Simulate();
        await PersistAsync();

        var completed = _orders.Count(x => x.Entity.Status == OrderStatus.Completed);
        return $"""
            Demo podaci su generisani:
              korisnika ............ {_users.Count + 1}
              artikala ............. {_products.Count}
              naloga ............... {_orders.Count} (realizovano {completed}, u toku {_orders.Count(x => x.Entity.Status is not (OrderStatus.Completed or OrderStatus.Cancelled))}, otkazano {_orders.Count(x => x.Entity.Status == OrderStatus.Cancelled)})
              kretanja robe ........ {_moves.Count}
            Lozinka svih demo korisnika: {DemoPassword}
            """;
    }

    // ------------------------------------------------------------------ maticni podaci

    private async Task CreateMasterDataAsync()
    {
        var created = Utc(_firstDay.AddDays(-5).AddHours(9));

        _admin = await _db.Users.Include(x => x.Role).FirstAsync(x => x.IsOwner || x.Email == "admin@warehouse.local");
        var roles = await _db.Roles.ToDictionaryAsync(x => x.Name, x => x.Id);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        foreach (var (name, parent, description, active) in CategorySpecs.Where(x => x.Parent is null))
            _categories[name] = new Category { Name = name, Description = description, IsActive = active, CreatedAt = created };
        _db.Categories.AddRange(_categories.Values);
        await SaveAsync();

        foreach (var (name, parent, description, active) in CategorySpecs.Where(x => x.Parent is not null))
            _categories[name] = new Category
            {
                Name = name,
                Description = description,
                IsActive = active,
                ParentCategoryId = _categories[parent!].Id,
                CreatedAt = created
            };
        _db.Categories.AddRange(_categories.Values.Where(x => x.Id == 0));
        await SaveAsync();

        AddSupplier("mlekara", "Mlekara Juzna Morava d.o.o.", "101234567", "Zoran Stojanovic", "prodaja@mlekarajm.rs", "016 245 112", "Industrijska zona bb", "Leskovac", true);
        AddSupplier("pekara", "Pekara Nisava d.o.o.", "102345678", "Milan Djokic", "porudzbine@pekaranisava.rs", "018 455 120", "Bulevar 12. februara 80", "Nis", true);
        AddSupplier("vodoizvor", "Vodoizvor Rtanj a.d.", "103456789", "Snezana Milic", "komercijala@vodoizvor-rtanj.rs", "030 461 300", "Rtanjska bb", "Boljevac", true);
        AddSupplier("sokovi", "Fructa Sokovi d.o.o.", "104567890", "Nikola Arsic", "office@fructa.rs", "037 444 210", "Kosovska 45", "Krusevac", true);
        AddSupplier("konditor", "Slatki Kutak d.o.o.", "105678901", "Tamara Radic", "prodaja@slatkikutak.rs", "011 330 4410", "Autoput za Zagreb 21", "Beograd", true);
        AddSupplier("mesara", "Mesara Zlatar d.o.o.", "106789012", "Rade Sekulic", "narudzbe@mesarazlatar.rs", "033 64 210", "Zlatarska 3", "Nova Varos", true);
        AddSupplier("vocar", "Vocar Toplicanin", "107890123", "Dusan Ilic", "vocartoplicanin@gmail.com", "027 321 900", "Pasjacka 18", "Prokuplje", true);
        AddSupplier("hemija", "DeterSan Hemija d.o.o.", "108901234", "Ljiljana Pavlovic", "prodaja@detersan.rs", "015 350 870", "Hajduk Veljkova 9", "Sabac", true);
        AddSupplier("higija", "Higija Plus d.o.o.", "109012345", "Branislav Kovac", "kupci@higijaplus.rs", "021 480 3300", "Rumenacki put 71", "Novi Sad", true);
        AddSupplier("stari", "Distributer Stari Grad d.o.o.", "100987654", "Petar Jankovic", "info@starigrad-distribucija.rs", "018 260 015", "Dusanova 4", "Nis", false);
        _db.Suppliers.AddRange(_suppliers.Values);

        foreach (var spec in StoreSpecs)
        {
            _stores[spec.Code] = new Store
            {
                Code = spec.Code,
                Name = spec.Name,
                Address = spec.Address,
                City = spec.City,
                ManagerName = spec.Manager,
                Phone = spec.Phone,
                IsActive = spec.Active,
                Email = $"{spec.Code.ToLowerInvariant()}@skladisnik.rs",
                CreatedAt = created
            };
        }
        _db.Stores.AddRange(_stores.Values);
        await SaveAsync();

        AddLocation("CW-A", "Centralni magacin — zona A (suvi asortiman)", "A", LocationType.CentralWarehouse, null, true);
        AddLocation("CW-B", "Centralni magacin — zona B (rashladna komora)", "B", LocationType.CentralWarehouse, null, true);
        AddLocation("CW-C", "Centralni magacin — zona C (kucna hemija i higijena)", "C", LocationType.CentralWarehouse, null, true);
        AddLocation("CW-D", "Centralni magacin — zona D (u renoviranju)", "D", LocationType.CentralWarehouse, null, false);

        foreach (var spec in StoreSpecs)
            AddLocation($"{spec.Code}-MAIN", $"{spec.Name} - prodajni prostor", null, LocationType.Store, spec.Code, spec.Active);

        AddLocation("NIS-01-BACK", "Prodavnica Nis Centar - magacin objekta", null, LocationType.Store, "NIS-01", true);
        _db.StorageLocations.AddRange(_locations.Values);

        AddUser("milica", "Milica", "Stankovic", "milica.stankovic@skladisnik.rs", roles["Menadzer"], -95, true);
        AddUser("nenad", "Nenad", "Jovanovic", "nenad.jovanovic@skladisnik.rs", roles["Menadzer"], -95, true);
        AddUser("marko", "Marko", "Petrovic", "marko.petrovic@skladisnik.rs", roles["Magacioner"], -95, true);
        AddUser("stefan", "Stefan", "Ilic", "stefan.ilic@skladisnik.rs", roles["Magacioner"], -95, true);
        AddUser("ana", "Ana", "Djordjevic", "ana.djordjevic@skladisnik.rs", roles["Magacioner"], -90, true);
        AddUser("ivan", "Ivan", "Kostic", "ivan.kostic@skladisnik.rs", roles["Magacioner"], -95, false);
        AddUser("jovan", "Jovan", "Nikolic", ResetDemoEmail, roles["Magacioner"], -20, true);
        AddUser("jelena", "Jelena", "Pavlovic", "jelena.pavlovic@skladisnik.rs", roles["Viewer"], -80, true);
        AddUser("dragan", "Dragan", "Mitic", "dragan.mitic@skladisnik.rs", roles["Viewer"], -30, true);
        AddPendingUser("petar", "Petar", "Markovic", "petar.markovic@skladisnik.rs", roles["Viewer"], _now.AddHours(-26));
        AddPendingUser("sanja", "Sanja", "Lazic", "sanja.lazic@skladisnik.rs", roles["Viewer"], _now.AddHours(-3));

        foreach (var user in _users.Values)
            user.PasswordHash = passwordHash;

        _db.Users.AddRange(_users.Values);
        await SaveAsync();

        foreach (var spec in ProductSpecs)
        {
            var isNew = spec.Demand == 0;
            _products[spec.Sku] = new Product
            {
                Sku = spec.Sku,
                Name = spec.Name,
                UnitOfMeasure = spec.Unit,
                Price = spec.Price,
                MinStock = spec.Min,
                MaxStock = spec.Max,
                CategoryId = _categories[spec.Category].Id,
                IsActive = true,
                Description = isNew ? "Novi artikal — prva nabavka u pripremi." : null,
                CreatedAt = isNew ? Utc(_now.AddDays(-2)) : created
            };
        }
        _db.Products.AddRange(_products.Values);
        await SaveAsync();
    }

    private void AddSupplier(string key, string name, string pib, string contact, string email, string phone, string address, string city, bool active)
        => _suppliers[key] = new Supplier
        {
            Name = name,
            TaxNumber = pib,
            ContactPerson = contact,
            Email = email,
            Phone = phone,
            Address = address,
            City = city,
            IsActive = active,
            CreatedAt = Utc(_firstDay.AddDays(-5).AddHours(9))
        };

    private void AddLocation(string code, string name, string? zone, LocationType type, string? store, bool active)
        => _locations[code] = new StorageLocation
        {
            Code = code,
            Name = name,
            Zone = zone,
            LocationType = type,
            IsActive = active,
            StoreId = store is null ? null : _stores[store].Id,
            CreatedAt = Utc(_firstDay.AddDays(-5).AddHours(9))
        };

    private void AddUser(string key, string first, string last, string email, int roleId, int createdDaysAgo, bool active)
    {
        var createdAt = _now.Date.AddDays(createdDaysAgo).AddHours(9);
        _users[key] = new User
        {
            FirstName = first,
            LastName = last,
            Email = email,
            RoleId = roleId,
            IsActive = active,
            CreatedAt = Utc(createdAt),
            ApprovedAt = Utc(createdAt.AddMinutes(20)),
            ApprovedByUserId = _admin.Id,
            LastLoginAt = active ? Utc(_now.AddHours(-_rng.Next(1, 30))) : Utc(_firstDay.AddDays(44).AddHours(15)),
            UpdatedAt = active ? null : Utc(_firstDay.AddDays(45).AddHours(10))
        };
    }

    private void AddPendingUser(string key, string first, string last, string email, int roleId, DateTime createdAt)
        => _users[key] = new User
        {
            FirstName = first,
            LastName = last,
            Email = email,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = Utc(createdAt),
            ApprovedAt = null
        };

    // ------------------------------------------------------------------ simulacija

    private void Simulate()
    {
        InitialStock();

        for (var day = 0; day <= Days; day++)
        {
            var date = _firstDay.AddDays(day);

            ProcessUntil(date.AddHours(7).AddMinutes(55));

            if (day > 0)
            {
                ReplenishStores(day, date.AddHours(8));
                ReplenishWarehouse(day, date.AddHours(8).AddMinutes(30));
            }

            ProcessUntil(date.AddHours(18).AddMinutes(55));

            if (day is 30 or 60)
                StockCount(date.AddHours(19));

            if (day == 60)
                RetireProduct("JOG-004", date.AddHours(18));

            RecordSales(day, date.AddHours(20).AddMinutes(30));
            RecordLosses(day, date.AddHours(21));

            ProcessUntil(date.AddHours(23).AddMinutes(59));
        }

        TodayManualOrders();
        ProcessUntil(_now);
    }

    private void InitialStock()
    {
        var at = _firstDay.AddHours(6);
        const string note = "Pocetno stanje pri uvodjenju sistema";

        foreach (var spec in Stocked())
        {
            var product = _products[spec.Sku];
            var share = spec.Sku == "HEM-003" ? 1.3m : 0.65m;
            var qty = RoundFor(spec.Unit, spec.Max * share);
            Move(at, MovementType.InitialStock, product.Id, qty, null, Loc(spec.Zone), _admin.Id,
                spec.Sku == "HEM-003" ? "Pocetno stanje — visak sa akcijske nabavke" : note);

            foreach (var store in ActiveStores())
            {
                if (!Carries(store, spec)) continue;
                var (_, max) = StoreLimits(spec, store);
                Move(at.AddMinutes(30), MovementType.InitialStock, product.Id, RoundFor(spec.Unit, max * 0.75m),
                    null, Loc($"{store.Code}-MAIN"), _admin.Id, note);
            }
        }

        foreach (var (sku, qty) in new[] { ("VOD-001", 60m), ("GAZ-001", 40m), ("KON-004", 50m) })
            Move(at.AddMinutes(45), MovementType.InitialStock, _products[sku].Id, qty, null, Loc("NIS-01-BACK"), _admin.Id, note);
    }

    private void ReplenishStores(int day, DateTime at)
    {
        var index = 0;

        foreach (var store in ActiveStores())
        {
            var storeLoc = Loc($"{store.Code}-MAIN");

            var low = Stocked()
                .Where(spec => Carries(store, spec) && !_retiredSkus.Contains(spec.Sku))
                .Where(spec => Stock(_products[spec.Sku].Id, storeLoc) < StoreLimits(spec, store).Min)
                .GroupBy(spec => spec.Zone);

            foreach (var zoneGroup in low)
            {
                var zoneLoc = Loc(zoneGroup.Key);
                if (HasOpenOrder(OrderType.Outbound, store.Code, zoneLoc)) continue;

                var lines = new List<(int Product, decimal Qty, decimal Price)>();
                foreach (var spec in zoneGroup)
                {
                    var product = _products[spec.Sku];
                    var need = StoreLimits(spec, store).Max - Stock(product.Id, storeLoc);
                    var available = Stock(product.Id, zoneLoc) - Reserved(product.Id, zoneLoc);
                    var qty = RoundFor(spec.Unit, Math.Min(need, available));
                    if (qty > 0) lines.Add((product.Id, qty, spec.Price));
                }

                if (lines.Count == 0) continue;

                var createdAt = at.AddMinutes(index++ * 4 + _rng.Next(0, 3));
                if (createdAt > _now) continue;

                var creator = Pick(Warehousemen(day));
                var order = CreateOrder(OrderType.Outbound, createdAt, creator, null, store.Code, zoneLoc, storeLoc, lines,
                    _rng.NextDouble() < 0.15 ? "Hitna dopuna pred vikend" : "Redovna dopuna objekta",
                    store.Name);

                ScheduleOutbound(order, creator, day);
            }
        }
    }

    private void ReplenishWarehouse(int day, DateTime at)
    {
        var needs = new List<(ProductSpec Spec, decimal Qty)>();

        foreach (var spec in Stocked().Where(s => !_retiredSkus.Contains(s.Sku)))
        {
            var product = _products[spec.Sku];
            var have = Stock(product.Id, Loc(spec.Zone));
            var incoming = _incoming.GetValueOrDefault(product.Id);

            if (have + incoming >= spec.Min) continue;

            var qty = RoundFor(spec.Unit, spec.Max - have - incoming);
            if (qty > 0) needs.Add((spec, qty));
        }

        var index = 0;
        foreach (var group in needs.GroupBy(x => (Supplier: SupplierFor(x.Spec, day), x.Spec.Zone)))
        {
            var createdAt = at.AddMinutes(index++ * 6);
            if (createdAt > _now) continue;

            var supplier = _suppliers[group.Key.Supplier];
            var lines = group.Select(x => (_products[x.Spec.Sku].Id, x.Qty, Math.Round(x.Spec.Price * 0.72m, 2))).ToList();
            var creator = Pick(Warehousemen(day));
            var lead = group.Max(x => x.Spec.LeadDays);

            var order = CreateOrder(OrderType.Inbound, createdAt, creator, supplier, null, null, Loc(group.Key.Zone), lines,
                lead <= 1 ? "Dnevna nabavka svezeg asortimana" : "Nabavka za popunu magacina", supplier.Name);

            foreach (var line in lines)
                _incoming[line.Item1] = _incoming.GetValueOrDefault(line.Item1) + line.Qty;

            ScheduleInbound(order, creator, day, lead);
        }
    }

    private void ScheduleOutbound(SimOrder order, User creator, int day)
    {
        var created = Local(order.Entity.CreatedAt);
        var pending = created.AddMinutes(8);
        order.PendingAt = pending;
        Enqueue(pending, order, OrderStatus.PendingApproval, creator, null);

        var manager = PickManager();
        var decisionAt = created.Date.AddHours(10).AddMinutes(_rng.Next(0, 120));
        var roll = _rng.NextDouble();

        if (roll < 0.04)
        {
            Enqueue(decisionAt, order, OrderStatus.Cancelled, manager,
                Pick(new[] { "Zalihe vec dopunjene prethodnim nalogom", "Duplikat naloga", "Objekat privremeno zatvoren zbog inventara" }));
            return;
        }

        var approvedAt = decisionAt;
        if (roll < 0.08)
        {
            Enqueue(decisionAt, order, OrderStatus.Draft, manager,
                Pick(new[] { "Smanjiti kolicinu vode — nema mesta u objektu", "Proveriti kolicine pre slanja" }));
            Enqueue(decisionAt.AddMinutes(40), order, OrderStatus.PendingApproval, creator, "Kolicine provereni");
            approvedAt = decisionAt.AddMinutes(95);
        }

        Enqueue(approvedAt, order, OrderStatus.Approved, manager, _rng.NextDouble() < 0.2 ? "Odobreno" : null);

        var worker = Pick(Warehousemen(day));
        var pickingAt = approvedAt.AddMinutes(_rng.Next(80, 160));
        Enqueue(pickingAt, order, OrderStatus.InProgress, worker, null);

        var deliveredAt = pickingAt.AddMinutes(_rng.Next(90, 200));
        if (day == Days - 1 && _rng.NextDouble() < 0.45)
            deliveredAt = _now.AddHours(3);

        Enqueue(deliveredAt, order, OrderStatus.Completed, worker, _rng.NextDouble() < 0.1 ? "Isporuceno uz manje kasnjenje" : null);
    }

    private void ScheduleInbound(SimOrder order, User creator, int day, int leadDays)
    {
        var created = Local(order.Entity.CreatedAt);
        order.PendingAt = created.AddMinutes(5);
        Enqueue(order.PendingAt, order, OrderStatus.PendingApproval, creator, null);

        var manager = PickManager();
        var decisionAt = created.Date.AddHours(10).AddMinutes(30 + _rng.Next(0, 120));

        if (_rng.NextDouble() < 0.03)
        {
            Enqueue(decisionAt, order, OrderStatus.Cancelled, manager, "Dobavljac trenutno nema robu na stanju");
            return;
        }

        Enqueue(decisionAt, order, OrderStatus.Approved, manager, null);

        var deliveryDay = created.Date.AddDays(leadDays);
        var worker = Pick(Warehousemen(day));
        var unloading = deliveryDay.AddHours(6).AddMinutes(_rng.Next(0, 40));
        Enqueue(unloading, order, OrderStatus.InProgress, worker, "Roba stigla, istovar u toku");
        Enqueue(unloading.AddMinutes(_rng.Next(30, 75)), order, OrderStatus.Completed, worker, null);
    }

    private void TodayManualOrders()
    {
        var day = Days;
        var start = new[] { _now.AddMinutes(-55), _now.AddMinutes(-25) };

        var drafts = new[] { ("NIS-02", "CW-A", new[] { "SOK-001", "KON-002", "TST-001" }), ("LES-01", "CW-C", new[] { "HEM-002", "HIG-001" }) };
        for (var i = 0; i < drafts.Length; i++)
        {
            var (storeCode, zone, skus) = drafts[i];
            var storeLoc = Loc($"{storeCode}-MAIN");
            var zoneLoc = Loc(zone);
            var lines = skus
                .Select(sku => ProductSpecs.First(s => s.Sku == sku))
                .Select(spec => (_products[spec.Sku].Id, RoundFor(spec.Unit, spec.Demand * 4), spec.Price))
                .Where(line => Stock(line.Item1, zoneLoc) - Reserved(line.Item1, zoneLoc) >= line.Item2)
                .ToList();

            if (lines.Count == 0 || start[i] < _now.Date) continue;

            CreateOrder(OrderType.Outbound, start[i], Pick(Warehousemen(day)), null, storeCode, zoneLoc, storeLoc, lines,
                "U pripremi — dopuna za promotivnu akciju", _stores[storeCode].Name);
        }

        var pendingAt = _now.AddMinutes(-40);
        if (pendingAt >= _now.Date)
        {
            var spec = ProductSpecs.First(s => s.Sku == "HIG-002");
            var supplier = _suppliers["higija"];
            var creator = Pick(Warehousemen(day));
            var order = CreateOrder(OrderType.Inbound, pendingAt, creator, supplier, null, null, Loc("CW-C"),
                new List<(int, decimal, decimal)> { (_products[spec.Sku].Id, 120, Math.Round(spec.Price * 0.72m, 2)) },
                "Dodatna nabavka — najavljena akcija", supplier.Name);
            Enqueue(pendingAt.AddMinutes(4), order, OrderStatus.PendingApproval, creator, null);
            order.PendingAt = pendingAt.AddMinutes(4);
        }
    }

    private void RecordSales(int day, DateTime at)
    {
        if (at > _now) return;

        var weekday = at.DayOfWeek switch
        {
            DayOfWeek.Saturday => 1.3m,
            DayOfWeek.Sunday => 0.75m,
            DayOfWeek.Friday => 1.15m,
            _ => 1m
        };

        foreach (var store in ActiveStores())
        {
            var loc = Loc($"{store.Code}-MAIN");
            var recorder = Pick(Warehousemen(day));

            foreach (var spec in Stocked().Where(s => Carries(store, s) && !_retiredSkus.Contains(s.Sku)))
            {
                if (_rng.NextDouble() > 0.85) continue;

                var factor = (decimal)(0.6 + _rng.NextDouble() * 0.8);
                var qty = RoundFor(spec.Unit, spec.Demand * store.Size * weekday * factor);
                if (qty <= 0) continue;

                Move(at, MovementType.Outbound, _products[spec.Sku].Id, qty, loc, null, recorder.Id, "Dnevni promet", IssueReason.Sale);
            }
        }
    }

    private void RecordLosses(int day, DateTime at)
    {
        if (at > _now) return;

        var dairy = Stocked().Where(s => s.Zone == "CW-B" && !_retiredSkus.Contains(s.Sku)).ToList();
        var drinks = Stocked().Where(s => s.Sku.StartsWith("VOD") || s.Sku.StartsWith("GAZ") || s.Sku.StartsWith("SOK")).ToList();
        var cleaning = Stocked().Where(s => s.Sku is "HEM-002" or "HIG-004").ToList();

        foreach (var store in ActiveStores())
        {
            var loc = Loc($"{store.Code}-MAIN");
            var recorder = Pick(Warehousemen(day));

            var carriedDairy = dairy.Where(s => Carries(store, s)).ToList();
            if (carriedDairy.Count > 0 && _rng.NextDouble() < 0.12)
            {
                var spec = Pick(carriedDairy);
                Move(at, MovementType.Outbound, _products[spec.Sku].Id, RoundFor(spec.Unit, _rng.Next(1, 5)), loc, null,
                    recorder.Id, "Istekao rok trajanja", IssueReason.WriteOff);
            }

            if (_rng.NextDouble() < 0.03)
            {
                var spec = Pick(drinks);
                Move(at.AddMinutes(5), MovementType.Outbound, _products[spec.Sku].Id, _rng.Next(1, 4), loc, null,
                    recorder.Id, Pick(new[] { "Lom flase pri slaganju na rafove", "Osteceno pakovanje pri istovaru" }), IssueReason.Damage);
            }

            if (_rng.NextDouble() < 0.04)
            {
                var spec = Pick(cleaning);
                Move(at.AddMinutes(10), MovementType.Outbound, _products[spec.Sku].Id, 1, loc, null,
                    recorder.Id, "Odrzavanje higijene prodajnog prostora", IssueReason.InternalUse);
            }
        }

        if (_rng.NextDouble() < 0.05)
        {
            var spec = Pick(drinks);
            var product = _products[spec.Sku].Id;
            var free = Stock(product, Loc(spec.Zone)) - Reserved(product, Loc(spec.Zone));
            Move(at.AddMinutes(20), MovementType.Outbound, product, Math.Min(_rng.Next(4, 13), Math.Floor(free)), Loc(spec.Zone), null,
                Pick(Warehousemen(day)).Id, "Ostecena paleta pri manipulaciji viljuskarom", IssueReason.Damage);
        }
    }

    private void StockCount(DateTime at)
    {
        if (at > _now) return;

        var manager = PickManager();
        var label = at.ToString("dd.MM.yyyy.", Sr);

        foreach (var spec in Stocked().OrderBy(_ => _rng.Next()).Take(7))
        {
            var product = _products[spec.Sku];
            var loc = Loc(spec.Zone);
            var have = Stock(product.Id, loc);
            if (have <= 10) continue;

            var surplus = _rng.NextDouble() < 0.25;
            var delta = RoundFor(spec.Unit, Math.Max(1, have * (decimal)(0.004 + _rng.NextDouble() * 0.015)));
            if (!surplus) delta = Math.Min(delta, Math.Floor(have - Reserved(product.Id, loc)));
            if (delta <= 0) continue;

            Move(at, MovementType.Adjustment, product.Id, delta,
                surplus ? null : loc, surplus ? loc : null, manager.Id,
                $"Popis {label} — utvrdjen {(surplus ? "visak" : "manjak")}");
        }
    }

    private void RetireProduct(string sku, DateTime at)
    {
        if (at > _now) return;

        var product = _products[sku];
        _retiredSkus.Add(sku);

        foreach (var key in _stock.Keys.Where(k => k.Product == product.Id).ToList())
        {
            var qty = _stock[key];
            if (qty > 0)
                Move(at, MovementType.Outbound, product.Id, qty, key.Location, null, PickManager().Id,
                    "Povlacenje artikla iz asortimana", IssueReason.WriteOff);
        }

        product.IsActive = false;
        product.Description = "Povucen iz asortimana — zamenjen novim pakovanjem.";
        product.UpdatedAt = Utc(at);
    }

    // ------------------------------------------------------------------ nalozi i prelazi

    private SimOrder CreateOrder(
        OrderType type, DateTime at, User creator, Supplier? supplier, string? storeCode,
        int? sourceLoc, int? destinationLoc, List<(int Product, decimal Qty, decimal Price)> lines,
        string note, string counterparty)
    {
        var prefix = type == OrderType.Inbound ? "ULZ" : "IZL";
        var key = $"{prefix}-{at.Year}";
        _numberCounters[key] = _numberCounters.GetValueOrDefault(key) + 1;

        var order = new Order
        {
            OrderNumber = $"{key}-{_numberCounters[key]:D5}",
            OrderType = type,
            Status = OrderStatus.Draft,
            SupplierId = supplier?.Id,
            StoreId = storeCode is null ? null : _stores[storeCode].Id,
            SourceLocationId = sourceLoc,
            DestinationLocationId = destinationLoc,
            Note = note,
            CreatedByUserId = creator.Id,
            CreatedAt = Utc(at),
            UpdatedAt = Utc(at),
            Items = lines.Select(line => new OrderItem
            {
                ProductId = line.Product,
                Quantity = line.Qty,
                UnitPrice = line.Price,
                LineTotal = Math.Round(line.Qty * line.Price, 2),
                CreatedAt = Utc(at)
            }).ToList()
        };

        order.TotalValue = order.Items.Sum(x => x.LineTotal);
        order.StatusHistory.Add(new OrderStatusHistory
        {
            FromStatus = null,
            ToStatus = OrderStatus.Draft,
            ChangedByUserId = creator.Id,
            Note = "Nalog kreiran",
            CreatedAt = Utc(at)
        });

        if (type == OrderType.Outbound && sourceLoc.HasValue)
            foreach (var line in lines)
                _reserved[(line.Product, sourceLoc.Value)] = Reserved(line.Product, sourceLoc.Value) + line.Qty;

        var sim = new SimOrder { Entity = order, Store = storeCode ?? string.Empty, Counterparty = counterparty };
        _orders.Add(sim);
        return sim;
    }

    private void Enqueue(DateTime at, SimOrder order, OrderStatus to, User by, string? note)
        => _queue.Add(new Transition(at, order, to, by, note));

    private void ProcessUntil(DateTime until)
    {
        var limit = until < _now ? until : _now;

        while (true)
        {
            var next = _queue.Where(t => t.At <= limit).OrderBy(t => t.At).FirstOrDefault();
            if (next is null) return;

            _queue.Remove(next);
            Apply(next);
        }
    }

    private void Apply(Transition t)
    {
        var order = t.Order.Entity;
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled) return;

        var from = order.Status;
        order.Status = t.To;
        order.UpdatedAt = Utc(t.At);
        order.StatusHistory.Add(new OrderStatusHistory
        {
            FromStatus = from,
            ToStatus = t.To,
            ChangedByUserId = t.By.Id,
            Note = t.Note,
            CreatedAt = Utc(t.At)
        });

        if (t.To == OrderStatus.PendingApproval)
            t.Order.PendingAt = t.At;

        if (t.To == OrderStatus.Approved)
        {
            order.ApprovedByUserId = t.By.Id;
            order.ApprovedAt = Utc(t.At);
        }

        if (t.To == OrderStatus.Cancelled)
        {
            _queue.RemoveAll(x => x.Order == t.Order);
            ReleaseCommitments(order);
        }

        if (t.To != OrderStatus.Completed) return;

        order.CompletedAt = Utc(t.At);
        var note = $"Realizacija naloga {order.OrderNumber}";

        foreach (var item in order.Items)
        {
            if (order.OrderType == OrderType.Inbound)
            {
                Move(t.At, MovementType.Inbound, item.ProductId, item.Quantity, null, order.DestinationLocationId, t.By.Id, note, null, t.Order);
            }
            else
            {
                Move(t.At, MovementType.Transfer, item.ProductId, item.Quantity, order.SourceLocationId, order.DestinationLocationId, t.By.Id, note, null, t.Order);
            }
        }

        ReleaseCommitments(order);
    }

    private void ReleaseCommitments(Order order)
    {
        foreach (var item in order.Items)
        {
            if (order.OrderType == OrderType.Outbound && order.SourceLocationId.HasValue)
            {
                var key = (item.ProductId, order.SourceLocationId.Value);
                _reserved[key] = Math.Max(0, Reserved(key.ProductId, key.Value) - item.Quantity);
            }
            else if (order.OrderType == OrderType.Inbound)
            {
                _incoming[item.ProductId] = Math.Max(0, _incoming.GetValueOrDefault(item.ProductId) - item.Quantity);
            }
        }
    }

    private bool HasOpenOrder(OrderType type, string storeCode, int sourceLoc)
        => _orders.Any(o => o.Entity.OrderType == type && o.Store == storeCode && o.Entity.SourceLocationId == sourceLoc
                         && o.Entity.Status is not (OrderStatus.Completed or OrderStatus.Cancelled));

    // ------------------------------------------------------------------ kretanje robe

    private void Move(DateTime at, MovementType type, int productId, decimal qty, int? from, int? to,
        int userId, string? note, IssueReason? reason = null, SimOrder? order = null)
    {
        if (at > _now || qty <= 0) return;

        if (from.HasValue)
        {
            var have = Stock(productId, from.Value);
            if (have < qty) qty = have;
            if (qty <= 0) return;

            _stock[(productId, from.Value)] = have - qty;
            _lastMove[(productId, from.Value)] = at;
        }

        if (to.HasValue)
        {
            _stock[(productId, to.Value)] = Stock(productId, to.Value) + qty;
            _lastMove[(productId, to.Value)] = at;
        }

        _moves.Add(new MoveRecord(at, type, productId, qty, from, to, order, userId, note, reason));
    }

    // ------------------------------------------------------------------ cuvanje

    private async Task PersistAsync()
    {
        _db.Orders.AddRange(_orders.Select(x => x.Entity));
        await SaveAsync();

        foreach (var chunk in _moves.OrderBy(x => x.At).Chunk(3000))
        {
            _db.StockMovements.AddRange(chunk.Select(m => new StockMovement
            {
                MovementType = m.Type,
                IssueReason = m.Reason,
                ProductId = m.ProductId,
                Quantity = m.Quantity,
                FromLocationId = m.From,
                ToLocationId = m.To,
                OrderId = m.Order?.Entity.Id,
                UserId = m.UserId,
                Note = m.Note,
                CreatedAt = Utc(m.At)
            }));
            await SaveAsync();
        }

        var storeLimits = StoreSpecs
            .Where(s => s.Active)
            .SelectMany(store => ProductSpecs
                .Where(spec => spec.Demand > 0 && Carries(store, spec))
                .Select(spec => (Key: (_products[spec.Sku].Id, Loc($"{store.Code}-MAIN")), Limits: StoreLimits(spec, store))))
            .ToDictionary(x => x.Key, x => x.Limits);

        _db.StockItems.AddRange(_stock.Select(entry =>
        {
            var hasLimits = storeLimits.TryGetValue(entry.Key, out var limits);
            var item = new StockItem
            {
                ProductId = entry.Key.Product,
                StorageLocationId = entry.Key.Location,
                Quantity = entry.Value,
                MinStockOverride = hasLimits ? limits.Min : null,
                MaxStockOverride = hasLimits ? limits.Max : null,
                CreatedAt = Utc(_firstDay.AddHours(6)),
                UpdatedAt = Utc(_lastMove[entry.Key])
            };

            if (entry.Key.Location == Loc("NIS-01-BACK"))
            {
                item.MinStockOverride = 10;
                item.MaxStockOverride = 100;
            }

            if (entry.Key.Product == _products["MLK-003"].Id && entry.Key.Location == Loc("CW-B"))
            {
                item.MinStockOverride = 50;
                item.MaxStockOverride = 300;
            }

            return item;
        }));
        await SaveAsync();

        AddFavorites();
        AddNotifications();
        await SaveAsync();
    }

    private void AddFavorites()
    {
        var favorites = new Dictionary<int, string[]>
        {
            [_admin.Id] = new[] { "MLK-001", "JOG-001", "VOD-001", "HLB-001", "KON-001" },
            [_users["marko"].Id] = new[] { "VOD-001", "GAZ-001", "HEM-001" },
            [_users["milica"].Id] = new[] { "SIR-001", "MES-001", "VOC-002", "KON-003" },
        };

        foreach (var (userId, skus) in favorites)
            foreach (var sku in skus)
                _db.UserFavoriteProducts.Add(new UserFavoriteProduct
                {
                    UserId = userId,
                    ProductId = _products[sku].Id,
                    CreatedAt = Utc(_now.AddDays(-_rng.Next(5, 60)))
                });
    }

    private void AddNotifications()
    {
        var approvers = new[] { _admin, _users["milica"], _users["nenad"] };

        foreach (var order in _orders.Where(o => o.Entity.Status == OrderStatus.PendingApproval))
            foreach (var user in approvers.Where(u => u.Id != order.Entity.CreatedByUserId))
                AddNotification(user.Id, NotificationType.OrderAwaitingApproval, "Nalog ceka odobrenje",
                    Summary(order), $"/nalozi?nalog={order.Entity.Id}", order.PendingAt, read: false);

        foreach (var order in _orders.Where(o => o.Entity.ApprovedAt.HasValue
                                              && Local(o.Entity.ApprovedAt.Value) > _now.AddHours(-36)
                                              && o.Entity.ApprovedByUserId != o.Entity.CreatedByUserId))
        {
            var at = Local(order.Entity.ApprovedAt!.Value);
            AddNotification(order.Entity.CreatedByUserId, NotificationType.OrderApproved,
                "Nalog je odobren i spreman za realizaciju", Summary(order), $"/nalozi?nalog={order.Entity.Id}",
                at, read: at < _now.AddHours(-12));
        }

        foreach (var order in _orders.Where(o => o.Entity.Status == OrderStatus.Cancelled
                                              && Local(o.Entity.UpdatedAt!.Value) > _now.AddDays(-5)))
        {
            var at = Local(order.Entity.UpdatedAt!.Value);
            AddNotification(order.Entity.CreatedByUserId, NotificationType.OrderCancelled, "Nalog je otkazan",
                Summary(order), $"/nalozi?nalog={order.Entity.Id}", at, read: at < _now.AddDays(-1));
        }

        var lowStock = ActiveStores()
            .SelectMany(store => Stocked()
                .Where(spec => Carries(store, spec) && !_retiredSkus.Contains(spec.Sku))
                .Select(spec =>
                {
                    var key = (_products[spec.Sku].Id, Loc($"{store.Code}-MAIN"));
                    var min = StoreLimits(spec, store).Min;
                    return (Store: store, Spec: spec, Qty: Stock(key.Item1, key.Item2), Min: min, Key: key);
                }))
            .Where(x => x.Qty < x.Min && _lastMove.ContainsKey(x.Key))
            .OrderBy(x => x.Qty / x.Min)
            .Take(6)
            .ToList();

        foreach (var item in lowStock)
            foreach (var user in approvers)
                AddNotification(user.Id, NotificationType.LowStock, $"Niska zaliha: {item.Spec.Name}",
                    $"{item.Store.Name}: {item.Qty.ToString("#,##0.###", Sr)} (minimum {item.Min.ToString("#,##0.###", Sr)})",
                    "/zalihe", _lastMove[item.Key], read: user == _admin ? false : _rng.NextDouble() < 0.5);

        foreach (var pending in new[] { _users["petar"], _users["sanja"] })
            AddNotification(_admin.Id, NotificationType.UserRegistered, "Novi zahtev za pristup",
                $"{pending.FirstName} {pending.LastName} ({pending.Email})", "/korisnici", Local(pending.CreatedAt), read: false);
    }

    private void AddNotification(int userId, NotificationType type, string title, string message, string link, DateTime at, bool read)
        => _db.Set<Notification>().Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            CreatedAt = Utc(at),
            ReadAt = read ? Utc(at.AddHours(1) < _now ? at.AddHours(1) : _now) : null
        });

    private static string Summary(SimOrder order)
        => $"{order.Entity.OrderNumber} · {order.Counterparty} · {order.Entity.TotalValue.ToString("#,##0.00", Sr)} RSD";

    private async Task SaveAsync()
    {
        _db.ChangeTracker.DetectChanges();
        await _db.SaveChangesAsync();
    }

    // ------------------------------------------------------------------ pomocne

    private IEnumerable<ProductSpec> Stocked() => ProductSpecs.Where(s => s.Demand > 0);

    private IEnumerable<StoreSpec> ActiveStores() => StoreSpecs.Where(s => s.Active);

    private static bool Carries(StoreSpec store, ProductSpec spec) => !(store.Size < 0.8m && spec.Demand < 4);

    private static (decimal Min, decimal Max) StoreLimits(ProductSpec spec, StoreSpec store)
        => (RoundFor(spec.Unit, Math.Max(2, spec.Demand * store.Size * 3)),
            RoundFor(spec.Unit, Math.Max(6, spec.Demand * store.Size * 8)));

    private string SupplierFor(ProductSpec spec, int day) => spec.Supplier == "konditor" && day < 20 ? "stari" : spec.Supplier;

    private List<User> Warehousemen(int day)
    {
        var pool = new List<User> { _users["marko"], _users["stefan"], _users["ana"] };
        if (day < 45) pool.Add(_users["ivan"]);
        if (day >= Days - 20) pool.Add(_users["jovan"]);
        return pool;
    }

    private User PickManager() => _rng.NextDouble() < 0.1 ? _admin : Pick(new[] { _users["milica"], _users["nenad"] });

    private T Pick<T>(IReadOnlyList<T> items) => items[_rng.Next(items.Count)];

    private int Loc(string code) => _locations[code].Id;

    private decimal Stock(int product, int location) => _stock.GetValueOrDefault((product, location));

    private decimal Reserved(int product, int location) => _reserved.GetValueOrDefault((product, location));

    private static decimal RoundFor(UnitOfMeasure unit, decimal value)
        => unit == UnitOfMeasure.Kilogram
            ? Math.Round(value, 1, MidpointRounding.AwayFromZero)
            : Math.Round(value, 0, MidpointRounding.AwayFromZero);

    private static DateTime Utc(DateTime local) => DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

    private static DateTime Local(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
}
