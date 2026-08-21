# Images

Thư mục này tập trung toàn bộ tài sản hình ảnh của website. Razor Pages và CSS chỉ tham chiếu ảnh qua đường dẫn bắt đầu bằng `/Images/`.

## Ảnh chung

| Tệp | Vai trò |
| --- | --- |
| `apex-logo.png` | Biểu tượng thương hiệu và favicon |
| `hero-graphite.jpg` | Ảnh hero chính |
| `aero-study.jpg` | Ảnh section khí động học |
| `cockpit-focus.jpg` | Ảnh section cockpit |

## Ảnh xe

Mỗi thương hiệu có thư mục riêng với các model con. Ảnh được quản lý thông qua database (bảng `Vehicles` và `VehicleImages`), không hard-code trong source.
