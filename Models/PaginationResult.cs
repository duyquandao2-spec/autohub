namespace AutoHub.Models;

public class PaginationResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
