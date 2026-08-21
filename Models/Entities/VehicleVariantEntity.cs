namespace AutoHub.Models.Entities;

public class VehicleVariantEntity
{
    public int VariantId { get; set; }
    public int ModelId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int? ModelYear { get; set; }
    public string? MarketCode { get; set; }
    public decimal BasePrice { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public int FuelTypeId { get; set; }
    public int TransmissionTypeId { get; set; }
    public int DriveTypeId { get; set; }
    public int SeatCount { get; set; } = 5;
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsPopular { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string Status { get; set; } = "ACTIVE";
    public string? Model3DPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VehicleModelEntity Model { get; set; } = null!;
    public FuelTypeEntity FuelType { get; set; } = null!;
    public TransmissionTypeEntity TransmissionType { get; set; } = null!;
    public DriveTypeEntity DriveType { get; set; } = null!;
    public VehicleSpecificationEntity? Specification { get; set; }
    public ICollection<VariantColorEntity> VariantColors { get; set; } = new List<VariantColorEntity>();
    public ICollection<VariantMediaEntity> VariantMedia { get; set; } = new List<VariantMediaEntity>();
    public ICollection<VariantFeatureEntity> VariantFeatures { get; set; } = new List<VariantFeatureEntity>();
}
