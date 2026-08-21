using AutoHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Data;

public class AutoHubDbContext : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BrandEntity>(e =>
        {
            e.HasKey(b => b.BrandId);
            e.HasIndex(b => b.Name).IsUnique();
            e.HasIndex(b => b.Slug).IsUnique();
            e.Property(b => b.Name).IsRequired().HasMaxLength(100);
            e.Property(b => b.Slug).IsRequired().HasMaxLength(100);
            e.Property(b => b.Country).HasMaxLength(100);
            e.Property(b => b.WebsiteUrl).HasMaxLength(500);
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
    }
}
