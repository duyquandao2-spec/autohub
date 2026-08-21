namespace AutoHub.Models.Entities;

public class VariantFeatureEntity
{
    public int VariantId { get; set; }
    public int FeatureId { get; set; }
    public string? FeatureValue { get; set; }
    public bool IsStandard { get; set; } = true;

    public VehicleVariantEntity Variant { get; set; } = null!;
    public FeatureEntity Feature { get; set; } = null!;
}
