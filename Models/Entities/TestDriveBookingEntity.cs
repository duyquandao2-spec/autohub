namespace AutoHub.Models.Entities;

public class TestDriveBookingEntity
{
    public int BookingId { get; set; }
    public string? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? VariantId { get; set; }
    public DateOnly PreferredDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public int? ShowroomId { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? AssignedStaffId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VehicleVariantEntity? Variant { get; set; }
    public ShowroomEntity? Showroom { get; set; }
    public AutoHub.Models.Identity.ApplicationUser? User { get; set; }
}
