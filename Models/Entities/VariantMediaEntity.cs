namespace AutoHub.Models.Entities;

public class VariantMediaEntity
{
    public int VariantMediaId { get; set; }
    public int VariantId { get; set; }
    public int MediaId { get; set; }
    public string MediaRole { get; set; } = "GALLERY";
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    public VehicleVariantEntity Variant { get; set; } = null!;
    public MediaAssetEntity Media { get; set; } = null!;
}
