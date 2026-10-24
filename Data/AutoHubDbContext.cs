using AutoHub.Models.Entities;
using AutoHub.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Data;

public class AutoHubDbContext : IdentityDbContext<ApplicationUser>
{
    public AutoHubDbContext(DbContextOptions<AutoHubDbContext> options) : base(options) { }

    public DbSet<BrandEntity> Brands => Set<BrandEntity>();
    public DbSet<BodyTypeEntity> BodyTypes => Set<BodyTypeEntity>();
    public DbSet<FuelTypeEntity> FuelTypes => Set<FuelTypeEntity>();
    public DbSet<TransmissionTypeEntity> TransmissionTypes => Set<TransmissionTypeEntity>();
    public DbSet<DriveTypeEntity> DriveTypes => Set<DriveTypeEntity>();
    public DbSet<VehicleModelEntity> VehicleModels => Set<VehicleModelEntity>();
    public DbSet<VehicleVariantEntity> VehicleVariants => Set<VehicleVariantEntity>();
    public DbSet<VehicleSpecificationEntity> VehicleSpecifications => Set<VehicleSpecificationEntity>();
    public DbSet<ColorEntity> Colors => Set<ColorEntity>();
    public DbSet<VariantColorEntity> VariantColors => Set<VariantColorEntity>();
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();
    public DbSet<VariantFeatureEntity> VariantFeatures => Set<VariantFeatureEntity>();
    public DbSet<MediaAssetEntity> MediaAssets => Set<MediaAssetEntity>();
    public DbSet<VariantMediaEntity> VariantMedia => Set<VariantMediaEntity>();
    public DbSet<TestDriveBookingEntity> TestDriveBookings => Set<TestDriveBookingEntity>();
    public DbSet<ContactMessageEntity> ContactMessages => Set<ContactMessageEntity>();
    public DbSet<ConsultationRequestEntity> ConsultationRequests => Set<ConsultationRequestEntity>();
    public DbSet<FavoriteEntity> Favorites => Set<FavoriteEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<NewsArticleEntity> NewsArticles => Set<NewsArticleEntity>();
    public DbSet<NewsCategoryEntity> NewsCategories => Set<NewsCategoryEntity>();
    public DbSet<SiteSettingEntity> SiteSettings => Set<SiteSettingEntity>();
    public DbSet<ShowroomEntity> Showrooms => Set<ShowroomEntity>();
    public DbSet<StaffNoteEntity> StaffNotes => Set<StaffNoteEntity>();
    public DbSet<Vehicle3DAssetEntity> Vehicle3DAssets => Set<Vehicle3DAssetEntity>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(150);
            e.Property(u => u.PhoneCode).HasMaxLength(20);
            e.Property(u => u.PreferredContactMethod).HasMaxLength(50);
            e.Property(u => u.AvatarPath).HasMaxLength(500);
        });

        modelBuilder.Entity<BrandEntity>(e =>
        {
            e.HasKey(b => b.BrandId);
            e.HasIndex(b => b.Name).IsUnique();
            e.HasIndex(b => b.Slug).IsUnique();
            e.Property(b => b.Name).IsRequired().HasMaxLength(100);
            e.Property(b => b.Slug).IsRequired().HasMaxLength(100);
            e.Property(b => b.Country).HasMaxLength(100);
            e.Property(b => b.WebsiteUrl).HasMaxLength(500);
            e.Property(b => b.LogoPath).HasMaxLength(500);
        });

        modelBuilder.Entity<BodyTypeEntity>(e =>
        {
            e.HasKey(b => b.BodyTypeId);
            e.HasIndex(b => b.Name).IsUnique();
            e.HasIndex(b => b.Slug).IsUnique();
            e.Property(b => b.Name).IsRequired().HasMaxLength(50);
            e.Property(b => b.Slug).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<FuelTypeEntity>(e =>
        {
            e.HasKey(f => f.FuelTypeId);
            e.HasIndex(f => f.Name).IsUnique();
            e.HasIndex(f => f.Slug).IsUnique();
            e.Property(f => f.Name).IsRequired().HasMaxLength(50);
            e.Property(f => f.Slug).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<TransmissionTypeEntity>(e =>
        {
            e.HasKey(t => t.TransmissionTypeId);
            e.HasIndex(t => t.Name).IsUnique();
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).IsRequired().HasMaxLength(50);
            e.Property(t => t.Slug).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<DriveTypeEntity>(e =>
        {
            e.HasKey(d => d.DriveTypeId);
            e.HasIndex(d => d.Name).IsUnique();
            e.HasIndex(d => d.Slug).IsUnique();
            e.Property(d => d.Name).IsRequired().HasMaxLength(50);
            e.Property(d => d.Slug).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<VehicleModelEntity>(e =>
        {
            e.HasKey(m => m.ModelId);
            e.HasIndex(m => new { m.BrandId, m.Name }).IsUnique();
            e.HasIndex(m => m.Slug).IsUnique();
            e.Property(m => m.Name).IsRequired().HasMaxLength(100);
            e.Property(m => m.Slug).IsRequired().HasMaxLength(200);
            e.Property(m => m.Generation).HasMaxLength(50);

            e.HasOne(m => m.Brand)
             .WithMany(b => b.Models)
             .HasForeignKey(m => m.BrandId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.BodyType)
             .WithMany(bt => bt.Models)
             .HasForeignKey(m => m.BodyTypeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VehicleVariantEntity>(e =>
        {
            e.HasKey(v => v.VariantId);
            e.HasIndex(v => v.Slug).IsUnique();
            e.HasIndex(v => v.IsFeatured);
            e.HasIndex(v => v.IsNewArrival);
            e.HasIndex(v => v.IsPopular);
            e.HasIndex(v => v.ModelId);
            e.HasIndex(v => v.FuelTypeId);
            e.HasIndex(v => v.TransmissionTypeId);
            e.HasIndex(v => v.DriveTypeId);
            e.HasIndex(v => v.Status);
            e.Property(v => v.VariantName).IsRequired().HasMaxLength(100);
            e.Property(v => v.Slug).IsRequired().HasMaxLength(200);
            e.Property(v => v.MarketCode).HasMaxLength(50);
            e.Property(v => v.CurrencyCode).IsRequired().HasMaxLength(10);
            e.Property(v => v.BasePrice).HasColumnType("decimal(19,2)");
            e.Property(v => v.Description).HasMaxLength(2000);
            e.Property(v => v.Status).IsRequired().HasMaxLength(50);
            e.Property(v => v.Model3DPath).HasMaxLength(500);

            e.HasOne(v => v.Model)
             .WithMany(m => m.Variants)
             .HasForeignKey(v => v.ModelId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.FuelType)
             .WithMany(f => f.Variants)
             .HasForeignKey(v => v.FuelTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.TransmissionType)
             .WithMany(t => t.Variants)
             .HasForeignKey(v => v.TransmissionTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.DriveType)
             .WithMany(d => d.Variants)
             .HasForeignKey(v => v.DriveTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VehicleSpecificationEntity>(e =>
        {
            e.HasKey(s => s.SpecificationId);
            e.HasIndex(s => s.VariantId).IsUnique();
            e.Property(s => s.EngineType).HasMaxLength(100);
            e.Property(s => s.Acceleration0To100).HasPrecision(4, 1);
            e.Property(s => s.FuelConsumptionCity).HasPrecision(4, 1);
            e.Property(s => s.FuelConsumptionHighway).HasPrecision(4, 1);
            e.Property(s => s.FuelConsumptionCombined).HasPrecision(4, 1);

            e.HasOne(s => s.Variant)
             .WithOne(v => v.Specification)
             .HasForeignKey<VehicleSpecificationEntity>(s => s.VariantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ColorEntity>(e =>
        {
            e.HasKey(c => c.ColorId);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.ColorCode).IsRequired().HasMaxLength(50);
            e.Property(c => c.HexCode).HasMaxLength(20);
            e.Property(c => c.FinishType).HasMaxLength(50);
        });

        modelBuilder.Entity<VariantColorEntity>(e =>
        {
            e.HasKey(vc => vc.VariantColorId);
            e.HasIndex(vc => new { vc.VariantId, vc.ColorId }).IsUnique();
            e.HasIndex(vc => vc.VariantId);
            e.Property(vc => vc.ExtraPrice).HasMaxLength(100);

            e.HasOne(vc => vc.Variant)
             .WithMany(v => v.VariantColors)
             .HasForeignKey(vc => vc.VariantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vc => vc.Color)
             .WithMany(c => c.VariantColors)
             .HasForeignKey(vc => vc.ColorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeatureEntity>(e =>
        {
            e.HasKey(f => f.FeatureId);
            e.HasIndex(f => f.Name).IsUnique();
            e.Property(f => f.Name).IsRequired().HasMaxLength(100);
            e.Property(f => f.Category).HasMaxLength(100);
            e.Property(f => f.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<VariantFeatureEntity>(e =>
        {
            e.HasKey(vf => new { vf.VariantId, vf.FeatureId });
            e.HasIndex(vf => vf.VariantId);
            e.Property(vf => vf.FeatureValue).HasMaxLength(200);

            e.HasOne(vf => vf.Variant)
             .WithMany(v => v.VariantFeatures)
             .HasForeignKey(vf => vf.VariantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vf => vf.Feature)
             .WithMany(f => f.VariantFeatures)
             .HasForeignKey(vf => vf.FeatureId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaAssetEntity>(e =>
        {
            e.HasKey(m => m.MediaId);
            e.Property(m => m.MediaType).IsRequired().HasMaxLength(50);
            e.Property(m => m.FileName).IsRequired().HasMaxLength(255);
            e.Property(m => m.FilePath).IsRequired().HasMaxLength(500);
            e.Property(m => m.MimeType).HasMaxLength(100);
            e.Property(m => m.AltText).HasMaxLength(500);
            e.Property(m => m.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<VariantMediaEntity>(e =>
        {
            e.HasKey(vm => vm.VariantMediaId);
            e.HasIndex(vm => vm.VariantId);
            e.HasIndex(vm => new { vm.VariantId, vm.IsPrimary });
            e.Property(vm => vm.MediaRole).IsRequired().HasMaxLength(50);

            e.HasOne(vm => vm.Variant)
             .WithMany(v => v.VariantMedia)
             .HasForeignKey(vm => vm.VariantId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vm => vm.Media)
             .WithMany(m => m.VariantMedia)
             .HasForeignKey(vm => vm.MediaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TestDriveBookingEntity>(e =>
        {
            e.HasKey(b => b.BookingId);
            e.HasIndex(b => b.Status);
            e.HasIndex(b => b.CreatedAt);
            e.HasIndex(b => b.UserId);
            e.HasIndex(b => b.PreferredDate);
            e.HasIndex(b => b.AssignedStaffId);
            e.Property(b => b.FullName).IsRequired().HasMaxLength(100);
            e.Property(b => b.Phone).IsRequired().HasMaxLength(20);
            e.Property(b => b.Email).HasMaxLength(200);
            e.Property(b => b.TimeSlot).IsRequired().HasMaxLength(50);
            e.Property(b => b.Notes).HasMaxLength(1000);
            e.Property(b => b.Status).IsRequired().HasMaxLength(50);

            e.HasOne(b => b.Variant)
             .WithMany()
             .HasForeignKey(b => b.VariantId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.User)
             .WithMany(u => u.TestDrives)
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Showroom)
             .WithMany(s => s.TestDriveBookings)
             .HasForeignKey(b => b.ShowroomId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ContactMessageEntity>(e =>
        {
            e.HasKey(m => m.MessageId);
            e.HasIndex(m => m.IsRead);
            e.HasIndex(m => m.CreatedAt);
            e.Property(m => m.FullName).IsRequired().HasMaxLength(100);
            e.Property(m => m.Phone).IsRequired().HasMaxLength(20);
            e.Property(m => m.Email).HasMaxLength(200);
            e.Property(m => m.Subject).IsRequired().HasMaxLength(200);
            e.Property(m => m.Content).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<ConsultationRequestEntity>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => c.PreferredDate);
            e.HasIndex(c => c.AssignedStaffId);
            e.HasIndex(c => c.CreatedAt);
            e.Property(c => c.FullName).IsRequired().HasMaxLength(100);
            e.Property(c => c.Phone).IsRequired().HasMaxLength(20);
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.PreferredTime).HasMaxLength(50);
            e.Property(c => c.ConsultationMethod).HasMaxLength(50);
            e.Property(c => c.CallbackPreference).HasMaxLength(50);
            e.Property(c => c.Budget).HasMaxLength(50);
            e.Property(c => c.Message).HasMaxLength(2000);
            e.Property(c => c.Status).IsRequired().HasMaxLength(50);

            e.HasOne(c => c.Vehicle)
             .WithMany()
             .HasForeignKey(c => c.VehicleId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(c => c.User)
             .WithMany(u => u.Consultations)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FavoriteEntity>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => new { f.UserId, f.VehicleId }).IsUnique();

            e.HasOne(f => f.User)
             .WithMany(u => u.Favorites)
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.Vehicle)
             .WithMany()
             .HasForeignKey(f => f.VehicleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLogEntity>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.EntityType);
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.Action);
            e.Property(a => a.Action).IsRequired().HasMaxLength(100);
            e.Property(a => a.EntityType).HasMaxLength(50);
            e.Property(a => a.EntityId).HasMaxLength(100);
            e.Property(a => a.Result).HasMaxLength(50);
            e.Property(a => a.IpAddress).HasMaxLength(64);
            e.Property(a => a.UserName).HasMaxLength(256);

            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NewsCategoryEntity>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Slug).IsUnique();
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Slug).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<NewsArticleEntity>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Slug).IsUnique();
            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.PublishedAt);
            e.HasIndex(a => a.Kind);
            e.Property(a => a.Title).IsRequired().HasMaxLength(300);
            e.Property(a => a.Slug).IsRequired().HasMaxLength(350);
            e.Property(a => a.Excerpt).HasMaxLength(1000);
            e.Property(a => a.CoverImagePath).HasMaxLength(500);
            e.Property(a => a.AuthorName).HasMaxLength(150);
            e.Property(a => a.Kind).HasMaxLength(50);
            e.Property(a => a.Status).HasMaxLength(50);

            e.HasOne(a => a.Category)
             .WithMany(c => c.Articles)
             .HasForeignKey(a => a.CategoryId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SiteSettingEntity>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Key).IsUnique();
            e.Property(s => s.Key).IsRequired().HasMaxLength(200);
            e.Property(s => s.Value).HasMaxLength(2000);
            e.Property(s => s.Group).HasMaxLength(100);
        });

        modelBuilder.Entity<ShowroomEntity>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Name).IsUnique();
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Code).HasMaxLength(50);
            e.Property(s => s.Address).HasMaxLength(500);
            e.Property(s => s.Phone).HasMaxLength(20);
            e.Property(s => s.Email).HasMaxLength(200);
            e.Property(s => s.BusinessHours).HasMaxLength(500);
        });

        modelBuilder.Entity<StaffNoteEntity>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.ConsultationId);
            e.HasIndex(n => n.TestDriveId);
            e.Property(n => n.AuthorName).HasMaxLength(150);
            e.Property(n => n.Note).IsRequired().HasMaxLength(2000);

            e.HasOne(n => n.Consultation)
             .WithMany(c => c.Notes)
             .HasForeignKey(n => n.ConsultationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.TestDrive)
             .WithMany()
             .HasForeignKey(n => n.TestDriveId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle3DAssetEntity>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.VariantId);
            e.Property(a => a.GlbPath).HasMaxLength(500);
            e.Property(a => a.GlbFileName).HasMaxLength(255);
            e.Property(a => a.GlbMimeType).HasMaxLength(100);
            e.Property(a => a.ModelVersion).HasMaxLength(50);

            e.HasOne(a => a.Variant)
             .WithMany()
             .HasForeignKey(a => a.VariantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.UserId);
            e.HasIndex(n => n.IsRead);
            e.Property(n => n.Type).HasMaxLength(50);
            e.Property(n => n.Title).IsRequired().HasMaxLength(200);
            e.Property(n => n.Message).HasMaxLength(1000);
            e.Property(n => n.Link).HasMaxLength(500);

            e.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
