# C# Backend Learning Roadmap for Frontend Developers

Tài liệu này lưu trữ lộ trình học C# Backend thông qua 4 dự án nhỏ (Micro-Projects). Mỗi dự án tập trung vào một nhóm kiến thức cốt lõi để bạn tích lũy dần dần mà không bị ngợp.

---

## Lộ trình 4 Dự án Micro-Projects

### 📦 Dự án 1: CLI Log Analyzer (Công cụ phân tích Log)
*   **Mục tiêu chính:** Làm quen với .NET CLI, cú pháp C# Core bên ngoài Unity, NuGet package manager, đọc/ghi file bất đồng bộ (`async/await`), và đặc biệt là **LINQ**.
*   **Yêu cầu tính năng:**
    *   Đọc một file log dạng text (VD: `app.log`).
    *   Sử dụng regex hoặc string manipulation để phân tách cấu trúc dòng log (Timestamp, Level: INFO/WARN/ERROR, Message).
    *   Sử dụng LINQ để lọc ra các log có Level = ERROR.
    *   Thống kê số lượng log theo từng loại lỗi.
    *   Xuất báo cáo kết quả ra file `report.json`.
*   **Kiến thức cần đạt:**
    *   `dotnet new console`
    *   Kiểu dữ liệu generic `List<T>`, `Dictionary<TKey, TValue>`.
    *   LINQ: `Where`, `Select`, `GroupBy`, `OrderBy`, `Count`.
    *   Bất đồng bộ: `Task`, `async`, `await`, `File.ReadAllLinesAsync`.

### 🌐 Dự án 2: REST API Task Manager (In-Memory)
*   **Mục tiêu chính:** Chuyển từ Console App lên Web. Hiểu cách ASP.NET Core nhận và xử lý HTTP Request qua **Minimal APIs**, cơ chế **Dependency Injection (DI)**, và **Middleware**.
*   **Yêu cầu tính năng:**
    *   Xây dựng REST API đầy đủ CRUD cho Task (Id, Title, Description, IsCompleted, CreatedAt).
    *   Lưu trữ tạm thời trong bộ nhớ (In-Memory Service).
    *   Viết Custom Middleware để log thời gian xử lý của mỗi request (Request Timing Middleware).
    *   Thêm cơ chế xác thực đơn giản bằng API Key thông qua Custom Middleware hoặc Endpoint Filter.
*   **Kiến thức cần đạt:**
    *   `dotnet new web`
    *   Minimal API routing & Parameter Binding (FromQuery, FromRoute, FromBody).
    *   DI Lifetime: `Transient`, `Scoped`, `Singleton`.
    *   Cấu trúc của HTTP Pipeline và Middleware.

### 🗄️ Dự án 3: E-Commerce Catalog & Orders (Database & ORM & Concurrency & Deploy)
*   **Mục tiêu chính:** Kết nối với Database thực tế qua **EF Core**, giải quyết vấn đề tranh chấp dữ liệu đồng thời (**Concurrency Control**), tái cấu trúc dự án chuẩn nghiệp vụ (**Service Layer** & **Global Exception**), và triển khai **CI/CD** lên **Azure**.
*   **Yêu cầu tính năng:**
    *   Thiết kế database gồm các bảng: `Product`, `Category`, `User`, `Wallet`, `Order`, `OrderItem`, `Transaction` (quan hệ 1-n, 1-1, n-n).
    *   Sử dụng **PostgreSQL** chạy trong Docker container làm database chính.
    *   Xây dựng API CRUD cho Product/Category và API tạo Order bọc trong ACID Transaction.
    *   Tái hiện lỗi Race Condition/Over-selling và giải quyết bằng **Optimistic Concurrency Control (OCC)** sử dụng cột ẩn hệ thống `xmin` của Postgres kết hợp **Retry với Jitter (10-50ms)**.
    *   Tái cấu trúc: Tách API Endpoints thành các Extension Methods, áp dụng **Service Layer** (`IOrderService`), và viết **Custom Middleware** xử lý lỗi tập trung.
    *   **CI/CD & Cloud (Nâng cao):** 
        *   Tạo GitHub Actions CI chạy build, linter (`dotnet format`) và tests.
        *   Tự động build Docker Image đẩy lên **Azure Container Registry (ACR)** và deploy lên **Azure App Service**.
        *   Sử dụng **Azure Database for PostgreSQL (Flexible Server)** và quản lý cấu hình bảo mật bằng **Azure Key Vault**.
*   **Kiến thức cần đạt:**
    *   EF Core DbContext, DbSet, Migrations.
    *   Đóng gói logic nghiệp vụ vào Entity (Rich Domain Model).
    *   Giải quyết Race Condition (Khóa lạc quan OCC / Khóa bi quan).
    *   Tư duy thiết kế Layered Architecture (API Layer - Service Layer - Data Layer).
    *   Thiết lập GitHub Actions CI/CD Pipeline và triển khai tài nguyên cơ bản trên Microsoft Azure.

### ⚡ Dự án 4: Real-time Price Monitor (Caching, Worker, WebSockets & Azure Integration)
*   **Mục tiêu chính:** Làm quen với các công nghệ tối ưu hóa hiệu năng và giao tiếp thời gian thực: **Redis Caching**, **SignalR (WebSockets)**, **Hosted Services** (Background Worker) và tích hợp các dịch vụ Cloud chuyên dụng của Azure.
*   **Yêu cầu tính năng:**
    *   Tạo một Background Worker (`BackgroundService`) chạy ngầm giả lập sinh giá Coin mới (BTC, ETH, SOL) sau mỗi 3 giây.
    *   Ghi đè giá mới nhất này vào **Redis Cache** tốc độ cao.
    *   Broadcast giá mới thời gian thực tới toàn bộ client kết nối qua **SignalR Hub**.
    *   Xây dựng Frontend HTML/JS kết nối SignalR hiển thị bảng giá cập nhật không reload.
    *   **CI/CD & Cloud (Nâng cao):**
        *   Chuyển đổi Redis local sang **Azure Cache for Redis**.
        *   Tích hợp **Azure SignalR Service** làm Backplane để hỗ trợ scale-out (chạy nhiều instance API mà không mất kết nối WebSocket).
        *   Triển khai Background Worker lên **Azure Container Apps** để chạy độc lập.
*   **Kiến thức cần đạt:**
    *   `BackgroundService` trong .NET.
    *   Sử dụng Redis với `IDistributedCache` hoặc `StackExchange.Redis`.
    *   ASP.NET Core SignalR (Hubs, Clients, Connections) và tích hợp Azure SignalR SDK.
    *   Kiến trúc ứng dụng phân tán trên Cloud.

---

## Hướng dẫn Copy-Paste Prompt cho AI ở các Chat/Project khác

Khi bạn bắt đầu một chat mới cho một dự án cụ thể, hãy sao chép mẫu Prompt tương ứng dưới đây để thiết lập vai trò cho AI.

### 📝 Prompt chung cho vai trò Mentor
> Xem chi tiết tại luật dự án: [AGENTS.md](file:///e:/Tuan%20Metacraft/dotnet/.agents/AGENTS.md)

### 🚀 Prompt khởi động riêng cho từng dự án

#### Mẫu cho Dự án 1 (CLI Log Analyzer):
```markdown
Chúng ta sẽ bắt đầu dự án "Dự án 1: CLI Log Analyzer". 
Yêu cầu của dự án: Đọc file log text, dùng LINQ phân tích, đếm lỗi theo Level và xuất ra JSON.
Nhiệm vụ của bạn (Mentor): 
- Tuyệt đối KHÔNG viết code hộ tôi, chỉ hướng dẫn bằng câu hỏi gợi ý hoặc từ khóa.
- Giúp tôi định hình cấu trúc chương trình.
- Review code tôi viết và chỉ ra các điểm chưa tốt (naming, async/await, LINQ tối ưu chưa).
Bây giờ, hãy đặt cho tôi câu hỏi đầu tiên để thiết lập cấu trúc thư mục và file của project C# Console này.
```

#### Mẫu cho Dự án 2 (REST API Task Manager):
```markdown
Chúng ta sẽ bắt đầu "Dự án 2: REST API Task Manager (In-Memory)".
Yêu cầu dự án: Tạo web API CRUD Task, học về Dependency Injection (DI) và Middleware trong ASP.NET Core.
Nhiệm vụ của bạn (Mentor):
- Giải thích các khái niệm DI (Transient/Scoped/Singleton) và Middleware bằng hình ảnh/mã giả khi tôi hỏi.
- Đặt câu hỏi để tôi tự thiết kế các class, interfaces phục vụ cho DI.
- KHÔNG viết code API cho tôi.
Bây giờ, hãy đặt câu hỏi để tôi bắt đầu khởi tạo dự án web API từ .NET CLI và giải thích sự khác biệt cơ bản giữa cấu trúc Console App (Dự án 1) và Web API.
```

#### Mẫu cho Dự án 3 (E-Commerce Catalog & EF Core):
```markdown
Chúng ta sẽ bắt đầu "Dự án 3: E-Commerce Catalog & Orders (EF Core)".
Yêu cầu dự án: Thiết kế database SQLite, dùng EF Core để CRUD và quản lý quan hệ 1-n, n-n.
Nhiệm vụ của bạn (Mentor):
- Hướng dẫn tôi tư duy thiết kế các Entity (Class đại diện cho bảng) và cấu hình DbContext.
- Giải thích cách hoạt động của Migrations.
- Chỉ ra các lỗi N+1 query hoặc loading sai cách khi tôi thực hiện truy vấn DB. KHÔNG viết code truy vấn hộ.
Bây giờ, hãy hỏi tôi về sơ đồ thực thể (Entities) và mối quan hệ giữa Product, Category, Order để tôi tự phác thảo code.
```

#### Mẫu cho Dự án 4 (Real-time Price Monitor):
```markdown
Chúng ta sẽ bắt đầu "Dự án 4: Real-time Price Monitor (SignalR & Redis)".
Yêu cầu dự án: Dùng Background Worker để cập nhật giá, cache vào Redis và push real-time qua SignalR.
Nhiệm vụ của bạn (Mentor):
- Hướng dẫn tôi cấu trúc của một Background Service.
- Gợi ý cách tích hợp thư viện Redis và SignalR vào ứng dụng.
- Giúp tôi debug luồng kết nối và luồng cache dữ liệu thông qua các câu hỏi truy vấn logic. KHÔNG code hộ.
Bây giờ, hãy đặt câu hỏi đầu tiên để tôi hình dung được luồng đi của dữ liệu từ Worker -> Redis & SignalR.
```
