# Exchange Rate Monitoring System (Assignment 04 Option 1)

Hệ thống theo dõi tỷ giá ngoại tệ: Worker Service gọi API HexaRate mỗi 5 giây và lưu vào DB; Web MVC hiển thị danh sách, lọc theo ngày, tỷ giá mới nhất và thống kê trung bình.

## Yêu cầu

- .NET 9 SDK
- SQL Server (LocalDB đi kèm Visual Studio hoặc SQL Server Express/full)
- API tỷ giá: [HexaRate](https://hexarate.paikama.co/) (không cần API key)

## Cấu trúc solution

- **PRN222-Assignment4-Option1** – Web MVC (Presentation)
- **PRN222-Assignment4-Option1.BusinessLogic** – BLL (Services, API client)
- **PRN222-Assignment4-Option1.DataAccess** – DAL (Entity, DbContext, Repository)
- **PRN222-Assignment4-Option1.Worker** – Worker Service (BackgroundService 5s)

## Chạy ứng dụng

**Chạy từ thư mục solution** (Web và Worker dùng chung database SQL Server `ExchangeRateDb`):

```bash
cd PRN222-Assignment4-Option1
```

### 1. Tạo database (chỉ cần một lần)

```bash
dotnet ef database update --project PRN222-Assignment4-Option1.DataAccess --startup-project PRN222-Assignment4-Option1
```

### 2. Chạy Worker (terminal 1)

```bash
dotnet run --project PRN222-Assignment4-Option1.Worker
```

Worker sẽ gọi API USD/VND mỗi 5 giây và lưu vào DB. Dừng bằng Ctrl+C.

### 3. Chạy Web (terminal 2)

```bash
dotnet run --project PRN222-Assignment4-Option1
```

Mở trình duyệt theo URL hiển thị (ví dụ https://localhost:7xxx), dùng menu:

- **Danh sách tỷ giá** – danh sách, lọc theo ngày, phân trang
- **Tỷ giá mới nhất** – bản ghi mới nhất
- **Thống kê** – trung bình tỷ giá theo ngày

## Cấu hình

- **Web:** `PRN222-Assignment4-Option1/appsettings.json` – `ConnectionStrings:DefaultConnection`
- **Worker:** `PRN222-Assignment4-Option1.Worker/appsettings.json` – Connection string, `ExchangeRate:BaseCurrency`, `TargetCurrency`, `IntervalSeconds` (5)

Connection string mặc định: `Server=.;Database=ExchangeRateDb;Trusted_Connection=True;TrustServerCertificate=True;` (Windows Authentication, server local). Có thể đổi sang SQL Server Authentication hoặc server khác trong appsettings.
