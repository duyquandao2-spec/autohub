namespace AutoHub.Models.Entities;

public class MediaAssetEntity
{
    public int MediaId { get; set; }
    public string MediaType { get; set; } = "IMAGE";
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? AltText { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VariantMediaEntity> VariantMedia { get; set; } = new List<VariantMediaEntity>();
}
