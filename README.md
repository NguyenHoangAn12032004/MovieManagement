# Movie Management System

Hệ thống quản lý rạp chiếu phim được xây dựng bằng ASP.NET Core MVC với Entity Framework Core.

## Tính năng chính

- **Quản lý phim**: Thêm, sửa, xóa thông tin phim
- **Quản lý lịch chiếu**: Tạo và quản lý các suất chiếu
- **Đặt vé**: Cho phép khách hàng đặt vé và chọn ghế
- **Quản lý bắp nước**: Đặt đồ ăn và thức uống
- **Hệ thống thanh toán**: Xử lý thanh toán trực tuyến
- **Quản lý nhân viên**: Phân quyền và quản lý tài khoản nhân viên
- **Đa ngôn ngữ**: Hỗ trợ tiếng Việt và tiếng Anh

## Công nghệ sử dụng

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap
- SignalR (cho tính năng real-time)
- Identity Framework (xác thực và phân quyền)

## Cài đặt

1. Clone repository:
```bash
git clone <repository-url>
cd MovieManagement
```

2. Cập nhật connection string trong `appsettings.json`

3. Chạy migration để tạo database:
```bash
dotnet ef database update
```

4. Chạy ứng dụng:
```bash
dotnet run
```

## Cấu trúc dự án

- `Controllers/`: Chứa các controllers xử lý logic nghiệp vụ
- `Models/`: Chứa các model entities và view models
- `Views/`: Chứa các view templates
- `Services/`: Chứa các service classes
- `Data/`: Chứa DbContext và các seeder
- `Areas/`: Chứa các area cho Admin và Identity
- `wwwroot/`: Chứa static files (CSS, JS, images)

## Tác giả

Dự án Movie Management System

## License

This project is licensed under the MIT License.
