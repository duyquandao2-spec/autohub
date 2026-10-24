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
public class EditModel : PageModel
{
    private readonly AutoHubDbContext _db;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AutoHubDbContext db, ILogger<EditModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public VehicleEditInput Input { get; set; } = new();

    public int VehicleId { get; set; }
    public string VehicleFullName { get; set; } = string.Empty;

    public SelectList Brands { get; set; } = null!;
    public SelectList FuelTypes { get; set; } = null!;
    public SelectList TransmissionTypes { get; set; } = null!;
    public SelectList DriveTypes { get; set; } = null!;
    public SelectList BodyTypes { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var v = await _db.VehicleVariants
            .Include(x => x.Model).ThenInclude(m => m.Brand)
            .Include(x => x.Specification)
            .Include(x => x.VariantMedia).ThenInclude(vm => vm.Media)
            .FirstOrDefaultAsync(x => x.VariantId == id);

        if (v == null) return NotFound();

        VehicleId = v.VariantId;
        VehicleFullName = $"{v.Model.Brand.Name} {v.Model.Name} {v.VariantName}";

        var heroMedia = v.VariantMedia.FirstOrDefault(vm => vm.IsPrimary && vm.MediaRole == "HERO")
                        ?? v.VariantMedia.FirstOrDefault();

        Input = new VehicleEditInput
        {
            Id = v.VariantId,
            BrandId = v.Model.BrandId,
            ModelName = v.Model.Name,
            VariantName = v.VariantName,
            Slug = v.Slug,
            ModelYear = v.ModelYear,
            BasePrice = v.BasePrice,
            FuelTypeId = v.FuelTypeId,
            TransmissionTypeId = v.TransmissionTypeId,
            DriveTypeId = v.DriveTypeId,
            BodyTypeId = v.Model.BodyTypeId,
            SeatCount = v.SeatCount,
            ImagePath = heroMedia?.Media.FilePath,
            Description = v.Description,
            Status = v.Status,
            IsFeatured = v.IsFeatured,
            IsNewArrival = v.IsNewArrival,
            IsPopular = v.IsPopular,
            IsAvailable = v.IsAvailable,

            // Specifications
            EngineType = v.Specification?.EngineType,
            EngineDisplacement = v.Specification?.EngineDisplacement,
            Horsepower = v.Specification?.Horsepower,
            Torque = v.Specification?.Torque,
            Acceleration0To100 = v.Specification?.Acceleration0To100,
            TopSpeed = v.Specification?.TopSpeed,
            FuelConsumptionCombined = v.Specification?.FuelConsumptionCombined,
            LengthMm = v.Specification?.LengthMm,
            WidthMm = v.Specification?.WidthMm,
            HeightMm = v.Specification?.HeightMm,
            WheelbaseMm = v.Specification?.WheelbaseMm,
            GroundClearanceMm = v.Specification?.GroundClearanceMm,
            CurbWeightKg = v.Specification?.CurbWeightKg
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var v = await _db.VehicleVariants
            .Include(x => x.Model).ThenInclude(m => m.Brand)
            .Include(x => x.Specification)
            .Include(x => x.VariantMedia).ThenInclude(vm => vm.Media)
            .FirstOrDefaultAsync(x => x.VariantId == id);

        if (v == null) return NotFound();

        VehicleId = v.VariantId;
        VehicleFullName = $"{v.Model.Brand.Name} {v.Model.Name} {v.VariantName}";

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            // 1. Update Model if changed
            var brand = await _db.Brands.FindAsync(Input.BrandId);
            if (brand == null)
            {
                ModelState.AddModelError("Input.BrandId", "Thương hiệu không hợp lệ.");
                await LoadDropdownsAsync();
                return Page();
            }

            var modelName = Input.ModelName.Trim();
            if (v.Model.Name != modelName || v.Model.BrandId != Input.BrandId)
            {
                var existingModel = await _db.VehicleModels
                    .FirstOrDefaultAsync(m => m.BrandId == Input.BrandId && m.Name.ToLower() == modelName.ToLower());

                if (existingModel == null)
                {
                    existingModel = new VehicleModelEntity
                    {
                        BrandId = Input.BrandId,
                        Name = modelName,
                        Slug = GenerateSlug(modelName),
                        BodyTypeId = Input.BodyTypeId,
                        ModelYearFrom = Input.ModelYear ?? DateTime.UtcNow.Year,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _db.VehicleModels.Add(existingModel);
                    await _db.SaveChangesAsync();
                }
                v.ModelId = existingModel.ModelId;
            }
            else if (v.Model.BodyTypeId != Input.BodyTypeId)
            {
                v.Model.BodyTypeId = Input.BodyTypeId;
                v.Model.UpdatedAt = DateTime.UtcNow;
            }

            // 2. Update Variant
            v.VariantName = Input.VariantName.Trim();
            if (!string.IsNullOrWhiteSpace(Input.Slug) && Input.Slug != v.Slug)
            {
                var cleanSlug = GenerateSlug(Input.Slug.Trim());
                var slugExists = await _db.VehicleVariants.AnyAsync(x => x.Slug == cleanSlug && x.VariantId != v.VariantId);
                v.Slug = slugExists ? $"{cleanSlug}-{DateTime.UtcNow.Ticks % 1000}" : cleanSlug;
            }

            v.BasePrice = Input.BasePrice;
            v.ModelYear = Input.ModelYear;
            v.FuelTypeId = Input.FuelTypeId;
            v.TransmissionTypeId = Input.TransmissionTypeId;
            v.DriveTypeId = Input.DriveTypeId;
            v.SeatCount = Input.SeatCount;
            v.Description = Input.Description?.Trim();
            v.Status = Input.Status ?? "ACTIVE";
            v.IsFeatured = Input.IsFeatured;
            v.IsNewArrival = Input.IsNewArrival;
            v.IsPopular = Input.IsPopular;
            v.IsAvailable = Input.IsAvailable;
            v.UpdatedAt = DateTime.UtcNow;

            // 3. Update Specification
            if (v.Specification == null)
            {
                v.Specification = new VehicleSpecificationEntity
                {
                    VariantId = v.VariantId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.VehicleSpecifications.Add(v.Specification);
            }

            v.Specification.EngineType = Input.EngineType?.Trim();
            v.Specification.EngineDisplacement = Input.EngineDisplacement;
            v.Specification.Horsepower = Input.Horsepower;
            v.Specification.Torque = Input.Torque;
            v.Specification.Acceleration0To100 = Input.Acceleration0To100;
            v.Specification.TopSpeed = Input.TopSpeed;
            v.Specification.FuelConsumptionCombined = Input.FuelConsumptionCombined;
            v.Specification.LengthMm = Input.LengthMm;
            v.Specification.WidthMm = Input.WidthMm;
            v.Specification.HeightMm = Input.HeightMm;
            v.Specification.WheelbaseMm = Input.WheelbaseMm;
            v.Specification.GroundClearanceMm = Input.GroundClearanceMm;
            v.Specification.CurbWeightKg = Input.CurbWeightKg;
            v.Specification.UpdatedAt = DateTime.UtcNow;

            // 4. Update Media Image
            if (!string.IsNullOrWhiteSpace(Input.ImagePath))
            {
                var cleanPath = Input.ImagePath.Trim();
                var hero = v.VariantMedia.FirstOrDefault(vm => vm.IsPrimary && vm.MediaRole == "HERO")
                           ?? v.VariantMedia.FirstOrDefault();

                if (hero != null && hero.Media != null)
                {
                    hero.Media.FilePath = cleanPath;
                    hero.Media.FileName = Path.GetFileName(cleanPath);
                }
                else
                {
                    var media = new MediaAssetEntity
                    {
                        MediaType = "IMAGE",
                        FileName = Path.GetFileName(cleanPath),
                        FilePath = cleanPath,
                        Title = v.VariantName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.MediaAssets.Add(media);
                    await _db.SaveChangesAsync();

                    _db.VariantMedia.Add(new VariantMediaEntity
                    {
                        VariantId = v.VariantId,
                        MediaId = media.MediaId,
                        MediaRole = "HERO",
                        IsPrimary = true,
                        SortOrder = 1
                    });
                }
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật thành công xe: {brand.Name} {v.Model.Name} {v.VariantName}";
            return RedirectToPage("/Admin/Vehicles/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật xe ID {Id}", id);
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
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

    public sealed class VehicleEditInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dòng xe.")]
        public string ModelName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên phiên bản.")]
        public string VariantName { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public int? ModelYear { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá niêm yết.")]
        public decimal BasePrice { get; set; }

        public int FuelTypeId { get; set; }
        public int TransmissionTypeId { get; set; }
        public int DriveTypeId { get; set; }
        public int? BodyTypeId { get; set; }
        public int SeatCount { get; set; } = 5;

        public string? ImagePath { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "ACTIVE";

        public bool IsFeatured { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsPopular { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Specifications
        public string? EngineType { get; set; }
        public int? EngineDisplacement { get; set; }
        public int? Horsepower { get; set; }
        public int? Torque { get; set; }
        public decimal? Acceleration0To100 { get; set; }
        public int? TopSpeed { get; set; }
        public decimal? FuelConsumptionCombined { get; set; }
        public int? LengthMm { get; set; }
        public int? WidthMm { get; set; }
        public int? HeightMm { get; set; }
        public int? WheelbaseMm { get; set; }
        public int? GroundClearanceMm { get; set; }
        public int? CurbWeightKg { get; set; }
    }
}
