using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHub.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSpecificationsAndAdd3D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model3DPath",
                table: "VehicleVariants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE vs SET
                    vs.EngineDisplacement = CASE WHEN vs.EngineDisplacement LIKE '%cc%' THEN CAST(REPLACE(vs.EngineDisplacement, 'cc', '') AS int) END,
                    vs.Horsepower = CASE WHEN vs.Horsepower LIKE '%HP%' THEN CAST(REPLACE(vs.Horsepower, ' HP', '') AS int) END,
                    vs.Torque = CASE WHEN vs.Torque LIKE '% Nm%' THEN CAST(REPLACE(vs.Torque, ' Nm', '') AS int) END,
                    vs.Acceleration0To100 = CASE WHEN vs.Acceleration0To100 LIKE '%s' THEN CAST(REPLACE(vs.Acceleration0To100, 's', '') AS decimal(4,1)) END,
                    vs.TopSpeed = CASE WHEN vs.TopSpeed LIKE '%km/h%' THEN CAST(REPLACE(vs.TopSpeed, ' km/h', '') AS int) END,
                    vs.FuelConsumptionCombined = CASE WHEN vs.FuelConsumptionCombined LIKE '%L/100km%' THEN CAST(REPLACE(vs.FuelConsumptionCombined, ' L/100km', '') AS decimal(4,1)) END,
                    vs.FuelTankCapacity = CASE WHEN vs.FuelTankCapacity LIKE '% L%' THEN CAST(REPLACE(vs.FuelTankCapacity, ' L', '') AS int) END,
                    vs.LengthMm = CASE WHEN vs.LengthMm LIKE '%mm%' THEN CAST(REPLACE(vs.LengthMm, ' mm', '') AS int) END,
                    vs.WidthMm = CASE WHEN vs.WidthMm LIKE '%mm%' THEN CAST(REPLACE(vs.WidthMm, ' mm', '') AS int) END,
                    vs.HeightMm = CASE WHEN vs.HeightMm LIKE '%mm%' THEN CAST(REPLACE(vs.HeightMm, ' mm', '') AS int) END,
                    vs.WheelbaseMm = CASE WHEN vs.WheelbaseMm LIKE '%mm%' THEN CAST(REPLACE(vs.WheelbaseMm, ' mm', '') AS int) END,
                    vs.GroundClearanceMm = CASE WHEN vs.GroundClearanceMm LIKE '%mm%' THEN CAST(REPLACE(vs.GroundClearanceMm, ' mm', '') AS int) END
                FROM VehicleSpecifications vs
            ");

            migrationBuilder.AlterColumn<int>(
                name: "WidthMm",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WheelbaseMm",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Torque",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TopSpeed",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LengthMm",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Horsepower",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HeightMm",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroundClearanceMm",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FuelTankCapacity",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelConsumptionHighway",
                table: "VehicleSpecifications",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelConsumptionCombined",
                table: "VehicleSpecifications",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelConsumptionCity",
                table: "VehicleSpecifications",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EngineDisplacement",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurbWeightKg",
                table: "VehicleSpecifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Acceleration0To100",
                table: "VehicleSpecifications",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model3DPath",
                table: "VehicleVariants");

            migrationBuilder.AlterColumn<string>(
                name: "WidthMm",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WheelbaseMm",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Torque",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TopSpeed",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LengthMm",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Horsepower",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HeightMm",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GroundClearanceMm",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FuelTankCapacity",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FuelConsumptionHighway",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FuelConsumptionCombined",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FuelConsumptionCity",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EngineDisplacement",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurbWeightKg",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Acceleration0To100",
                table: "VehicleSpecifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);
        }
    }
}
