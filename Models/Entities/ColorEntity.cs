namespace AutoHub.Models.Entities;

public class ColorEntity
{
    public int ColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public string? HexCode { get; set; }
    public string? FinishType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VariantColorEntity> VariantColors { get; set; } = new List<VariantColorEntity>();
}
