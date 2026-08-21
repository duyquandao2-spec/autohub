namespace AutoHub.Models.Entities;

public class DriveTypeEntity
{
    public int DriveTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<VehicleVariantEntity> Variants { get; set; } = new List<VehicleVariantEntity>();
}
