# AUTOHUB — ASP.NET Core

Solution Razor Pages cho website showroom AUTOHUB. Giao diện duy trì thiết kế **Monolith Velocity**: nền graphite tối, Signal Red dạng đường apex, lựa chọn xe theo telemetry và hiệu ứng chuyển động tiết chế.

## Kiến trúc

| Thành phần | Vai trò |
| --- | --- |
| `Pages/` | Razor Pages và layout giao diện |
| `Models/` | Entity models (Brand, Vehicle, VehicleColor, VehicleImage, VehicleSpecification) |
| `Data/` | AutoHubDbContext + AutoHubSeeder |
| `Services/` | VehicleService — truy xuất dữ liệu từ SQL Server |
| `wwwroot/Images/` | Ảnh xe, logo và tài sản hình ảnh |
| `wwwroot/css/` | Hệ visual Monolith Velocity |
| `wwwroot/js/` | Hero pointer motion, modal cấu hình, mobile menu và reveal on-scroll |

## Chạy ứng dụng

Cài .NET SDK 8 và SQL Server, sau đó chạy:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Mở `https://localhost:7171` hoặc `http://localhost:5171`.

## API

| Endpoint | Mô tả |
| --- | --- |
| `GET /api/vehicles?brand=` | Danh sách xe, lọc theo thương hiệu |
| `GET /api/vehicles/search?q=` | Tìm kiếm xe |
| `GET /api/vehicles/{slug}` | Chi tiết xe theo slug |
| `GET /api/brands` | Danh sách thương hiệu |

## Database

Dữ liệu xe được quản lý duy nhất qua SQL Server. Thêm/sửa/xóa xe bằng cách cập nhật database, không cần sửa source code.
