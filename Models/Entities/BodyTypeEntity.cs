namespace AutoHub.Models.Entities;

public class BodyTypeEntity
{
    public int BodyTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<VehicleModelEntity> Models { get; set; } = new List<VehicleModelEntity>();
}
