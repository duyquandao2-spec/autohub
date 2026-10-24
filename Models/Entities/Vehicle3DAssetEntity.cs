namespace AutoHub.Models.Entities;

public class Vehicle3DAssetEntity
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    public string? GlbPath { get; set; }
    public string? GlbFileName { get; set; }
    public string? GlbMimeType { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ModelVersion { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VehicleVariantEntity Variant { get; set; } = null!;
}
