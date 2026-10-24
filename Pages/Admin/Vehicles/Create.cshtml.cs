using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Admin.Vehicles;

[Authorize(Policy = Policies.VehicleWrite)]
public class CreateModel : PageModel
{
    private readonly AutoHubDbContext _db;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(AutoHubDbContext db, ILogger<CreateModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public VehicleInput Input { get; set; } = new();

    public SelectList Brands { get; set; } = null!;
    public SelectList FuelTypes { get; set; } = null!;
    public SelectList TransmissionTypes { get; set; } = null!;
    public SelectList DriveTypes { get; set; } = null!;
    public SelectList BodyTypes { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            // 1. Resolve or create VehicleModel
            var brand = await _db.Brands.FindAsync(Input.BrandId);
            if (brand == null)
            {
                ModelState.AddModelError("Input.BrandId", "Thương hiệu không hợp lệ.");
                await LoadDropdownsAsync();
                return Page();
            }

            var modelName = Input.ModelName.Trim();
            var modelSlug = GenerateSlug(modelName);

            var model = await _db.VehicleModels
                .FirstOrDefaultAsync(m => m.BrandId == Input.BrandId && m.Name.ToLower() == modelName.ToLower());

            if (model == null)
            {
                model = new VehicleModelEntity
                {
                    BrandId = Input.BrandId,
                    Name = modelName,
                    Slug = modelSlug,
                    BodyTypeId = Input.BodyTypeId,
                    ModelYearFrom = Input.ModelYear ?? DateTime.UtcNow.Year,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.VehicleModels.Add(model);
                await _db.SaveChangesAsync();
            }

            // 2. Resolve variant slug
            var variantName = Input.VariantName.Trim();
            var variantSlug = string.IsNullOrWhiteSpace(Input.Slug)
                ? GenerateSlug($"{brand.Name}-{model.Name}-{variantName}")
                : GenerateSlug(Input.Slug.Trim());

            // Check if slug exists
            var existingSlug = await _db.VehicleVariants.AnyAsync(v => v.Slug == variantSlug);
            if (existingSlug)
            {
                variantSlug += $"-{DateTime.UtcNow.Ticks % 10000}";
            }

            // 3. Create Variant Entity
            var variant = new VehicleVariantEntity
            {
                ModelId = model.ModelId,
                VariantName = variantName,
                Slug = variantSlug,
                ModelYear = Input.ModelYear ?? DateTime.UtcNow.Year,
                MarketCode = "VN",
                BasePrice = Input.BasePrice,
                CurrencyCode = "VND",
                FuelTypeId = Input.FuelTypeId,
                TransmissionTypeId = Input.TransmissionTypeId,
                DriveTypeId = Input.DriveTypeId,
                SeatCount = Input.SeatCount,
                Description = Input.Description?.Trim(),
                IsFeatured = Input.IsFeatured,
                IsNewArrival = Input.IsNewArrival,
                IsPopular = Input.IsPopular,
                IsAvailable = Input.IsAvailable,
                Status = Input.Status ?? "ACTIVE",
                Model3DPath = string.IsNullOrWhiteSpace(Input.Model3DPath) ? null : Input.Model3DPath.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.VehicleVariants.Add(variant);
            await _db.SaveChangesAsync();

            // 4. Create Specification Entity
            var spec = new VehicleSpecificationEntity
            {
                VariantId = variant.VariantId,
                EngineType = Input.EngineType?.Trim(),
                EngineDisplacement = Input.EngineDisplacement,
                Horsepower = Input.Horsepower,
                Torque = Input.Torque,
                Acceleration0To100 = Input.Acceleration0To100,
                TopSpeed = Input.TopSpeed,
                FuelConsumptionCombined = Input.FuelConsumptionCombined,
                LengthMm = Input.LengthMm,
                WidthMm = Input.WidthMm,
                HeightMm = Input.HeightMm,
                WheelbaseMm = Input.WheelbaseMm,
                GroundClearanceMm = Input.GroundClearanceMm,
                CurbWeightKg = Input.CurbWeightKg,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.VehicleSpecifications.Add(spec);

            // 5. Hero Image / MediaAsset
            if (!string.IsNullOrWhiteSpace(Input.ImagePath))
            {
                var cleanPath = Input.ImagePath.Trim();
                var mediaAsset = await _db.MediaAssets.FirstOrDefaultAsync(m => m.FilePath == cleanPath);
                if (mediaAsset == null)
                {
                    mediaAsset = new MediaAssetEntity
                    {
                        MediaType = "IMAGE",
                        FileName = Path.GetFileName(cleanPath),
                        FilePath = cleanPath,
                        Title = $"{brand.Name} {model.Name} {variantName}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.MediaAssets.Add(mediaAsset);
                    await _db.SaveChangesAsync();
                }

                var variantMedia = new VariantMediaEntity
                {
                    VariantId = variant.VariantId,
                    MediaId = mediaAsset.MediaId,
                    MediaRole = "HERO",
                    IsPrimary = true,
                    SortOrder = 1
                };
                _db.VariantMedia.Add(variantMedia);
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm thành công xe: {brand.Name} {model.Name} {variantName}";
            return RedirectToPage("/Admin/Vehicles/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm xe mới vào hệ thống");
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình lưu dữ liệu: " + ex.Message);
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var brands = await _db.Brands.OrderBy(b => b.Name).ToListAsync();
        var fuels = await _db.FuelTypes.OrderBy(f => f.Name).ToListAsync();
        var transmissions = await _db.TransmissionTypes.OrderBy(t => t.Name).ToListAsync();
        var drives = await _db.DriveTypes.OrderBy(d => d.Name).ToListAsync();
        var bodies = await _db.BodyTypes.OrderBy(b => b.Name).ToListAsync();

        Brands = new SelectList(brands, nameof(BrandEntity.BrandId), nameof(BrandEntity.Name));
        FuelTypes = new SelectList(fuels, nameof(FuelTypeEntity.FuelTypeId), nameof(FuelTypeEntity.Name));
        TransmissionTypes = new SelectList(transmissions, nameof(TransmissionTypeEntity.TransmissionTypeId), nameof(TransmissionTypeEntity.Name));
        DriveTypes = new SelectList(drives, nameof(DriveTypeEntity.DriveTypeId), nameof(DriveTypeEntity.Name));
        BodyTypes = new SelectList(bodies, nameof(BodyTypeEntity.BodyTypeId), nameof(BodyTypeEntity.Name));
    }

    private static string GenerateSlug(string text)
    {
        var str = text.ToLowerInvariant();
        str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
        str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
        str = Regex.Replace(str, @"[íìỉĩị]", "i");
        str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
        str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
        str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
        str = Regex.Replace(str, @"đ", "d");
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        return str;
    }

    public sealed class VehicleInput
    {
        [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
        [Display(Name = "Thương hiệu")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dòng xe / mẫu xe (VD: 3 Series, C-Class, Macan).")]
        [Display(Name = "Dòng xe (Model)")]
        [StringLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên phiên bản xe (VD: 330i M Sport, C 300 AMG).")]
        [Display(Name = "Phiên bản (Variant)")]
        [StringLength(100)]
        public string VariantName { get; set; } = string.Empty;

        [Display(Name = "Đường dẫn URL (Slug)")]
        [StringLength(150)]
        public string? Slug { get; set; }

        [Display(Name = "Năm sản xuất / Đời xe")]
        [Range(2000, 2030, ErrorMessage = "Năm sản xuất từ 2000 đến 2030.")]
        public int? ModelYear { get; set; } = 2024;

        [Required(ErrorMessage = "Vui lòng nhập giá niêm yết.")]
        [Display(Name = "Giá niêm yết (VNĐ)")]
        [Range(0, 100_000_000_000, ErrorMessage = "Giá bán không hợp lệ.")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại nhiên liệu.")]
        [Display(Name = "Nhiên liệu / Động cơ")]
        public int FuelTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hộp số.")]
        [Display(Name = "Hộp số")]
        public int TransmissionTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hệ dẫn động.")]
        [Display(Name = "Hệ dẫn động")]
        public int DriveTypeId { get; set; }

        [Display(Name = "Kiểu dáng xe (Body Type)")]
        public int? BodyTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số chỗ ngồi.")]
        [Display(Name = "Số chỗ ngồi")]
        [Range(1, 16, ErrorMessage = "Số chỗ ngồi từ 1 đến 16.")]
        public int SeatCount { get; set; } = 5;

        [Display(Name = "Ảnh đại diện xe (Hero Image)")]
        public string? ImagePath { get; set; }

        [Display(Name = "Mô hình 3D (.glb, tùy chọn)")]
        public string? Model3DPath { get; set; }

        [Display(Name = "Mô tả giới thiệu")]
        public string? Description { get; set; }

        [Display(Name = "Trạng thái bán")]
        public string Status { get; set; } = "ACTIVE";

        [Display(Name = "Xe nổi bật (Featured)")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Xe mới về (New Arrival)")]
        public bool IsNewArrival { get; set; }

        [Display(Name = "Xe phổ biến (Popular)")]
        public bool IsPopular { get; set; }

        [Display(Name = "Sẵn hàng")]
        public bool IsAvailable { get; set; } = true;

        // Specifications
        [Display(Name = "Động cơ")]
        public string? EngineType { get; set; }

        [Display(Name = "Dung tích xi lanh (cc)")]
        public int? EngineDisplacement { get; set; }

        [Display(Name = "Công suất cực đại (HP)")]
        public int? Horsepower { get; set; }

        [Display(Name = "Mô-men xoắn cực đại (Nm)")]
        public int? Torque { get; set; }

        [Display(Name = "Tăng tốc 0 - 100 km/h (giây)")]
        public decimal? Acceleration0To100 { get; set; }

        [Display(Name = "Tốc độ tối đa (km/h)")]
        public int? TopSpeed { get; set; }

        [Display(Name = "Tiêu thụ nhiên liệu hỗn hợp (L/100km)")]
        public decimal? FuelConsumptionCombined { get; set; }

        [Display(Name = "Chiều dài (mm)")]
        public int? LengthMm { get; set; }

        [Display(Name = "Chiều rộng (mm)")]
        public int? WidthMm { get; set; }

        [Display(Name = "Chiều cao (mm)")]
        public int? HeightMm { get; set; }

        [Display(Name = "Chiều dài cơ sở (mm)")]
        public int? WheelbaseMm { get; set; }

        [Display(Name = "Khoảng sáng gầm (mm)")]
        public int? GroundClearanceMm { get; set; }

        [Display(Name = "Trọng lượng không tải (kg)")]
        public int? CurbWeightKg { get; set; }
    }
}
