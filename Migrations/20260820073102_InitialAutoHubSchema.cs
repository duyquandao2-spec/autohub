using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialAutoHubSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyTypes",
                columns: table => new
                {
                    BodyTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyTypes", x => x.BodyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoMediaId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FinishType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "DriveTypes",
                columns: table => new
                {
                    DriveTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveTypes", x => x.DriveTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    FeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.FeatureId);
                });

            migrationBuilder.CreateTable(
                name: "FuelTypes",
                columns: table => new
                {
                    FuelTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelTypes", x => x.FuelTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.MediaId);
                });

            migrationBuilder.CreateTable(
                name: "TransmissionTypes",
                columns: table => new
                {
                    TransmissionTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionTypes", x => x.TransmissionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    ModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    BodyTypeId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Generation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModelYearFrom = table.Column<int>(type: "int", nullable: true),
                    ModelYearTo = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModels", x => x.ModelId);
                    table.ForeignKey(
                        name: "FK_VehicleModels_BodyTypes_BodyTypeId",
                        column: x => x.BodyTypeId,
                        principalTable: "BodyTypes",
                        principalColumn: "BodyTypeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VehicleModels_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleVariants",
                columns: table => new
                {
                    VariantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    VariantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModelYear = table.Column<int>(type: "int", nullable: true),
                    MarketCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FuelTypeId = table.Column<int>(type: "int", nullable: false),
                    TransmissionTypeId = table.Column<int>(type: "int", nullable: false),
                    DriveTypeId = table.Column<int>(type: "int", nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsNewArrival = table.Column<bool>(type: "bit", nullable: false),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleVariants", x => x.VariantId);
                    table.ForeignKey(
                        name: "FK_VehicleVariants_DriveTypes_DriveTypeId",
                        column: x => x.DriveTypeId,
                        principalTable: "DriveTypes",
                        principalColumn: "DriveTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleVariants_FuelTypes_FuelTypeId",
                        column: x => x.FuelTypeId,
                        principalTable: "FuelTypes",
                        principalColumn: "FuelTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleVariants_TransmissionTypes_TransmissionTypeId",
                        column: x => x.TransmissionTypeId,
                        principalTable: "TransmissionTypes",
                        principalColumn: "TransmissionTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleVariants_VehicleModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VariantColors",
                columns: table => new
                {
                    VariantColorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    ColorId = table.Column<int>(type: "int", nullable: false),
                    ExtraPrice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantColors", x => x.VariantColorId);
                    table.ForeignKey(
                        name: "FK_VariantColors_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "ColorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VariantColors_VehicleVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "VehicleVariants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantFeatures",
                columns: table => new
                {
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    FeatureValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsStandard = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantFeatures", x => new { x.VariantId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_VariantFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VariantFeatures_VehicleVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "VehicleVariants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantMedia",
                columns: table => new
                {
                    VariantMediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    MediaRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantMedia", x => x.VariantMediaId);
                    table.ForeignKey(
                        name: "FK_VariantMedia_MediaAssets_MediaId",
                        column: x => x.MediaId,
                        principalTable: "MediaAssets",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VariantMedia_VehicleVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "VehicleVariants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleSpecifications",
                columns: table => new
                {
                    SpecificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    EngineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EngineDisplacement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CylinderCount = table.Column<int>(type: "int", nullable: true),
                    Horsepower = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Torque = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Acceleration0To100 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TopSpeed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelConsumptionCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelConsumptionHighway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelConsumptionCombined = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelTankCapacity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LengthMm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WidthMm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HeightMm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WheelbaseMm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroundClearanceMm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurbWeightKg = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleSpecifications", x => x.SpecificationId);
                    table.ForeignKey(
                        name: "FK_VehicleSpecifications_VehicleVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "VehicleVariants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BodyTypes_Name",
                table: "BodyTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BodyTypes_Slug",
                table: "BodyTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colors_Name",
                table: "Colors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriveTypes_Name",
                table: "DriveTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriveTypes_Slug",
                table: "DriveTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_Name",
                table: "Features",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelTypes_Name",
                table: "FuelTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelTypes_Slug",
                table: "FuelTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionTypes_Name",
                table: "TransmissionTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionTypes_Slug",
                table: "TransmissionTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantColors_ColorId",
                table: "VariantColors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantColors_VariantId",
                table: "VariantColors",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantColors_VariantId_ColorId",
                table: "VariantColors",
                columns: new[] { "VariantId", "ColorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantFeatures_FeatureId",
                table: "VariantFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantFeatures_VariantId",
                table: "VariantFeatures",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantMedia_MediaId",
                table: "VariantMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantMedia_VariantId",
                table: "VariantMedia",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantMedia_VariantId_IsPrimary",
                table: "VariantMedia",
                columns: new[] { "VariantId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BodyTypeId",
                table: "VehicleModels",
                column: "BodyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandId_Name",
                table: "VehicleModels",
                columns: new[] { "BrandId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_Slug",
                table: "VehicleModels",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleSpecifications_VariantId",
                table: "VehicleSpecifications",
                column: "VariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_DriveTypeId",
                table: "VehicleVariants",
                column: "DriveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_FuelTypeId",
                table: "VehicleVariants",
                column: "FuelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_IsFeatured",
                table: "VehicleVariants",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_IsNewArrival",
                table: "VehicleVariants",
                column: "IsNewArrival");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_IsPopular",
                table: "VehicleVariants",
                column: "IsPopular");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_ModelId",
                table: "VehicleVariants",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_Slug",
                table: "VehicleVariants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariants_TransmissionTypeId",
                table: "VehicleVariants",
                column: "TransmissionTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariantColors");

            migrationBuilder.DropTable(
                name: "VariantFeatures");

            migrationBuilder.DropTable(
                name: "VariantMedia");

            migrationBuilder.DropTable(
                name: "VehicleSpecifications");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "VehicleVariants");

            migrationBuilder.DropTable(
                name: "DriveTypes");

            migrationBuilder.DropTable(
                name: "FuelTypes");

            migrationBuilder.DropTable(
                name: "TransmissionTypes");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropTable(
                name: "BodyTypes");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
