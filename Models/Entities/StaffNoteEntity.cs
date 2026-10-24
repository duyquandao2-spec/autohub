namespace AutoHub.Models.Entities;

public class StaffNoteEntity
{
    public int Id { get; set; }
    public int? ConsultationId { get; set; }
    public int? TestDriveId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ConsultationRequestEntity? Consultation { get; set; }
    public TestDriveBookingEntity? TestDrive { get; set; }
}
