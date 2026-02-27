# Thiết kế cấu trúc – Phù hợp với source code hiện tại

## 1. Cấu trúc hiện tại (đã tạo)

```
PRN222-Assignment4-Option1/
├── PRN222-Assignment4-Option1.sln              # 4 project
├── PRN222-Assignment4-Option1/                 # Web MVC (net9.0)
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Models/
│   │   └── ErrorViewModel.cs
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── PRN222-Assignment4-Option1.csproj      # → reference BusinessLogic
│
├── PRN222-Assignment4-Option1.DataAccess/      # Class Library (DAL)
│   ├── Class1.cs                              # (sẽ thay bằng Entities, DbContext, Repositories)
│   └── PRN222-Assignment4-Option1.DataAccess.csproj
│
├── PRN222-Assignment4-Option1.BusinessLogic/   # Class Library (BLL)
│   ├── Class1.cs                              # (sẽ thay bằng Services)
│   └── PRN222-Assignment4-Option1.BusinessLogic.csproj  # → reference DataAccess
│
├── PRN222-Assignment4-Option1.Worker/         # Worker Service
│   ├── Worker.cs                              # BackgroundService (template)
│   ├── Program.cs
│   └── PRN222-Assignment4-Option1.Worker.csproj  # → reference BusinessLogic
│
├── doc.txt
├── DESIGN.md
└── SUMMARY.md
```

- **Tất cả project ở root** (không dùng thư mục `src/`).
- **Đặt tên:** `PRN222-Assignment4-Option1.*` (nhất quán với Web).
- **Solution:** đã có 4 project, references đã cấu hình đúng.

---

## 2. Loại project và references (đã đúng)

| Project | Loại | Reference tới |
|---------|------|----------------|
| **PRN222-Assignment4-Option1** | Web Application (`Microsoft.NET.Sdk.Web`) | `PRN222-Assignment4-Option1.BusinessLogic` |
| **PRN222-Assignment4-Option1.BusinessLogic** | Class Library | `PRN222-Assignment4-Option1.DataAccess` |
| **PRN222-Assignment4-Option1.DataAccess** | Class Library | (chỉ NuGet, ví dụ EF Core) |
| **PRN222-Assignment4-Option1.Worker** | Worker Service (`Microsoft.NET.Sdk.Worker`) | `PRN222-Assignment4-Option1.BusinessLogic` |

---

## 3. Cấu trúc thư mục mục tiêu (khi triển khai đủ)

### 3.1. DataAccess (DAL + Entities gộp chung)

```
PRN222-Assignment4-Option1.DataAccess/
├── Entities/
│   └── ExchangeRate.cs
├── Data/
│   └── AppDbContext.cs
├── Repositories/
│   ├── IExchangeRateRepository.cs
│   └── ExchangeRateRepository.cs
└── PRN222-Assignment4-Option1.DataAccess.csproj
```

### 3.2. BusinessLogic (BLL)

```
PRN222-Assignment4-Option1.BusinessLogic/
├── Services/
│   ├── IExchangeRateService.cs
│   ├── ExchangeRateService.cs
│   └── ExchangeRateApiService.cs    # Gọi API tỷ giá
└── PRN222-Assignment4-Option1.BusinessLogic.csproj
```

### 3.3. Web (Presentation)

```
PRN222-Assignment4-Option1/
├── Controllers/
│   ├── HomeController.cs
│   └── ExchangeRateController.cs   # Danh sách, lọc, mới nhất, thống kê
├── Models/
├── ViewModels/                      # (optional) cho ExchangeRate
├── Views/
│   ├── Home/
│   └── ExchangeRate/                # Index, Latest, Statistics
├── Program.cs                       # Đăng ký DI: DataAccess + BusinessLogic
└── appsettings.json                # ConnectionString, API (nếu cần)
```

### 3.4. Worker

```
PRN222-Assignment4-Option1.Worker/
├── Worker.cs                        # BackgroundService: mỗi 5s gọi BLL, CancellationToken
├── Program.cs                       # Đăng ký DI: DataAccess + BusinessLogic + HttpClient
├── appsettings.json                # ConnectionString, API URL, interval 5s
└── PRN222-Assignment4-Option1.Worker.csproj
```

---

## 4. Namespace (theo code hiện tại)

| Project | RootNamespace / Namespace gốc |
|---------|------------------------------|
| Web | `PRN222_Assignment4_Option1` |
| DataAccess | `PRN222_Assignment4_Option1.DataAccess` |
| BusinessLogic | `PRN222_Assignment4_Option1.BusinessLogic` |
| Worker | `PRN222_Assignment4_Option1.Worker` |

Trong Web khi dùng BLL: `using PRN222_Assignment4_Option1.BusinessLogic.Services;` và inject `IExchangeRateService`.

---

## 5. Điều chỉnh trong Web

- **Program.cs:** Đăng ký DbContext (từ DataAccess), Repository, Services (BLL). Có thể dùng extension `AddExchangeRateServices(...)` trong BLL.
- **Controllers:** Thêm `ExchangeRateController`, inject `IExchangeRateService`.
- **Views:** Thêm trang danh sách, lọc ngày, mới nhất, thống kê.
- **_Layout:** Có thể thêm menu link tới các trang tỷ giá.

---

## 6. Tóm tắt

| Nội dung | Quyết định (phù hợp source hiện tại) |
|----------|-------------------------------------|
| Vị trí project | Cả 4 project ở **root**, không dùng `src/` |
| Tên project | `PRN222-Assignment4-Option1`, `PRN222-Assignment4-Option1.DataAccess`, `.BusinessLogic`, `.Worker` |
| Core | Gộp trong DataAccess (Entities + Repository trong cùng project) |
| Worker | Project riêng, template Worker Service; class `Worker` (có thể đổi tên thành `ExchangeRateWorker` nếu muốn) |
| Solution | 4 project, references đã cấu hình |

Thiết kế này mô tả **đúng cấu trúc bạn đã tạo** và hướng triển khai tiếp theo (Entities, DbContext, Repositories, Services, Controller, Worker logic).
