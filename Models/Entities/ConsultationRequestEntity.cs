namespace AutoHub.Models.Entities;

public class ConsultationRequestEntity
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? PreferredDate { get; set; }
    public string? PreferredTime { get; set; }
    public string? ConsultationMethod { get; set; }
    public int? VehicleId { get; set; }
    public string? Budget { get; set; }
    public string? Message { get; set; }
    public string? CallbackPreference { get; set; }
    public string Status { get; set; } = "NEW";
    public string? AssignedStaffId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VehicleVariantEntity? Vehicle { get; set; }
    public AutoHub.Models.Identity.ApplicationUser? User { get; set; }
    public ICollection<StaffNoteEntity> Notes { get; set; } = new List<StaffNoteEntity>();
}
