using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

public interface IVehicleService
{
    IReadOnlyList<Vehicle> GetVehicles(string? brand = null);
    PaginationResult<Vehicle> GetVehiclesPaged(int page, int pageSize, string? brand = null, string? search = null);
    IReadOnlyList<string> GetBrands();
    Vehicle? GetVehicleBySlug(string slug);
    IReadOnlyList<Vehicle> Search(string query);
}

public sealed class VehicleService : IVehicleService
{
    private readonly AutoHubDbContext _db;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(AutoHubDbContext db, ILogger<VehicleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private bool IsDbAvailable()
    {
        try { return _db.Database.CanConnect(); }
        catch { return false; }
    }

    private IQueryable<VehicleVariantEntity> BaseQuery()
    {
        return _db.VehicleVariants
            .Include(v => v.Model).ThenInclude(m => m.Brand)
            .Include(v => v.Model).ThenInclude(m => m.BodyType)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .Include(v => v.DriveType)
            .Include(v => v.Specification)
            .Include(v => v.VariantColors).ThenInclude(vc => vc.Color)
            .Include(v => v.VariantMedia).ThenInclude(vm => vm.Media)
            .AsNoTracking();
    }

    private static Vehicle ProjectToVehicle(VehicleVariantEntity v)
    {
        var heroMedia = v.VariantMedia.FirstOrDefault(vm => vm.IsPrimary && vm.MediaRole == "HERO")
                        ?? v.VariantMedia.FirstOrDefault(vm => vm.MediaRole == "HERO")
                        ?? v.VariantMedia.FirstOrDefault();

        return new Vehicle
        {
            Id = v.VariantId,
            BrandId = v.Model.BrandId,
            Code = $"{v.Model.Brand.Name} {v.VariantName}",
            Slug = v.Slug,
            Name = $"{v.Model.Brand.Name} {v.Model.Name} {v.VariantName}",
            BrandName = v.Model.Brand.Name,
            Model = v.Model.Name,
            Variant = v.VariantName,
            Category = v.Model.BodyType?.Name ?? "",
            FuelType = v.FuelType.Name,
            Transmission = v.TransmissionType.Name,
            Drivetrain = v.DriveType.Name,
            Seats = v.SeatCount,
            Price = $"Từ {v.BasePrice:N0}đ",
            BasePrice = v.BasePrice,
            ImagePath = heroMedia?.Media.FilePath ?? "",
            Description = v.Description ?? "",
            Acceleration = v.Specification?.Acceleration0To100,
            TopSpeed = v.Specification?.TopSpeed,
            Power = v.Specification?.Horsepower,
            FuelEconomy = v.Specification?.FuelConsumptionCombined,
            IsFeatured = v.IsFeatured,
            IsNewArrival = v.IsNewArrival,
            IsPopular = v.IsPopular,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            Colors = v.VariantColors
                .OrderBy(vc => vc.SortOrder)
                .Select(vc => new VehicleColor
                {
                    Id = vc.ColorId,
                    VehicleId = v.VariantId,
                    Name = vc.Color.Name,
                    Code = vc.Color.HexCode ?? vc.Color.ColorCode,
                    ImagePath = vc.Color.HexCode ?? "#888888",
                    ExtraPrice = vc.ExtraPrice,
                }).ToList(),
            Specification = v.Specification != null ? new VehicleSpecification
            {
                Id = v.Specification.SpecificationId,
                VehicleId = v.VariantId,
                Engine = v.Specification.EngineType,
                Displacement = v.Specification.EngineDisplacement,
                CylinderCount = v.Specification.CylinderCount,
                Horsepower = v.Specification.Horsepower,
                Torque = v.Specification.Torque,
                Acceleration = v.Specification.Acceleration0To100,
                TopSpeed = v.Specification.TopSpeed,
                FuelConsumptionCity = v.Specification.FuelConsumptionCity,
                FuelConsumptionHighway = v.Specification.FuelConsumptionHighway,
                FuelConsumptionCombined = v.Specification.FuelConsumptionCombined,
                FuelTank = v.Specification.FuelTankCapacity,
                Length = v.Specification.LengthMm,
                Width = v.Specification.WidthMm,
                Height = v.Specification.HeightMm,
                Wheelbase = v.Specification.WheelbaseMm,
                GroundClearance = v.Specification.GroundClearanceMm,
            } : null,
        };
    }

    private static IQueryable<VehicleVariantEntity> ApplyFilters(
        IQueryable<VehicleVariantEntity> query, string? brand, string? search)
    {
        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(v => v.Model.Brand.Name == brand);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var terms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(v => terms.All(term =>
                v.Model.Brand.Name.Contains(term) ||
                v.Model.Name.Contains(term) ||
                v.VariantName.Contains(term) ||
                (v.Model.BodyType != null && v.Model.BodyType.Name.Contains(term)) ||
                v.FuelType.Name.Contains(term) ||
                v.DriveType.Name.Contains(term) ||
                (v.Description != null && v.Description.Contains(term))));
        }

        return query;
    }

    public IReadOnlyList<Vehicle> GetVehicles(string? brand = null)
    {
        if (!IsDbAvailable()) return [];

        try
        {
            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(v => v.Model.Brand.Name == brand);
            }

            return query
                .OrderByDescending(v => v.IsFeatured)
                .ThenByDescending(v => v.VariantId)
                .Select(v => ProjectToVehicle(v))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the truy van vehicles");
            return [];
        }
    }

    public PaginationResult<Vehicle> GetVehiclesPaged(int page, int pageSize, string? brand = null, string? search = null)
    {
        if (!IsDbAvailable()) return new PaginationResult<Vehicle> { CurrentPage = page, PageSize = pageSize };

        try
        {
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = BaseQuery();
            query = ApplyFilters(query, brand, search);

            var orderedQuery = query
                .OrderByDescending(v => v.IsFeatured)
                .ThenByDescending(v => v.VariantId);

            var totalItems = orderedQuery.Count();

            if (totalItems == 0)
            {
                return new PaginationResult<Vehicle>
                {
                    Items = [],
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Clamp(page, 1, totalPages);

            var items = orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => ProjectToVehicle(v))
                .ToList();

            return new PaginationResult<Vehicle>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the truy van vehicles paged");
            return new PaginationResult<Vehicle> { CurrentPage = page, PageSize = pageSize };
        }
    }

    public IReadOnlyList<string> GetBrands()
    {
        if (!IsDbAvailable()) return [];

        try
        {
            return _db.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => b.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the truy van brands");
            return [];
        }
    }

    public Vehicle? GetVehicleBySlug(string slug)
    {
        if (!IsDbAvailable()) return null;

        try
        {
            var variant = BaseQuery().FirstOrDefault(v => v.Slug == slug);
            return variant is null ? null : ProjectToVehicle(variant);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tim vehicle slug={Slug}", slug);
            return null;
        }
    }

    public IReadOnlyList<Vehicle> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return GetVehicles();
        if (!IsDbAvailable()) return [];

        try
        {
            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return BaseQuery()
                .Where(v => terms.All(term =>
                    v.Model.Brand.Name.Contains(term) ||
                    v.Model.Name.Contains(term) ||
                    v.VariantName.Contains(term) ||
                    (v.Model.BodyType != null && v.Model.BodyType.Name.Contains(term)) ||
                    v.FuelType.Name.Contains(term) ||
                    v.DriveType.Name.Contains(term) ||
                    (v.Description != null && v.Description.Contains(term))))
                .Select(v => ProjectToVehicle(v))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the search vehicles");
            return [];
        }
    }
}
