# C# Backend Project 03: E-Commerce API (PostgreSQL & EF Core) - Progress & Context

Tài liệu này lưu trữ toàn bộ trạng thái hiện tại, cấu trúc code đã hoàn thành, và lộ trình chi tiết các bước còn lại của dự án để AI Tutor tiếp quản có thể hướng dẫn liên tục.

---

## 1. Vai trò của AI Tutor (Luật Mentor nghiêm khắc)
*   **Không code hộ:** Tuyệt đối không viết cả hàm, class hay cấu trúc hoàn chỉnh (trừ trường hợp Developer yêu cầu cụ thể như "hãy code hộ mình..."). Chỉ dùng mã giả hoặc gợi ý từ khóa/cú pháp.
*   **Không debug hộ:** Giải thích bản chất lỗi biên dịch/runtime, gợi ý từ khóa tìm kiếm hoặc đặt câu hỏi gợi mở để Developer tự dò lỗi.
*   **Tư duy sư phạm:** Khuyến khích Rich Domain Model (đóng gói logic nghiệp vụ vào Entity) và refactor mã nguồn từng bước khi dự án phát triển lớn hơn.

---

## 2. Tiến độ dự án hiện tại (Đã hoàn thành)

### A. Hạ tầng & Database:
*   Đã dựng **PostgreSQL** trong Docker. Cấu hình volume chuẩn `/var/lib/postgresql`.
*   Đã tạo thành công Migration `InitialCreate` và đồng bộ cấu trúc bảng xuống database.
*   Đã cập nhật migration `AddProductVersion` để hỗ trợ cột concurrency token hệ thống `xmin` trên Postgres.

### B. Cấu trúc Entity (Rich Domain Model):
*   `Product`: Có `ReduceStock(quantity)` bảo vệ tồn kho và thuộc tính `Version` kiểu `uint` để chống tranh chấp.
*   `Category`: Khai báo `IReadOnlyList<Product>` ngăn sửa đổi danh sách sản phẩm trực tiếp từ bên ngoài.
*   `User` & `Wallet`: Quan hệ 1-1, khởi tạo `Wallet` tự động trong constructor của `User`.
*   `Wallet`: Có `Deposit(amount)` và `Withdraw(amount)` quản lý số dư an toàn.
*   `Order` & `OrderItem`: Lưu đơn giá tại thời điểm mua (`Price`), có `AddItem` tự động tính lũy tiến tổng tiền `TotalPrice`.
*   `Transaction`: Ghi lại lịch sử biến động số dư ví (Nạp tiền/Thanh toán đơn hàng).

### C. Concurrency Control (OCC) & Change Tracker Management:
*   Đã viết thành công endpoint `/test-concurrency` bắn 10 request mua hàng song song qua `Task.WhenAll`.
*   Triển khai thành công **Optimistic Concurrency Control (OCC)**. Bắt lỗi `DbUpdateConcurrencyException`, tự động Rollback và Retry với delay ngẫu nhiên (Jitter 10-50ms) tối đa 3 lần.
*   Giải quyết triệt để lỗi Change Tracker bị "bẩn" (Dirty State) của EF Core khi retry bằng phương thức `_dbContext.ChangeTracker.Clear()`.

### D. Kiến trúc Layered & Refactoring:
*   **Tách API Layer:** Toàn bộ Minimal API được di chuyển ra khỏi `Program.cs` vào các file static class trong thư mục `Endpoints/` (`CategoryEndpoints`, `ProductEndpoints`, `UserEndpoints`, `OrderEndpoints`).
*   **Sửa lỗi Circular Reference:** Sử dụng DTO phẳng (`ResponseProductDto`, `ResponseOrderItemDto`) thay vì trả về trực tiếp Entity Model để tránh vòng lặp tuần tự hóa JSON (lỗi 500).
*   **Service Layer:** Tách toàn bộ logic nghiệp vụ đặt hàng (bao gồm transaction, check kho/ví và retry OCC) từ Endpoint sang `OrderService` (thư mục `Services/`) thông qua interface `IOrderService`.
*   **Global Exception Handling:** Loại bỏ hoàn toàn khối `try-catch` lặp lại tại các Endpoint. Triển khai xử lý lỗi tập trung bằng interface `IExceptionHandler` (.NET 8+) trả về định dạng lỗi tiêu chuẩn RFC 7807 (ProblemDetails).

---

## 3. Lộ trình các bước còn lại cần thực hiện

Dự án 03 sẽ đi qua các bước sau để hoàn thành trọn vẹn:

### ☁️ Bước 3.5: CI/CD & Deploy Cloud (Nâng cao - Hiện tại)
*   **Mục tiêu:** Đóng gói và đưa ứng dụng lên hạ tầng Cloud thực tế.
*   **Thực hiện:**
    *   [ ] Thiết lập GitHub Actions Workflow để chạy build, linter (`dotnet format`).
    *   [ ] Build Docker Image và đẩy lên **Azure Container Registry (ACR)**.
    *   [ ] Deploy Web API lên **Azure App Service** (hoặc App Service chạy Docker).
    *   [ ] Tạo database **Azure Database for PostgreSQL (Flexible Server)** và cấu hình Key Vault bảo mật kết nối.

### 🏁 Bước 3.6: Nghiệm thu & Chuyển giao sang Dự án 4
*   **Mục tiêu:** Tổng kết bài học và chuẩn bị cho dự án SignalR & Redis.
*   **Thực hiện:** Chạy test toàn bộ luồng hệ thống từ đầu đến cuối và chuyển giao context.
