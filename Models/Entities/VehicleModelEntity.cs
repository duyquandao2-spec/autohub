namespace AutoHub.Models.Entities;

public class VehicleModelEntity
{
    public int ModelId { get; set; }
    public int BrandId { get; set; }
    public int? BodyTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Generation { get; set; }
    public int? ModelYearFrom { get; set; }
    public int? ModelYearTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public BrandEntity Brand { get; set; } = null!;
    public BodyTypeEntity? BodyType { get; set; }
    public ICollection<VehicleVariantEntity> Variants { get; set; } = new List<VehicleVariantEntity>();
}
