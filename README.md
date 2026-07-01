# UniTaste - Backend Application

UniTaste là dự án Backend được xây dựng để cung cấp nền tảng khám phá ẩm thực, quản lý nhà hàng, tương tác mạng xã hội và thanh toán trực tuyến. Dự án được phát triển với mục tiêu đem lại trải nghiệm liền mạch cho người dùng trong việc tìm kiếm, đánh giá và đặt món tại các nhà hàng.

## 🏗️ Kiến trúc Hệ thống (Architecture)
Dự án được xây dựng theo kiến trúc **Microservices** tiên tiến, giúp hệ thống dễ dàng mở rộng, bảo trì và triển khai các thành phần một cách độc lập.
- **API Gateway**: Sử dụng **Ocelot** làm cổng giao tiếp duy nhất (Single Point of Entry) cho các client (Web/Mobile App). Tất cả request từ bên ngoài đều đi qua Gateway để được định tuyến tới các dịch vụ phía sau.

## ⚙️ Các Microservices và Chức năng chính
Dự án bao gồm 5 project chính:

1. **ApiGateway**: Chịu trách nhiệm định tuyến (Routing) các API requests đến các Microservices tương ứng (User, Restaurant, Payment, Social).
2. **UserService.API**: 
   - Quản lý tài khoản, đăng ký, đăng nhập.
   - Authentication & Authorization bằng **JWT (JSON Web Token)**.
   - Mã hóa mật khẩu an toàn với **BCrypt**.
   - Gửi email thông báo, xác thực người dùng thông qua **SendGrid**.
   - Cung cấp các tính năng thông minh bằng cách tích hợp **OpenAI** (ví dụ: gợi ý món ăn, tóm tắt đánh giá).
3. **RestaurantService.API**:
   - Quản lý thông tin nhà hàng, món ăn, và đánh giá của người dùng.
   - Tích hợp **Google Places API** để kéo dữ liệu địa điểm, nhà hàng và các đánh giá từ Google.
4. **PaymentService.API**:
   - Chịu trách nhiệm xử lý các luồng thanh toán hóa đơn.
   - Tích hợp cổng thanh toán nội địa **VNPAY**.
5. **SocialService.API**:
   - Quản lý tương tác giữa các người dùng (bình luận, thả tim, chia sẻ).
   - Xử lý thông báo và chat theo thời gian thực (Real-time) bằng **SignalR**.
   - Hỗ trợ xuất/đọc báo cáo bằng Excel thông qua **NPOI**.

## 🚀 Công nghệ và Dịch vụ (Tech Stack & Services)
Dự án sử dụng các công nghệ, thư viện và dịch vụ đám mây hiện đại:
- **Framework & Ngôn ngữ**: C# / .NET 8.0 (ASP.NET Core Web API)
- **Cơ sở dữ liệu (Database)**: SQL Server kết hợp với **Entity Framework Core**.
- **Bảo mật & Xác thực**: `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`
- **Lưu trữ Media (Cloud Storage)**:
  - **Cloudinary** (`CloudinaryDotNet`) để quản lý và tối ưu hóa hình ảnh.
  - **Google Cloud Storage** (`Google.Cloud.Storage.V1`) làm nơi lưu trữ các tệp tĩnh.
- **Dịch vụ bên thứ ba (Third-party Integrations)**:
  - **VNPAY**: Cổng thanh toán.
  - **SendGrid**: Dịch vụ gửi Email.
  - **Google API**: Lấy dữ liệu bản đồ, địa điểm nhà hàng.
  - **OpenAI**: Xử lý logic AI (Gợi ý, Chatbot).
- **Giao tiếp Real-time**: `Microsoft.AspNetCore.SignalR`
- **API Documentation**: Swagger (`Swashbuckle.AspNetCore`)

## 🔄 Core Flow cơ bản của dự án
1. **Luồng Xác thực (Auth Flow)**: Client gửi thông tin đăng nhập/đăng ký tới Gateway. Gateway forward tới `UserService`. Sau khi xác minh thành công với Database (hoặc tạo mới, gửi mail qua SendGrid), hệ thống trả về **JWT Token**. Client sẽ dùng token này để truy cập các dịch vụ khác.
2. **Luồng Khám phá Nhà hàng (Discovery Flow)**: Người dùng muốn tìm kiếm nhà hàng -> Request đi qua Gateway tới `RestaurantService`. Service này sẽ query SQL Server hoặc gọi sang **Google Places API** để lấy thông tin. Hình ảnh của nhà hàng và món ăn được load từ **Cloudinary** hoặc **Google Cloud Storage**.
3. **Luồng Tương tác (Social Flow)**: Khi người dùng tương tác, bình luận, hoặc nhắn tin -> Request gửi tới `SocialService`. Service này sử dụng **SignalR** để đẩy thông báo realtime tới các client khác đang online.
4. **Luồng Thanh toán (Payment Flow)**: Khi người dùng thanh toán đơn hàng -> `PaymentService` tiếp nhận request, tạo URL thanh toán **VNPAY** và trả về cho client. Client thực hiện thanh toán. Sau khi xong, VNPAY sẽ gọi Webhook về hệ thống để cập nhật trạng thái đơn hàng.

## 🛠️ Hướng dẫn cài đặt (Setup Instructions)
1. Clone dự án về máy.
2. Cập nhật các chuỗi kết nối Database (Connection Strings) và API Keys (Cloudinary, SendGrid, VNPAY, OpenAI, Google) trong các file `appsettings.json` của từng service.
3. Chạy lệnh `Update-Database` (Entity Framework) trong Package Manager Console cho các service có sử dụng DB để tự động tạo schema.
4. Cấu hình Ocelot (trong thư mục ApiGateway) trỏ tới đúng Port của các service đang chạy ở local.
5. Chạy Multi-project (chạy đồng thời Gateway và các Microservices) trong Visual Studio.
