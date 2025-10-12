# Video-sharing-platform
AWS-Project


1. Cấu trúc thư mục

		src/
	 ├── Controllers/       → Chứa các API controller
	 ├── Models/            → Định nghĩa model (entity, DTO, request)
	 ├── Services/          → Xử lý nghiệp vụ (business logic)
	 ├── Repositories/      → Làm việc với database (DynamoDB, SQL, v.v.)
	 ├── Configurations/    → Cấu hình AWS, Database, Dependency Injection
	 ├── Program.cs         → Entry point của ứng dụng
	 └── appsettings.json   → Cấu hình môi trường



2. RESTful API Rule

	| Hành động   | HTTP Method | Route             | Ví dụ                              |
	| ----------- | ----------- | ----------------- | ---------------------------------- |
	| Lấy tất cả  | GET         | `/api/users`      | `GetAllUsers()`                    |
	| Lấy theo id | GET         | `/api/users/{id}` | `GetUserById(string id)`           |
	| Thêm mới    | POST        | `/api/users`      | `CreateUser(User user)`            |
	| Cập nhật    | PUT         | `/api/users/{id}` | `UpdateUser(string id, User user)` |
	| Xóa         | DELETE      | `/api/users/{id}` | `DeleteUser(string id)`            |

	*Không dùng verb trong route (ví dụ: /api/getUsers ❌)
	*Dùng danh từ số nhiều cho tài nguyên (ví dụ: /api/users ✅, /api/user ❌)



3. Quy tắc đặt tên

	| Thành phần        | Quy tắc              | Ví dụ                                     |
	| ----------------- | -------------------- | ----------------------------------------- |
	| Class & Interface | PascalCase           | `UserService`, `IUserRepository`          |
	| Method            | PascalCase           | `GetUserById()`                           |
	| Biến & Tham số    | camelCase            | `userId`, `userName`                      |
	| File              | Trùng với tên class  | `UserController.cs`, `DynamoDbService.cs` |
	| Route             | snake-case (RESTful) | `/api/users/get-all`                      |



4. Dependency Injection (DI)

		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IUserRepository, UserRepository>();

	- Dùng Interface + Service để tách logic
	- Không inject trực tiếp context hoặc client AWS trong controller.