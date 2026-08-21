namespace AutoHub.Models.Entities;

public class FeatureEntity
{
    public int FeatureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VariantFeatureEntity> VariantFeatures { get; set; } = new List<VariantFeatureEntity>();
}
