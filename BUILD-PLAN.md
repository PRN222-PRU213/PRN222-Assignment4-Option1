# Kế hoạch Step-by-Step – Build Source Code Hoàn Chỉnh

Dựa trên **doc.txt** (requirements Option 1), **SUMMARY.md**, **DESIGN.md** và cấu trúc solution hiện tại. API tỷ giá: **HexaRate** (https://hexarate.paikama.co/).

---

## Tổng quan thứ tự thực hiện

| Phase | Nội dung                                 | Project       |
| ----- | ---------------------------------------- | ------------- |
| **1** | DAL: Entity, DbContext, Repository       | DataAccess    |
| **2** | BLL: API client, Service, interface      | BusinessLogic |
| **3** | Worker: BackgroundService 5s, DI, config | Worker        |
| **4** | Web: Controller, Views, DI               | Web           |
| **5** | Cấu hình chung, test E2E                 | All           |

---

## Phase 1 – Data Access Layer (DAL)

### Step 1.1 – Thêm EF Core vào DataAccess

- Mở `PRN222-Assignment4-Option1.DataAccess.csproj`.
- Thêm package:
  - `Microsoft.EntityFrameworkCore.SqlServer` (hoặc `Microsoft.EntityFrameworkCore.Sqlite` nếu dùng SQLite).
  - `Microsoft.EntityFrameworkCore.Design` (để chạy migrations từ Web hoặc Worker).

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Step 1.2 – Tạo Entity ExchangeRate

- Tạo thư mục `Entities` trong project DataAccess.
- Tạo file `Entities/ExchangeRate.cs` với các property phù hợp response HexaRate:
  - `Id` (int, key, identity)
  - `BaseCurrency` (string, e.g. "USD")
  - `TargetCurrency` (string, e.g. "VND")
  - `Rate` (decimal)
  - `Timestamp` (DateTime hoặc DateTimeOffset)
  - Có thể thêm `CreatedAt` (DateTime) để lọc theo ngày dễ hơn.

### Step 1.3 – Tạo DbContext

- Tạo thư mục `Data` trong project DataAccess.
- Tạo `Data/AppDbContext.cs` kế thừa `DbContext`, có `DbSet<ExchangeRate>`, cấu hình entity (key, length, index nếu cần).

### Step 1.4 – Tạo Repository interface và implementation

- Tạo thư mục `Repositories`.
- `Repositories/IExchangeRateRepository.cs`: định nghĩa methods:
  - `Task AddAsync(ExchangeRate entity, CancellationToken ct = default)`
  - `Task<List<ExchangeRate>> GetByDateAsync(DateTime date, CancellationToken ct = default)`
  - `Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default)` (theo Base/Target hoặc mới nhất theo Id/Timestamp)
  - `Task<List<ExchangeRate>> GetAllAsync(int skip, int take, CancellationToken ct = default)` (phân trang)
  - Method cho thống kê: ví dụ `Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency, string? targetCurrency, CancellationToken ct = default)` hoặc trả về list theo ngày.
- `Repositories/ExchangeRateRepository.cs`: implement interface, inject `AppDbContext`, dùng LINQ.

### Step 1.5 – Xóa Class1.cs trong DataAccess

- Xóa file `Class1.cs` trong project DataAccess (không dùng nữa).

### Step 1.6 – Migration (chạy từ Web project)

- Trong Web project thêm package `Microsoft.EntityFrameworkCore.Design` (nếu chưa có) và reference DataAccess (đã có qua BusinessLogic).
- Chạy từ thư mục solution:
  - `dotnet ef migrations add InitialCreate --project PRN222-Assignment4-Option1.DataAccess --startup-project PRN222-Assignment4-Option1`
- Cập nhật database:
  - `dotnet ef database update --project PRN222-Assignment4-Option1.DataAccess --startup-project PRN222-Assignment4-Option1`
- (Connection string đặt trong `appsettings.json` của Web; Worker cũng cần connection string riêng trong `appsettings.json`.)

---

## Phase 2 – Business Logic Layer (BLL)

### Step 2.1 – Thêm package HttpClient / JSON (nếu cần)

- BusinessLogic là class library; dùng `HttpClient` và `System.Text.Json` (có sẵn). Không bắt buộc thêm package nếu chỉ dùng .NET 9.

### Step 2.2 – DTO cho API HexaRate

- Tạo folder `Models` hoặc `Dtos` trong BusinessLogic (optional).
- Tạo class map response HexaRate, ví dụ:
  - `HexaRateResponse` với `status_code`, `data` (object có `base`, `target`, `mid`, `unit`, `timestamp`).

### Step 2.3 – ExchangeRateApiService (gọi API)

- Tạo `Services/ExchangeRateApiService.cs` (hoặc `Services/ExchangeRateApiClient.cs`).
- Inject `HttpClient` (đăng ký named hoặc typed từ Host).
- Method: `Task<HexaRateResponse?> GetLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default)`.
- URL: `https://hexarate.paikama.co/api/rates/{base}/{target}/latest`.
- Dùng `HttpClient.GetAsync`, đọc JSON, deserialize sang DTO.

### Step 2.4 – IExchangeRateService và ExchangeRateService

- `Services/IExchangeRateService.cs`:
  - `Task<List<ExchangeRate>> GetListAsync(DateTime? date, int skip, int take, CancellationToken ct = default)`
  - `Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default)`
  - `Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency, string? targetCurrency, CancellationToken ct = default)` (hoặc trả về model thống kê)
  - `Task FetchAndSaveLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default)` — cho Worker: gọi API → map sang Entity → gọi Repository.AddAsync.
- `Services/ExchangeRateService.cs`: inject `IExchangeRateRepository` và `ExchangeRateApiService` (hoặc `IHttpClientFactory` nếu gọi API trong đây). Implement các method trên.

### Step 2.5 – Extension đăng ký DI (optional nhưng nên có)

- Tạo `Extensions/ServiceCollectionExtensions.cs` (hoặc trong folder Services).
- Method: `AddExchangeRateServices(this IServiceCollection services, IConfiguration config)`:
  - Đăng ký `AppDbContext` với connection string từ config.
  - Đăng ký `IExchangeRateRepository`, `ExchangeRateRepository`.
  - Đăng ký `IExchangeRateService`, `ExchangeRateService`.
  - Đăng ký `HttpClient` cho HexaRate (BaseAddress = https://hexarate.paikama.co) và/hoặc đăng ký `ExchangeRateApiService`.

### Step 2.6 – Xóa Class1.cs trong BusinessLogic

- Xóa `Class1.cs` trong project BusinessLogic.

---

## Phase 3 – Worker Service

### Step 3.1 – Thêm reference và package cần thiết

- Worker đã reference BusinessLogic. Đảm bảo Worker có thể gọi BLL và BLL đăng ký DbContext (cần thêm package EF vào Worker để có thể chạy migration từ Worker nếu muốn; thường chỉ cần config connection string).

### Step 3.2 – appsettings.json trong Worker

- Tạo/cập nhật `appsettings.json` trong project Worker:
  - `ConnectionStrings:DefaultConnection` (giống Web để cùng DB).
  - Section tùy chọn: `ExchangeRate:BaseCurrency`, `ExchangeRate:TargetCurrency`, `ExchangeRate:IntervalSeconds` (5).

### Step 3.3 – Cập nhật Worker.cs

- Inject `IExchangeRateService` và `ILogger`.
- Trong `ExecuteAsync`: vòng lặp `while (!stoppingToken.IsCancellationRequested)`:
  - Gọi `await _exchangeRateService.FetchAndSaveLatestRateAsync("USD", "VND", stoppingToken)`.
  - Log thông tin (đã lấy tỷ giá, đã lưu, hoặc lỗi).
  - `await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken)` (hoặc đọc từ config).

### Step 3.4 – Program.cs Worker – Đăng ký DI

- Gọi extension từ BLL: `builder.Services.AddExchangeRateServices(builder.Configuration);` (hoặc đăng ký từng service + DbContext + HttpClient).
- Đảm bảo `AddHostedService<Worker>()` vẫn có.
- Cấu hình `DbContext` dùng connection string từ `builder.Configuration`.

---

## Phase 4 – Web Application

### Step 4.1 – appsettings.json (Web)

- Thêm `ConnectionStrings:DefaultConnection` trỏ tới SQL Server hoặc SQLite (trùng với Worker để cùng DB).

### Step 4.2 – Program.cs (Web) – Đăng ký DI

- Gọi `AddExchangeRateServices(builder.Configuration)` (extension từ BLL) hoặc đăng ký DbContext, Repository, IExchangeRateService thủ công.

### Step 4.3 – ExchangeRateController

- Tạo `Controllers/ExchangeRateController.cs`.
- Inject `IExchangeRateService`.
- Actions:
  - **Index (GET):** danh sách tỷ giá (có phân trang). Query: `date` (optional), `page`, `pageSize`. Gọi service.GetListAsync(date, skip, take). Trả về View với model (list + filter).
  - **Latest (GET):** xem tỷ giá mới nhất. Gọi service.GetLatestAsync(). Trả về View.
  - **Statistics (GET):** thống kê trung bình. Query: `date`. Gọi service.GetAverageRateByDateAsync(date, null, null) hoặc theo base/target. Trả về View với model thống kê.

### Step 4.4 – ViewModels (nếu dùng)

- Có thể tạo ViewModels cho list (List&lt;ExchangeRate&gt; + Date filter + Paging), Latest (ExchangeRate), Statistics (Average, Min, Max, Count…).

### Step 4.5 – Views cho ExchangeRate

- Tạo folder `Views/ExchangeRate/`.
- **Index.cshtml:** hiển thị bảng danh sách; form/query lọc theo ngày; phân trang.
- **Latest.cshtml:** hiển thị 1 bản ghi mới nhất (Base, Target, Rate, Time).
- **Statistics.cshtml:** hiển thị thống kê (theo ngày): trung bình, có thể thêm min/max, số bản ghi.

### Step 4.6 – \_Layout.cshtml – Menu

- Thêm link đến ExchangeRate: Index (Danh sách), Latest (Mới nhất), Statistics (Thống kê).

### Step 4.7 – Route (mặc định)

- Có thể giữ default route; hoặc thêm route cho ExchangeRate nếu cần.

---

## Phase 5 – Cấu hình và kiểm tra

### Step 5.1 – Connection string thống nhất

- Web và Worker dùng cùng connection string (cùng DB) để Worker ghi, Web đọc.

### Step 5.2 – Chạy migration (một lần)

- Đã nêu ở Step 1.6; đảm bảo DB đã tạo và bảng ExchangeRate tồn tại.

### Step 5.3 – Test Worker

- Chạy Worker project: `dotnet run --project PRN222-Assignment4-Option1.Worker`.
- Kiểm tra log: mỗi 5 giây có log gọi API và lưu DB; dừng bằng Ctrl+C (CancellationToken).
- Kiểm tra DB: có bản ghi mới.

### Step 5.4 – Test Web

- Chạy Web: `dotnet run --project PRN222-Assignment4-Option1`.
- Mở trình duyệt: Danh sách (có lọc ngày), Mới nhất, Thống kê — dữ liệu khớp với DB.

### Step 5.5 – Test kết hợp

- Chạy Worker và Web cùng lúc (2 terminal). Worker ghi liên tục; Web refresh trang danh sách / mới nhất thấy dữ liệu cập nhật.

---

## Checklist tổng hợp

- [x] **Phase 1:** Entity, DbContext, Repository, Migration, xóa Class1 (DataAccess).
- [x] **Phase 2:** DTO HexaRate, ExchangeRateApiService, IExchangeRateService, ExchangeRateService, Extension DI, xóa Class1 (BusinessLogic).
- [x] **Phase 3:** Worker appsettings, Worker.cs (5s, FetchAndSave, log, CancellationToken), Program.cs DI.
- [x] **Phase 4:** Web appsettings, Program.cs DI, ExchangeRateController (Index, Latest, Statistics), Views, \_Layout menu.
- [x] **Phase 5:** Connection string thống nhất, migration đã chạy, README hướng dẫn chạy Worker + Web, DTO HexaRate có JsonPropertyName.

---

## Ghi chú kỹ thuật

- **HexaRate API:** `GET https://hexarate.paikama.co/api/rates/USD/VND/latest` — không cần API key.
- **DbContext:** Nên đăng ký scope: `AddDbContext<AppDbContext>(...)`.
- **HttpClient:** Nên đăng ký typed hoặc named client cho HexaRate để tránh socket exhaustion.
- **CancellationToken:** Worker luôn truyền `stoppingToken` vào các async method khi có thể.

Khi hoàn thành từng phase, đánh dấu checklist và build lại solution để tránh lỗi tích lũy.
