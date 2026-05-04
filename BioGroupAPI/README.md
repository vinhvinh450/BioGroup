# BioGroup API Backend

Backend ASP.NET Core Web API cho BioWraps Vietnam Shop. API này lấy dữ liệu sản phẩm từ SQL Server database.

## Yêu Cầu

- .NET 8.0 SDK trở lên
- SQL Server 2019 hoặc mới hơn (hoặc SQL Server Express)
- Windows, macOS hoặc Linux

## Cài Đặt

### 1. Cài đặt SQL Server (nếu chưa có)

**Windows:**
- Download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
- Hoặc cài SQL Server Management Studio (SSMS)

**macOS/Linux:**
- Dùng Docker: `docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourPassword123' -p 1433:1433 mcr.microsoft.com/mssql/server:latest`

### 2. Configure Connection String

Mở file `appsettings.json` và thay đổi connection string nếu cần:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BioGroupDB;Integrated Security=true;TrustServerCertificate=true;"
}
```

**Nếu dùng SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=BioGroupDB;Integrated Security=true;TrustServerCertificate=true;
```

**Nếu dùng password:**
```
Server=localhost;Database=BioGroupDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;
```

### 3. Build & Run

```powershell
# Build
dotnet build

# Run
dotnet run

# Hoặc chỉ chạy (sẽ build tự động)
dotnet run --launch-profile https
```

API sẽ chạy ở: `https://localhost:5001` hoặc `http://localhost:5000`

### 4. Test API

Mở browser và truy cập:
- Swagger UI: `https://localhost:5001/swagger/ui`
- Products list: `https://localhost:5001/api/products`

## API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/products` | Lấy tất cả sản phẩm |
| GET | `/api/products/{id}` | Lấy sản phẩm theo ID |
| GET | `/api/products/category/{category}` | Lấy sản phẩm theo category |
| GET | `/api/products/featured` | Lấy sản phẩm nổi bật |
| GET | `/api/products/search/{keyword}` | Tìm kiếm sản phẩm |
| POST | `/api/products` | Tạo sản phẩm mới |
| PUT | `/api/products/{id}` | Cập nhật sản phẩm |
| DELETE | `/api/products/{id}` | Xóa sản phẩm |

## Ví dụ Response

```json
[
  {
    "id": 1,
    "name": "Bao Bì Thực Phẩm Phân Hủy Sinh Học",
    "category": "Packaging",
    "badge": "Mới",
    "badgeClass": "new",
    "icon": "📦",
    "imageUrl": "https://...",
    "description": "Bao bì thực phẩm compostable 100%...",
    "isFeatured": false,
    "createdAt": "2026-04-21T00:00:00Z"
  }
]
```

## Database

Database sẽ được tạo tự động khi chạy API lần đầu. Nó sẽ có:
- Bảng `Products` với 9 sản phẩm mẫu
- Đầy đủ các trường: Id, Name, Category, Badge, Icon, ImageUrl, Description, IsFeatured, CreatedAt

## Frontend Integration

Frontend (BioGroup project) đã được cấu hình để fetch dữ liệu từ API:

```javascript
// js/data.js
const API_URL = 'http://localhost:5000/api/products';
await loadProductsFromAPI();
```

**Lưu ý:** Nếu API chạy trên port khác, thay đổi `API_URL` trong `js/data.js`.

## Deploy

### Development
```
dotnet run
```

### Production
```
dotnet publish -c Release -o ./publish
```

Sau đó deploy folder `publish` lên server.

## Troubleshooting

**Lỗi: "Cannot connect to SQL Server"**
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `appsettings.json`
- Thử `Server=.\\SQLEXPRESS` thay vì `localhost`

**Lỗi: CORS issues**
- CORS đã được enable cho mọi origin (`AllowAll` policy)
- Nếu vẫn gặp lỗi, kiểm tra frontend request có gửi đúng API URL không

**Database không được tạo**
- Thêm vào `Program.cs`: `dbContext.Database.EnsureCreated();`
- Hoặc xóa database cũ rồi chạy lại

## Support

Nếu gặp lỗi:
1. Kiểm tra console output
2. Xem Swagger UI để test endpoints
3. Kiểm tra SQL Server logs
