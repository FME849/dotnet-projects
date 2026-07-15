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

### 🗄️ Dự án 3: E-Commerce Catalog & Orders (Database & ORM)
*   **Mục tiêu chính:** Kết nối với Database thực tế thông qua ORM chuẩn của .NET: **Entity Framework Core (EF Core)**. Học cách thiết kế DB quan hệ và xử lý migrations.
*   **Yêu cầu tính năng:**
    *   Thiết kế database gồm 3 bảng quan hệ: `Product` (Sản phẩm), `Category` (Danh mục - 1-n với Product), và `Order` + `OrderItem` (Đơn hàng - n-n giữa Product và Order).
    *   Sử dụng SQLite cho gọn nhẹ, dễ cài đặt.
    *   Tạo API CRUD cho Product/Category và API tạo Order (kèm tính toán tổng tiền).
    *   Sử dụng migrations để quản lý phiên bản database.
*   **Kiến thức cần đạt:**
    *   EF Core DbContext, DbSet.
    *   Cấu hình quan hệ: One-to-Many, Many-to-Many qua Fluent API hoặc Data Annotations.
    *   LINQ-to-Entities (cách EF Core chuyển code C# thành câu lệnh SQL).
    *   Eager Loading (`Include`), Lazy Loading, và Explicit Loading.

### ⚡ Dự án 4: Real-time Price Monitor (Caching, Worker & WebSockets)
*   **Mục tiêu chính:** Làm quen với các công nghệ tối ưu hóa hiệu năng và giao tiếp thời gian thực: **Redis Caching**, **SignalR (WebSockets)**, và **Hosted Services** (Background Worker).
*   **Yêu cầu tính năng:**
    *   Tạo một Background Worker (`IHostedService` hoặc `BackgroundService`) chạy ngầm, cứ mỗi 3 giây sẽ sinh ra giá giả lập của các đồng Coin (BTC, ETH, SOL).
    *   Mỗi khi có giá mới, worker ghi đè giá mới nhất này vào **Redis Cache** để lưu trữ tạm thời tốc độ cao.
    *   Đồng thời, worker phát (broadcast) giá mới này tới toàn bộ Client đang kết nối thông qua **SignalR Hub**.
    *   Xây dựng 1 trang HTML/JS đơn giản (Frontend) kết nối vào SignalR để hiển thị biểu đồ hoặc bảng giá cập nhật real-time không cần reload.
*   **Kiến thức cần đạt:**
    *   `BackgroundService` trong .NET.
    *   Sử dụng Redis với `IDistributedCache` hoặc `StackExchange.Redis`.
    *   ASP.NET Core SignalR (Hubs, Clients, Connections).

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
