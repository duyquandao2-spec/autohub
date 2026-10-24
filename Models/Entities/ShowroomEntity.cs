namespace AutoHub.Models.Entities;

public class ShowroomEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? BusinessHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TestDriveBookingEntity> TestDriveBookings { get; set; } = new List<TestDriveBookingEntity>();
}
