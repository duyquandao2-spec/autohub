using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoHub.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PreferredContactMethod { get; set; }
    public bool MarketingConsent { get; set; }
    public string? AvatarPath { get; set; }
    public bool IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConsultationRequestEntity> Consultations { get; set; } = new List<ConsultationRequestEntity>();
    public ICollection<TestDriveBookingEntity> TestDrives { get; set; } = new List<TestDriveBookingEntity>();
    public ICollection<FavoriteEntity> Favorites { get; set; } = new List<FavoriteEntity>();
    public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
}
