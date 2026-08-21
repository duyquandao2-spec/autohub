namespace AutoHub.Models.Entities;

public class VehicleSpecificationEntity
{
    public int SpecificationId { get; set; }
    public int VariantId { get; set; }
    public string? EngineType { get; set; }
    public int? EngineDisplacement { get; set; }
    public int? CylinderCount { get; set; }
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public decimal? Acceleration0To100 { get; set; }
    public int? TopSpeed { get; set; }
    public decimal? FuelConsumptionCity { get; set; }
    public decimal? FuelConsumptionHighway { get; set; }
    public decimal? FuelConsumptionCombined { get; set; }
    public int? FuelTankCapacity { get; set; }
    public int? LengthMm { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? WheelbaseMm { get; set; }
    public int? GroundClearanceMm { get; set; }
    public int? CurbWeightKg { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VehicleVariantEntity Variant { get; set; } = null!;
}
