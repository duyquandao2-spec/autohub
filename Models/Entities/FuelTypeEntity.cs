namespace AutoHub.Models.Entities;

public class FuelTypeEntity
{
    public int FuelTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<VehicleVariantEntity> Variants { get; set; } = new List<VehicleVariantEntity>();
}
