namespace AutoHub.Models.Entities;

public class VariantColorEntity
{
    public int VariantColorId { get; set; }
    public int VariantId { get; set; }
    public int ColorId { get; set; }
    public string? ExtraPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; }

    public VehicleVariantEntity Variant { get; set; } = null!;
    public ColorEntity Color { get; set; } = null!;
}
