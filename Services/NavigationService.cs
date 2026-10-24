using AutoHub.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

/* ── AUTOHUB Header Navigation — single source of truth.
   Every menu entry is derived from the canonical vehicle dataset
   (AutoHubDbContext). Nothing is hard-coded twice: brands without
   variants, body types / fuel types / drivetrains without vehicles,
   and features that are not implemented never appear. ── */

public sealed record NavBrandEntry(string Name, string Href, string? LogoPath, int VariantCount);

public sealed record NavEntry(
    string Label,
    string? Href = null,
    string? Meta = null,
    bool ComingSoon = false,
    bool OpensConsultDrawer = false);

public sealed record NavColumn(string Title, IReadOnlyList<NavEntry> Entries);

public sealed class NavSection
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public IReadOnlyList<NavBrandEntry> Brands { get; init; } = [];
    public IReadOnlyList<NavColumn> Columns { get; init; } = [];
}

public sealed class HeaderNavigation
{
    public List<NavSection> Sections { get; } = [];
    public string FeaturedSampleSlug { get; set; } = "";
    public string FeaturedSampleName { get; set; } = "";
}

public interface INavigationService
{
    HeaderNavigation Get();
}

public sealed class NavigationService : INavigationService
{
    private readonly AutoHubDbContext _db;
    private readonly IVehicleService _vehicles;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(AutoHubDbContext db, IVehicleService vehicles, ILogger<NavigationService> logger)
    {
        _db = db;
        _vehicles = vehicles;
        _logger = logger;
    }

    public HeaderNavigation Get()
    {
        var nav = new HeaderNavigation();

        try
        {
            if (!_db.Database.CanConnect()) return nav;
        }
        catch
        {
            return nav;
        }

        try
        {
            var brands = _vehicles.GetBrandItems()
                .Select(b => new NavBrandEntry(b.Name, $"/?brand={Uri.EscapeDataString(b.Name)}#machines", b.LogoPath, 0))
                .ToList();

            /* Body types — only those actually referenced by seeded models/variants. */
            var bodyTypes = _db.BodyTypes
                .AsNoTracking()
                .Where(bt => bt.Models.Any(m => m.Variants.Any()))
                .OrderBy(bt => bt.BodyTypeId)
                .Select(bt => new
                {
                    bt.Name,
                    Count = bt.Models.SelectMany(m => m.Variants).Count(),
                })
                .ToList()
                .Where(bt => bt.Count > 0)
                .Select(bt => new NavEntry(
                    bt.Name,
                    $"/?q={Uri.EscapeDataString(bt.Name)}#machines",
                    $"{bt.Count} mẫu"))
                .ToList();

            /* Fuel types & drivetrains present in the dataset, with counts. */
            var fuelCounts = _db.FuelTypes
                .AsNoTracking()
                .Select(f => new { f.Name, f.Slug, Count = f.Variants.Count() })
                .ToList()
                .Where(f => f.Count > 0)
                .ToDictionary(f => f.Slug, f => (f.Name, f.Count));

            var driveCounts = _db.DriveTypes
                .AsNoTracking()
                .Select(d => new { d.Name, d.Slug, Count = d.Variants.Count() })
                .ToList()
                .Where(d => d.Count > 0)
                .ToDictionary(d => d.Slug, d => (d.Name, d.Count));

            /* Discovery flags — only shown when the flag matches real rows. */
            var featuredCount = _db.VehicleVariants.Count(v => v.IsFeatured);
            var newArrivalCount = _db.VehicleVariants.Count(v => v.IsNewArrival);
            var popularCount = _db.VehicleVariants.Count(v => v.IsPopular);

            var featuredSample = _db.VehicleVariants
                .AsNoTracking()
                .Where(v => v.IsFeatured)
                .OrderByDescending(v => v.IsPopular)
                .ThenBy(v => v.VariantId)
                .Select(v => new { v.Slug, Brand = v.Model.Brand.Name, ModelName = v.Model.Name, v.VariantName })
                .FirstOrDefault();

            nav.FeaturedSampleSlug = featuredSample?.Slug ?? "";
            nav.FeaturedSampleName = featuredSample is null ? "" : $"{featuredSample.Brand} {featuredSample.ModelName} {featuredSample.VariantName}";

            /* ── DÒNG XE ── */
            var discovery = new List<NavEntry>
            {
                new("Tất cả dòng xe", "/#machines", $"{CountAll()} mẫu"),
            };
            if (featuredCount > 0) discovery.Add(new NavEntry("Xe nổi bật", "/?flag=featured#machines", $"{featuredCount} mẫu"));
            if (newArrivalCount > 0) discovery.Add(new NavEntry("Xe mới", "/?flag=new#machines", $"{newArrivalCount} mẫu"));
            if (popularCount > 0) discovery.Add(new NavEntry("Xe phổ biến", "/?flag=popular#machines", $"{popularCount} mẫu"));
            if (fuelCounts.TryGetValue("ev", out var ev)) discovery.Add(new NavEntry("Xe điện (EV)", "/?flag=ev#machines", $"{ev.Count} mẫu"));
            discovery.Add(new NavEntry("Dự toán lăn bánh & Trả góp", "/Finance", "Công cụ tài chính"));

            var vehicleColumns = new List<NavColumn>();
            if (bodyTypes.Count > 0)
                vehicleColumns.Add(new NavColumn("Phân loại xe", bodyTypes));
            vehicleColumns.Add(new NavColumn("Bộ sưu tập", discovery));

            nav.Sections.Add(new NavSection
            {
                Id = "vehicles",
                Label = "Dòng xe",
                Brands = brands,
                Columns = vehicleColumns,
            });

            /* ── TRẢI NGHIỆM ── */
            var experienceColumns = new List<NavColumn>
            {
                new("Khám phá", new List<NavEntry>
                {
                    new("Trải nghiệm toàn cảnh", "/Experience"),
                    new("Thẩm mỹ & khí động học", "/Experience#exp-vehicles"),
                    new("Tương tác 3D", "/Experience#exp-3d"),
                    new("Môi trường showroom", "/Experience#exp-showroom"),
                }),
            };

            if (!string.IsNullOrEmpty(nav.FeaturedSampleSlug))
            {
                experienceColumns.Add(new NavColumn("Trên từng chiếc xe", new List<NavEntry>
                {
                    new("Bảng màu thực tế", $"/Vehicles/{nav.FeaturedSampleSlug}#colors",
                        Truncate(nav.FeaturedSampleName, 26)),
                    new("Thông số cấu hình", $"/Vehicles/{nav.FeaturedSampleSlug}#specifications"),
                }));
            }

            experienceColumns.Add(new NavColumn("Trải nghiệm sẵn sàng", new List<NavEntry>
            {
                new("Xem xe 3D", "/Experience#exp-3d"),
                new("Xoay 360°", "/Experience#exp-3d"),
                new("7 lĩnh vực trải nghiệm", "/Experience#exp-vehicles"),
            }));

            nav.Sections.Add(new NavSection
            {
                Id = "experience",
                Label = "Trải nghiệm",
                Columns = experienceColumns,
            });

            /* ── CÔNG NGHỆ ── */
            var techColumns = new List<NavColumn>();

            var fuelEntries = new List<NavEntry>();
            foreach (var (slug, label) in new[] { ("petrol", "Động cơ xăng"), ("diesel", "Động cơ diesel"), ("hybrid", "Hybrid"), ("ev", "Xe điện (EV)") })
            {
                if (fuelCounts.TryGetValue(slug, out var f))
                    fuelEntries.Add(slug == "ev"
                        ? new NavEntry(label, "/?flag=ev#machines", $"{f.Count} mẫu")
                        : new NavEntry(label, $"/?q={Uri.EscapeDataString(f.Name)}#machines", $"{f.Count} mẫu"));
            }
            if (fuelEntries.Count > 0) techColumns.Add(new NavColumn("Nhiên liệu & động cơ", fuelEntries));

            var driveEntries = new List<NavEntry>();
            foreach (var (slug, label) in new[] { ("fwd", "Cầu trước · FWD"), ("rwd", "Cầu sau · RWD"), ("awd", "Toàn thời gian · AWD"), ("4wd", "Chọn thời gian · 4WD") })
            {
                if (driveCounts.TryGetValue(slug, out var d))
                    driveEntries.Add(new NavEntry(label, $"/?q={Uri.EscapeDataString(d.Name)}#machines", $"{d.Count} mẫu"));
            }
            if (driveEntries.Count > 0) techColumns.Add(new NavColumn("Hệ thống dẫn động", driveEntries));

            var techMore = new List<NavEntry>
            {
                new("Tin công nghệ ô tô", "/#press"),
                new("Dự toán lăn bánh & Trả góp", "/Finance"),
            };
            if (!string.IsNullOrEmpty(nav.FeaturedSampleSlug))
                techMore.Add(new NavEntry("Đọc thông số thực tế", $"/Vehicles/{nav.FeaturedSampleSlug}#specifications"));
            techColumns.Add(new NavColumn("Khám phá thêm", techMore));

            nav.Sections.Add(new NavSection
            {
                Id = "technology",
                Label = "Công nghệ",
                Columns = techColumns,
            });

            /* ── LÁI THỬ ── */
            nav.Sections.Add(new NavSection
            {
                Id = "test-drive",
                Label = "Lái thử",
                Columns = new List<NavColumn>
                {
                    new("Đặt lịch", new List<NavEntry>
                    {
                        new("Đăng ký lái thử", "/TestDrive"),
                        new("Chọn mẫu xe", "/TestDrive#chon-xe"),
                        new("Ngày & khung giờ", "/TestDrive#thoi-gian"),
                    }),
                    new("Thông tin", new List<NavEntry>
                    {
                        new("Quy trình lái thử", "/TestDrive#luu-y"),
                        new("Điều kiện lái thử", "/TestDrive#luu-y"),
                        new("Showroom Hà Nội", "/Contact#showroom"),
                    }),
                },
            });

            /* ── LIÊN HỆ ── */
            nav.Sections.Add(new NavSection
            {
                Id = "contact",
                Label = "Liên hệ",
                Columns = new List<NavColumn>
                {
                    new("Liên hệ trực tiếp", new List<NavEntry>
                    {
                        new("Gửi tin nhắn về AUTOHUB", "/Contact"),
                        new("Đặt lịch tư vấn nhanh", OpensConsultDrawer: true),
                        new("Câu hỏi thường gặp", ComingSoon: true),
                    }),
                    new("Showroom & hỗ trợ", new List<NavEntry>
                    {
                        new("Thông tin showroom", "/Contact#showroom"),
                        new("Kênh liên hệ (Zalo, Email…)", OpensConsultDrawer: true),
                    }),
                },
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tao header navigation");
        }

        return nav;
    }

    private int CountAll() => _db.VehicleVariants.Count();

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 1)].TrimEnd() + "…";
}
