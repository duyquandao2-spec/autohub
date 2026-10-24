namespace AutoHub.Models.Entities;

public class NewsCategoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<NewsArticleEntity> Articles { get; set; } = new List<NewsArticleEntity>();
}
