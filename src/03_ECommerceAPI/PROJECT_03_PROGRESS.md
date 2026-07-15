# C# Backend Project 03: E-Commerce API (PostgreSQL & EF Core) - Progress & Context

Tài liệu này lưu trữ toàn bộ trạng thái hiện tại, cấu trúc code đã hoàn thành, và lộ trình chi tiết các bước còn lại của dự án để AI Tutor tiếp quản có thể hướng dẫn liên tục.

---

## 1. Vai trò của AI Tutor (Luật Mentor nghiêm khắc)
*   **Không code hộ:** Tuyệt đối không viết cả hàm, class hay cấu trúc hoàn chỉnh. Chỉ dùng mã giả hoặc gợi ý từ khóa/cú pháp.
*   **Không debug hộ:** Giải thích bản chất lỗi biên dịch/runtime, gợi ý từ khóa tìm kiếm hoặc đặt câu hỏi gợi mở để Developer tự dò lỗi.
*   **Tư duy sư phạm:** Khuyến khích Rich Domain Model (đóng gói logic nghiệp vụ vào Entity) và refactor mã nguồn từng bước khi dự án phát triển lớn hơn.

---

## 2. Tiến độ dự án hiện tại (Đã hoàn thành)

### A. Hạ tầng & Database:
*   Đã dựng **PostgreSQL** trong Docker. Cấu hình volume chuẩn `/var/lib/postgresql` (tránh lỗi xung đột của Postgres 18+).
*   Đã tạo thành công Migration `InitialCreate` và đồng bộ cấu trúc bảng xuống database.

### B. Cấu trúc Entity (Rich Domain Model):
*   `Product`: Có `ReduceStock(quantity)` bảo vệ tồn kho.
*   `Category`: Khai báo `IReadOnlyList<Product>` ngăn sửa đổi danh sách sản phẩm trực tiếp từ bên ngoài.
*   `User` & `Wallet`: Quan hệ 1-1, khởi tạo `Wallet` tự động trong constructor của `User` qua từ khóa `this`.
*   `Wallet`: Có `Deposit(amount)` và `Withdraw(amount)` quản lý số dư an toàn.
*   `Order` & `OrderItem`: Lưu đơn giá tại thời điểm mua (`Price`), có `AddItem` tự động tính lũy tiến tổng tiền `TotalPrice`.
*   `Transaction`: Ghi lại lịch sử biến động số dư ví (Nạp tiền/Thanh toán đơn hàng).

### C. Các API Endpoints đã viết (Minimal API trong `Program.cs`):
*   `POST /categories` & `GET /categories`
*   `POST /products` & `GET /products` (sử dụng `Include` để tải dữ liệu quan hệ).
*   `POST /users` & `GET /users/{id}` (dùng DTO ẩn password và chống lỗi vòng lặp tuần tự hóa JSON).
*   `PATCH /wallets/{id}/deposit`: Nạp tiền và lưu lịch sử `Transaction`.
*   `POST /orders`: Tạo đơn hàng chạy dưới một **ACID Database Transaction** (`BeginTransactionAsync`), xử lý đồng thời trừ kho, trừ tiền ví, lưu lịch sử giao dịch và rollback nếu lỗi.

---

## 3. Lộ trình các bước còn lại cần thực hiện

Dự án 03 sẽ đi qua các bước sau để hoàn thành trọn vẹn:

### 🏁 Bước 3.1: Tái hiện lỗi Race Condition & Over-selling (Hiện tại)
*   **Mục tiêu:** Chứng minh lỗi bán quá số lượng tồn kho khi nhiều yêu cầu mua hàng đồng thời chạm vào 1 sản phẩm cuối cùng.
*   **Thực hiện:** Viết endpoint `/test-concurrency` giả lập reset tồn kho = 1, ví tiền = 10,000, sau đó dùng `HttpClient` bắn đồng thời 10 request POST tới `/orders` bằng `Task.WhenAll`. Xem kết quả DB bị sai lệch (tồn kho giảm về 0 nhưng tạo được nhiều hơn 1 đơn hàng thành công).

### 🛡️ Bước 3.2: Triển khai giải pháp chống tranh chấp dữ liệu (Concurrency Control)
*   **Mục tiêu:** Bảo vệ tồn kho và ví tiền trước các request đồng thời.
*   **Thực hiện:** Hướng dẫn Developer tìm hiểu và thử nghiệm các cách giải quyết:
    *   *Cách 1:* **Database Atomic Updates** (trực tiếp chạy câu lệnh SQL trừ kho kèm điều kiện lọc: `WHERE Stock >= quantity`).
    *   *Cách 2:* **Optimistic Concurrency Control (Khóa lạc quan)**: Thêm cột `RowVersion`/`Version` vào entity `Product` và `Wallet`. Sử dụng thuộc tính `[ConcurrencyCheck]` hoặc cấu hình Fluent API trong EF Core. Bắt lỗi `DbUpdateConcurrencyException` để xử lý rollback.
    *   *Cách 3:* **Pessimistic Concurrency Control (Khóa bi quan)**: Sử dụng khóa dòng trong Database (ví dụ: `SELECT ... FOR UPDATE` trong PostgreSQL).
*   **Kiểm chứng:** Chạy lại endpoint `/test-concurrency` để chứng minh hệ thống đã an toàn (chỉ duy nhất 1 đơn hàng thành công, 9 đơn hàng còn lại báo hết hàng).

### 🏗️ Bước 3.3: Tái cấu trúc mã nguồn (Incremental Refactoring / Layering)
*   **Mục tiêu:** Tách rời các logic đang bị dồn hết vào file `Program.cs` để mã nguồn dễ bảo trì và mở rộng hơn.
*   **Thực hiện:**
    *   Tách các DTOs ra các file riêng biệt trong thư mục `DTOs/`.
    *   Tách các Endpoints ra các module hoặc extension methods riêng (Ví dụ: `ProductEndpoints`, `OrderEndpoints`) thay vì viết trực tiếp ở `Program.cs`.
    *   Giới thiệu khái niệm **Service Layer** (Tầng nghiệp vụ) để tách biệt logic xử lý (ví dụ: `OrderService`) ra khỏi tầng HTTP (API Endpoints).

### 🚦 Bước 3.4: Xây dựng Middleware xử lý lỗi tập trung (Global Exception Handling)
*   **Mục tiêu:** Loại bỏ các khối lệnh `try-catch` lặp đi lặp lại ở từng endpoint.
*   **Thực hiện:** Viết một **Custom Middleware** để bắt toàn bộ Exception chưa được xử lý trong ứng dụng, tự động Rollback Database Transaction nếu có, và trả về một cấu trúc lỗi JSON tiêu chuẩn cho client (ví dụ: HTTP 400 Bad Request kèm lý do).

### 🏁 Bước 3.5: Nghiệm thu & Chuyển giao sang Dự án 4
*   **Mục tiêu:** Tổng kết bài học và chuẩn bị cho dự án SignalR & Redis.
*   **Thực hiện:** Chạy test toàn bộ luồng hệ thống từ tạo User, nạp ví, tạo sản phẩm, mua hàng, kiểm tra lịch sử giao dịch và chuyển giao context.
