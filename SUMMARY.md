# Tổng hợp nội dung cuối cùng – Assignment 04 Option 1

## 1. Yêu cầu bài (Option 1)

- **Bỏ Desktop App** – chỉ làm Web + Worker Service.
- **Dùng API tỷ giá có sẵn** – không sinh tỷ giá ngẫu nhiên.

### 1.1. Worker Service

| Yêu cầu | Mô tả |
|--------|--------|
| Chu kỳ | Mỗi **5 giây** gọi API tỷ giá |
| Lưu trữ | Lưu vào database |
| Logging | Ghi log |
| Dừng | Hỗ trợ dừng bằng **CancellationToken** |

### 1.2. Web Application (MVC hoặc Razor Pages)

| Chức năng | Mô tả |
|-----------|--------|
| Danh sách | Hiển thị danh sách tỷ giá |
| Lọc | Lọc theo ngày |
| Mới nhất | Xem tỷ giá mới nhất |
| Thống kê | Xem thống kê trung bình |

---

## 2. Kiến trúc: 3-layer

| Layer | Project | Nội dung |
|-------|---------|----------|
| **Data Access (DAL)** | `PRN222-Assignment4-Option1.DataAccess` | Entities, DbContext, Repositories (interface + implementation) |
| **Business Logic (BLL)** | `PRN222-Assignment4-Option1.BusinessLogic` | Services (gọi API, xử lý nghiệp vụ, gọi Repository) |
| **Presentation** | `PRN222-Assignment4-Option1` (Web) | MVC – Controllers, Views, ViewModels |

**Worker** không phải một layer, mà là **một ứng dụng riêng** (project), dùng chung BLL + DAL.

---

## 3. Quyết định đã thống nhất

- **Core gộp chung với DataAccess**  
  Không tách project Core riêng. Entities và interface repository nằm trong `PRN222-Assignment4-Option1.DataAccess`.

- **Worker tách riêng, không gộp với BusinessLogic**  
  Worker là project độc lập, reference sang BusinessLogic (và qua BLL dùng DataAccess). Logic nghiệp vụ chỉ nằm trong BLL.

- **Cấu trúc thực tế**  
  Cả 4 project đặt ở **root** (không dùng thư mục `src/`), đặt tên `PRN222-Assignment4-Option1.*`. Chi tiết cấu trúc thư mục xem **DESIGN.md**.

---

## 4. Cấu trúc solution (phù hợp source hiện tại)

```
PRN222-Assignment4-Option1/
├── PRN222-Assignment4-Option1.sln
│
├── PRN222-Assignment4-Option1/                    # Web (Presentation)
│   ├── Controllers/, Views/, Models/, wwwroot/
│   └── PRN222-Assignment4-Option1.csproj
│
├── PRN222-Assignment4-Option1.DataAccess/          # DAL (Class Library)
│   ├── Entities/
│   │   └── ExchangeRate.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   ├── IExchangeRateRepository.cs
│   │   └── ExchangeRateRepository.cs
│   └── PRN222-Assignment4-Option1.DataAccess.csproj
│
├── PRN222-Assignment4-Option1.BusinessLogic/       # BLL (Class Library)
│   ├── Services/
│   │   ├── IExchangeRateService.cs
│   │   ├── ExchangeRateService.cs
│   │   └── ExchangeRateApiService.cs
│   └── PRN222-Assignment4-Option1.BusinessLogic.csproj
│
├── PRN222-Assignment4-Option1.Worker/             # Worker Service
│   ├── Worker.cs
│   ├── Program.cs
│   └── PRN222-Assignment4-Option1.Worker.csproj
│
├── doc.txt
├── DESIGN.md
└── SUMMARY.md
```

---

## 5. Luồng tham chiếu (Dependencies)

```
PRN222-Assignment4-Option1 (Web)   ──►  PRN222-Assignment4-Option1.BusinessLogic  ──►  PRN222-Assignment4-Option1.DataAccess
PRN222-Assignment4-Option1.Worker  ──►  PRN222-Assignment4-Option1.BusinessLogic  ──►  PRN222-Assignment4-Option1.DataAccess
```

- **DataAccess**: không reference project nào trong solution (chỉ NuGet: EF Core, v.v.).
- **BusinessLogic**: reference **DataAccess**.
- **Web**: reference **BusinessLogic**.
- **Worker**: reference **BusinessLogic**.

---

## 6. Trách nhiệm từng project

### 6.1. PRN222-Assignment4-Option1.DataAccess (DAL)

- Entity `ExchangeRate`.
- `AppDbContext`, cấu hình EF Core, migrations.
- `IExchangeRateRepository`, `ExchangeRateRepository`: CRUD, lấy theo ngày, mới nhất, phục vụ thống kê.

### 6.2. PRN222-Assignment4-Option1.BusinessLogic (BLL)

- **ExchangeRateApiService**: gọi API tỷ giá bên ngoài, trả về dữ liệu (DTO/model).
- **IExchangeRateService / ExchangeRateService**:
  - Lấy danh sách tỷ giá (có lọc ngày).
  - Lấy tỷ giá mới nhất.
  - Thống kê trung bình.
  - Method cho Worker: fetch từ API + lưu qua Repository (ví dụ `FetchAndSaveLatestRatesAsync`).

### 6.3. PRN222-Assignment4-Option1 (Web – Presentation)

- Controllers chỉ gọi `IExchangeRateService`.
- ViewModels, Views: danh sách, lọc ngày, mới nhất, thống kê.

### 6.4. PRN222-Assignment4-Option1.Worker

- `Worker : BackgroundService` (hoặc đổi tên thành `ExchangeRateWorker`):
  - Inject service BLL (ví dụ `IExchangeRateService` hoặc service chuyên sync từ API).
  - Trong `ExecuteAsync`: mỗi 5 giây gọi fetch + lưu; dùng `CancellationToken`; ghi log.
- `Program.cs`: cấu hình Host, đăng ký DI (DbContext, Repository, Services, HttpClient cho API).

---

## 7. Checklist triển khai

- [x] Tạo solution với 4 project: DataAccess, BusinessLogic, Web, Worker.
- [x] Cấu hình project references (Web → BLL, Worker → BLL, BLL → DAL).
- [ ] DataAccess: Entity, DbContext, Repository + interface, migrations.
- [ ] BusinessLogic: API client service, ExchangeRateService (interface + impl), đăng ký DI (nếu có extension).
- [ ] Web: ExchangeRateController, Views, ViewModels; đăng ký DbContext + BLL services.
- [ ] Worker: BackgroundService 5 giây, gọi BLL, CancellationToken, logging; đăng ký DbContext + BLL.
- [ ] Cấu hình: connection string, API URL/Key (appsettings), interval 5 giây.
- [ ] Test: Worker chạy độc lập, Web hiển thị đúng danh sách / lọc ngày / mới nhất / thống kê.

---

*Tài liệu tổng hợp: 3-layer, Core gộp DataAccess, Worker tách riêng; cấu trúc và tên project phù hợp source hiện tại (PRN222-Assignment4-Option1.*, các project ở root).*
