# Asset Management System (QLTS)

Hệ thống quản lý tài sản với phân quyền RBAC theo menu, phân quyền theo nhóm, quản lý tài sản theo danh mục (Hành chính, Y tế, Công nghệ thông tin), linh kiện thay thế, và tính khấu hao theo quy định Việt Nam.

## Kiến trúc

### Backend
- **Framework**: ASP.NET Core 8.0
- **Architecture**: Vertical Slice Monolith
- **Database**: MySQL với Entity Framework Core
- **Authentication**: JWT (Access Token + Refresh Token)
- **Authorization**: Menu-Based RBAC + Group-Based Permissions
- **Logging**: Serilog
- **Cache**: Redis
- **Search**: Elasticsearch
- **Documentation**: Swagger/OpenAPI
- **Patterns**: CQRS với MediatR, FluentValidation, Result<T> pattern

### Frontend
- **Framework**: Angular 18.2
- **Architecture**: Nx Monorepo
- **Components**: Standalone Components
- **UI Library**: PrimeNG 17
- **State Management**: Angular Signals (Signal-based state)
- **Reactive Patterns**: Computed signals, Effects
- **Routing**: Functional guards (AuthGuard, MenuPermissionGuard)
- **HTTP Client**: HttpClient with interceptors

## Cấu trúc dự án

```
QLTS/
├── backend/
│   ├── src/
│   │   ├── Api/                    # API Gateway
│   │   │   ├── Controllers/        # REST API endpoints
│   │   │   └── Features/           # Cross-module features
│   │   │       ├── AssignMenuPermissionsToRole/
│   │   │       └── GetRoleMenuPermissions/
│   │   ├── BuildingBlocks/         # Shared libraries
│   │   │   ├── Common/             # Common models, Result<T>
│   │   │   ├── Database/           # EF Core DbContext, Migrations
│   │   │   ├── Logging/            # Serilog configuration
│   │   │   ├── Caching/            # Redis cache
│   │   │   └── Search/             # Elasticsearch
│   │   ├── Identity/               # Authentication module
│   │   │   ├── Entities/           # User, RefreshToken
│   │   │   └── Features/           # Login, Register, RefreshToken
│   │   ├── Authorization/          # RBAC module
│   │   │   ├── Entities/           # Role, Permission, RolePermission, UserRole
│   │   │   └── Features/           # Role/Permission management
│   │   ├── Menus/                  # Menu-based permission module
│   │   │   ├── Entities/           # Menu, MenuPermission, GroupMenuPermission
│   │   │   ├── Configurations/     # EF Core configurations
│   │   │   └── Features/           # Menu CRUD, GetUserMenus
│   │   ├── Users/                  # User & Group management
│   │   │   ├── Entities/           # Group, UserGroup, GroupPermission
│   │   │   ├── Configurations/     # EF Core configurations
│   │   │   └── Features/           # User/Group CRUD, AssignPermissionsToGroup
│   │   ├── Assets/                 # Asset management
│   │   ├── Inventory/              # Inventory & components
│   │   └── Maintenance/            # Maintenance requests
│   └── AssetManagement.sln
├── frontend/
│   ├── apps/
│   │   └── admin-portal/           # Admin web application
│   │       ├── src/app/
│   │       │   ├── pages/
│   │       │   │   ├── dashboard/  # Main dashboard with dynamic menu
│   │       │   │   ├── menus/      # Menu management UI
│   │       │   │   ├── roles/      # Role menu permissions UI
│   │       │   │   ├── groups/     # Group management UI
│   │       │   │   ├── assets/     # Asset management
│   │       │   │   └── users/      # User management
│   │       │   └── app.routes.ts   # Route configuration
│   ├── libs/
│   │   ├── api-client/             # API client services
│   │   ├── auth/                   # Authentication & Guards
│   │   ├── ui/                     # Shared UI components
│   │   └── state/                  # State management
│   ├── package.json
│   └── .npmrc                      # legacy-peer-deps=true
└── docs/
    └── README.md                   # This file
```

## Tính năng chính

### 1. Quản lý người dùng & Phân quyền

#### Phân quyền theo Role (Role-Based Access Control)
- **Đăng ký, đăng nhập** với JWT (Access Token + Refresh Token)
- **Phân quyền theo Menu**: Mỗi menu có 4 loại quyền (View, Create, Edit, Delete)
- **Phân quyền theo Role**: Role → RolePermission → Permission ← MenuPermission ← Menu
- **Dynamic Menu Rendering**: Menu tự động hiển thị dựa trên quyền của user
- **Route Protection**: MenuPermissionGuard kiểm tra quyền trước khi cho phép truy cập

#### Phân quyền theo Nhóm (Group-Based Permissions) ⭐ MỚI
Hệ thống hỗ trợ **phân quyền theo nhóm** song song với phân quyền theo role:

**Cấu trúc quyền:**
```
User
  ├─ Quyền từ Role (Role Permissions) → Menu từ Role
  └─ Quyền từ Group (Group Permissions) → Menu từ Group
```

**Khi user đăng nhập → Menu hiển thị = Menu Role + Menu Group**

**Đặc điểm:**
- User có thể thuộc nhiều Group → Quyền = Union của tất cả Groups + Role
- Menu hiển thị = Union của Menu từ Role + Menu từ Group
- Quyền View được kiểm tra để hiển thị menu (PermissionType = "View")
- UNIQUE constraints đảm bảo không gán quyền trùng lặp
- Cascading Delete tự động xóa quyền khi xóa Group/Role

**Ví dụ nhóm:**
- **Phòng CNTT**: Quyền quản lý Users, Assets, có thể tạo/sửa tài sản
- **Phòng Kỹ thuật**: Quyền xem và sửa Assets
- **Phòng Hành chính**: Quyền xem Users và Groups

### 2. Quản lý Menu (UI hoàn chỉnh)
- **Menus List**: TreeTable hiển thị cấu trúc menu cha-con, CRUD operations
- **Menu Permissions Setup**: Gán permissions cho menu (View/Create/Edit/Delete)
- **Role Menu Permissions**: Gán menu permissions cho role (Tree với checkboxes)
- **Menu Service**: Signal-based state management, auto-load/clear on auth changes
- **Flat Cache**: Computed signal cho O(1) route lookup
- **Hierarchical Menus**: Hỗ trợ menu con không giới hạn độ sâu
- **Smart Permission Clearing**: Chỉ xóa quyền liên quan đến menu khi gán lại

### 3. Quản lý tài sản
- Phân loại tài sản: Hành chính, Y tế, Công nghệ thông tin
- Thông tin chi tiết: Mã tài sản, tên, nhà sản xuất, serial number, giá mua, vị trí
- Theo dõi trạng thái: Đang sử dụng, Bảo trì, Hỏng, Đã thanh lý
- Tính khấu hao theo phương pháp đường thẳng (Thông tư 45/2013/TT-BTC)

### 4. Quản lý linh kiện
- Kho linh kiện thay thế
- Theo dõi số lượng tồn kho
- Lịch sử thay thế linh kiện cho tài sản

### 5. Quản lý bảo trì
- Tạo phiếu báo hỏng tài sản
- Tự động chuyển phiếu đến đơn vị sửa chữa phù hợp
- Theo dõi trạng thái bảo trì
- Ghi nhận linh kiện sử dụng và chi phí

## Database Schema

### Core Tables
- **Users**: Người dùng
- **Roles**: Vai trò (Admin, Manager, User, v.v.)
- **Permissions**: Quyền hạn (ViewAssets, CreateAssets, v.v.)
- **UserRole**: Liên kết User ↔ Role (many-to-many)
- **RolePermission**: Liên kết Role ↔ Permission (many-to-many)

### Group Tables ⭐ MỚI
- **Groups**: Nhóm người dùng (Phòng CNTT, Phòng Kỹ thuật, v.v.)
- **UserGroup**: Liên kết User ↔ Group (many-to-many)
- **GroupPermissions**: Liên kết Group ↔ Permission (many-to-many)
- **GroupMenuPermissions**: Menu + Quyền cho nhóm (View, Create, Edit, Delete)

### Menu Tables
- **Menus**: Cấu trúc menu phân cấp (self-referencing ParentId)
- **MenuPermissions**: Liên kết Menu ↔ Permission với PermissionType

### Permission Flow

**Role-Based:**
```
User → UserRole → Role → RolePermission → Permission
                                            ↑
Menu → MenuPermission ──────────────────────┘
```

**Group-Based (MỚI):**
```
User → UserGroup → Group → GroupPermission → Permission
                                               ↑
Menu → GroupMenuPermission ─────────────────────┘
```

**Combined Flow:**
```
User
  ├─ UserRole → Role → RolePermission ─────┐
  └─ UserGroup → Group → GroupPermission ──┤
                                           ↓
                                      Permission
                                           ↑
Menu ← MenuPermission ──────────────────────┤
Menu ← GroupMenuPermission ──────────────────┘
```

## Yêu cầu hệ thống

### Backend
- .NET 8 SDK
- MySQL 8.0+
- Redis (optional)
- Elasticsearch (optional)

### Frontend
- Node.js 18+
- npm 8+

## Hướng dẫn cài đặt

### 1. Cài đặt Backend

#### Bước 1: Clone repository và cài đặt dependencies
```bash
cd backend
dotnet restore
```

#### Bước 2: Cấu hình Database
Sửa file `backend/src/Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=AssetManagement_Dev;User=root;Password=your_password;"
  }
}
```

#### Bước 3: Chạy Migrations
```bash
cd backend

# Add migration
dotnet ef migrations add AddGroupPermissions --project src/BuildingBlocks/Database --startup-project src/Api

# Apply migration
dotnet ef database update --project src/BuildingBlocks/Database --startup-project src/Api
```

#### Bước 4: Chạy API
```bash
cd backend/src/Api
dotnet run
```

API sẽ chạy tại: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### 2. Cài đặt Frontend

#### Bước 1: Cài đặt dependencies
```bash
cd frontend
npm install --legacy-peer-deps
```

**Lưu ý**: Phải sử dụng `--legacy-peer-deps` vì PrimeNG 17 chỉ hỗ trợ Angular 18.

#### Bước 2: Cấu hình API URL
File `frontend/apps/admin-portal/src/environments/environment.ts` đã được cấu hình mặc định:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

#### Bước 3: Chạy ứng dụng
```bash
npm start
# hoặc
npx nx serve admin-portal
```

Frontend sẽ chạy tại: `http://localhost:4200`

#### Bước 4: Build cho production
```bash
npm run build
# hoặc
npx nx build admin-portal
```

## Sử dụng

### 1. Đăng ký tài khoản
- Truy cập `http://localhost:4200/register`
- Nhập thông tin: username, email, mật khẩu, họ tên
- Click "Đăng ký"

### 2. Đăng nhập
- Truy cập `http://localhost:4200/login`
- Nhập username và password
- Click "Đăng nhập"

### 3. Quản lý Menu & Phân quyền

#### 3.1. Tạo Menu mới
- Vào "Menu Management" → "Menus"
- Click "Add Menu"
- Nhập: Name, Icon (PrimeNG class), Route, Display Order
- Chọn Parent Menu (nếu là menu con)
- Click "Save"

#### 3.2. Gán Permissions cho Menu
- Chọn menu trong danh sách
- Click "Setup Permissions"
- Chọn permissions cần gán (View/Create/Edit/Delete)
- Click "Save Permissions"

#### 3.3. Gán Menu Permissions cho Role
- Vào "Roles" → "Role Menu Permissions"
- Chọn Role từ dropdown
- Dùng checkboxes để chọn quyền cho từng menu
- Click "Save Assignments"

### 4. Quản lý Nhóm & Phân quyền theo Nhóm ⭐ MỚI

#### 4.1. Tạo Nhóm mới
- Vào "Groups"
- Click "Add Group"
- Nhập: Name, Description
- Click "Save"

#### 4.2. Gán Permissions cho Nhóm
```
POST /api/users/groups/{groupId}/permissions
Body: { "permissionIds": ["uuid1", "uuid2", ...] }
```

#### 4.3. Gán User vào Nhóm
- Vào "Users" → chọn user
- Click "Assign Groups"
- Chọn các nhóm cần gán
- Click "Save"

### 5. Quản lý tài sản
- Sau khi đăng nhập, click "Quản lý tài sản"
- Xem danh sách tài sản
- Thêm tài sản mới
- Tìm kiếm và lọc tài sản

## API Endpoints

### Authentication
- `POST /api/auth/register` - Đăng ký tài khoản mới
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/refresh-token` - Làm mới access token

### Menus
- `GET /api/menus` - Lấy danh sách tất cả menus (admin)
- `GET /api/menus/user` - Lấy menus của user hiện tại (kết hợp Role + Group)
- `POST /api/menus` - Tạo menu mới
- `GET /api/menus/{id}` - Lấy chi tiết menu
- `PUT /api/menus/{id}` - Cập nhật menu
- `DELETE /api/menus/{id}` - Xóa menu
- `GET /api/menus/{menuId}/permissions` - Lấy permissions của menu
- `POST /api/menus/{menuId}/permissions` - Gán permissions cho menu

### Permissions
- `GET /api/permissions` - Lấy danh sách tất cả permissions

### Roles
- `GET /api/roles` - Lấy danh sách vai trò
- `POST /api/roles` - Tạo vai trò mới
- `POST /api/roles/{roleId}/menu-permissions` - Gán menu permissions cho role
- `GET /api/roles/{roleId}/menu-permissions` - Lấy menu permissions của role

### Groups ⭐ MỚI
- `GET /api/groups` - Lấy danh sách nhóm
- `POST /api/groups` - Tạo nhóm mới
- `PUT /api/groups/{id}` - Cập nhật nhóm
- `DELETE /api/groups/{id}` - Xóa nhóm
- `POST /api/users/groups/{groupId}/permissions` - Gán permissions cho nhóm

### Users
- `GET /api/users` - Lấy danh sách người dùng
- `POST /api/users/{id}/roles` - Gán vai trò cho user
- `POST /api/users/{id}/groups` - Gán nhóm cho user

### Assets
- `GET /api/assets` - Lấy danh sách tài sản (có phân trang)
- `POST /api/assets` - Tạo tài sản mới
- `GET /api/assets/{id}` - Lấy chi tiết tài sản
- `PUT /api/assets/{id}` - Cập nhật tài sản
- `DELETE /api/assets/{id}` - Xóa tài sản

## Kiến trúc Backend: Vertical Slice

Mỗi feature được tổ chức thành một "slice" tự chứa theo pattern CQRS:

```
Users/
├── Entities/
│   ├── Group.cs                        # Nhóm người dùng
│   ├── UserGroup.cs                    # User ↔ Group
│   └── GroupPermission.cs              # Group ↔ Permission
├── Configurations/                     # EF Core configurations
│   ├── GroupConfiguration.cs
│   └── GroupPermissionConfiguration.cs
├── Features/
│   ├── CreateGroup/
│   ├── UpdateGroup/
│   ├── DeleteGroup/
│   ├── GetGroups/
│   └── AssignPermissionsToGroup/       # Gán quyền cho nhóm
│       ├── AssignPermissionsToGroupCommand.cs
│       ├── AssignPermissionsToGroupCommandHandler.cs
│       └── AssignPermissionsToGroupValidator.cs
└── UsersModule.cs

Menus/
├── Entities/
│   ├── Menu.cs                         # Menu hierarchy
│   ├── MenuPermission.cs               # Menu ↔ Permission (Role-based)
│   └── GroupMenuPermission.cs          # Menu ↔ Group ↔ Permission
├── Configurations/
│   ├── MenuConfiguration.cs
│   ├── MenuPermissionConfiguration.cs
│   └── GroupMenuPermissionConfiguration.cs
├── Features/
│   ├── GetUserMenus/                   # CRITICAL - Load user menus
│   │   ├── GetUserMenusQuery.cs        # Kết hợp Role + Group
│   │   └── GetUserMenusQueryHandler.cs # Union logic
│   ├── CreateMenu/
│   ├── UpdateMenu/
│   └── DeleteMenu/
└── MenusModule.cs
```

### GetUserMenus Logic - Kết hợp Role + Group

```csharp
// 1. Lấy quyền từ Role
var rolePermissionIds = await _context.Set<RolePermission>()
    .Where(rp => userRoleIds.Contains(rp.RoleId))
    .Select(rp => rp.PermissionId)
    .ToListAsync();

// 2. Lấy quyền từ Group (MỚI)
var groupPermissionIds = await _context.Set<GroupPermission>()
    .Where(gp => userGroupIds.Contains(gp.GroupId))
    .Select(gp => gp.PermissionId)
    .ToListAsync();

// 3. Kết hợp cả hai
var allPermissionIds = rolePermissionIds.Union(groupPermissionIds).ToList();

// 4. Lấy menu từ Role
var roleMenuIds = await _context.Set<MenuPermission>()
    .Where(mp => rolePermissionIds.Contains(mp.PermissionId))
    .Select(mp => mp.MenuId)
    .ToListAsync();

// 5. Lấy menu từ Group (MỚI)
var groupMenuIds = await _context.Set<GroupMenuPermission>()
    .Where(gmp => userGroupIds.Contains(gmp.GroupId))
    .Select(gmp => gmp.MenuId)
    .ToListAsync();

// 6. Kết hợp cả hai menu
var accessibleMenuIds = roleMenuIds.Union(groupMenuIds).ToList();
```

### Result<T> Pattern
Tất cả handlers trả về `Result<T>` để xử lý lỗi nhất quán:

```csharp
public async Task<Result<MenuDto>> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
{
    var menu = await _context.Set<Menu>().FindAsync(request.Id);
    if (menu == null)
        return Result.Failure<MenuDto>(new Error("Menu.NotFound", "Menu not found"));

    return Result.Success(menu.ToDto());
}
```

## Kiến trúc Frontend: Signal-Based State

### MenuService - Signal-Based State Management
```typescript
export class MenuService {
  private userMenus = signal<MenuDto[]>([]);
  public readonly menus = this.userMenus.asReadonly();
  public readonly flatMenus = computed(() => this.flattenMenus(this.userMenus()));

  constructor(private menusApiService: MenusApiService, private authService: AuthService) {
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.loadUserMenus();  // Auto-load when authenticated
      } else {
        this.userMenus.set([]);  // Auto-clear when logged out
      }
    });
  }

  hasMenuAccess(routePath: string): boolean {
    const normalizedPath = this.normalizePath(routePath);
    return this.flatMenus().some(menu =>
      menu.route && this.normalizePath(menu.route) === normalizedPath);
  }
}
```

### MenuPermissionGuard
```typescript
export const menuPermissionGuard: CanActivateFn = (route, state) => {
  const menuService = inject(MenuService);
  const router = inject(Router);

  if (menuService.hasMenuAccess(state.url)) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
```

### Dynamic Menu Rendering
```typescript
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MenubarModule],
  template: `<p-menubar [model]="menuItems()" />`
})
export class DashboardComponent {
  private menuService = inject(MenuService);

  menuItems = computed(() =>
    this.convertToMenuItems(this.menuService.menus())
  );
}
```

## Luồng Phân quyền Hoàn chỉnh

```
USER ĐĂNG NHẬP
    ↓
JWT Token được tạo
    ↓
Frontend gọi GET /api/menus/user
    ↓
Backend GetUserMenus:
    ├─ Lấy Role IDs của user
    ├─ Lấy Group IDs của user
    ├─ Lấy Permission IDs từ Roles
    ├─ Lấy Permission IDs từ Groups
    ├─ Kết hợp Permissions (Role + Group)
    ├─ Lấy Menu IDs từ Role-Permissions
    ├─ Lấy Menu IDs từ Group-Permissions
    ├─ Kết hợp Menu IDs
    └─ Build hierarchical menu
    ↓
Frontend nhận menu và hiển thị
```

## Tính khấu hao

Hệ thống sử dụng phương pháp khấu hao đường thẳng theo quy định Việt Nam:

**Công thức:**
```
Khấu hao tháng = Nguyên giá × Tỷ lệ khấu hao năm / 12
```

**Ví dụ:**
- Máy tính: Nguyên giá 20,000,000 VND, tỷ lệ khấu hao 25%/năm
- Khấu hao tháng = 20,000,000 × 25% / 12 = 416,667 VND

Tham khảo:
- Thông tư 45/2013/TT-BTC
- Thông tư 200/2014/TT-BTC

## Mở rộng thêm modules

Để thêm module mới (ví dụ: Reports), tạo theo pattern Vertical Slice:

### Backend
```bash
cd backend/src
mkdir Reports
```

Tạo các files:
- `Entities/` - Domain entities
- `Features/` - Vertical slices (Commands/Queries + Handlers + Validators)
- `Configurations/` - EF Core configurations
- `ReportsModule.cs` - Service registration

### Frontend
```bash
cd frontend/libs
npx nx g @nx/angular:library reports
```

Tạo services và components cần thiết theo standalone pattern.

## Troubleshooting

### Backend không kết nối được MySQL
- Kiểm tra MySQL đã chạy: `mysql -u root -p`
- Kiểm tra connection string trong `appsettings.json`
- Tạo database thủ công: `CREATE DATABASE AssetManagement_Dev;`

### Frontend không call được API
- Kiểm tra CORS trong `Program.cs`
- Kiểm tra API URL trong `environment.ts`
- Mở Developer Tools > Network để xem lỗi

### npm install errors (PrimeNG compatibility)
```bash
# Xóa node_modules và package-lock.json
cd frontend
rmdir /s /q node_modules
del package-lock.json

# Install lại với legacy-peer-deps
npm install --legacy-peer-deps
```

Hoặc chạy script tự động:
```bash
cd frontend
install-fix.bat
```

### Migration errors
```bash
# Remove all migrations
cd backend/src/BuildingBlocks/Database
rm -rf Migrations/

# Create new migration
cd ../../Api
dotnet ef migrations add InitialCreate --project ../BuildingBlocks/Database
dotnet ef database update --project ../BuildingBlocks/Database
```

### Menu không hiển thị
1. Kiểm tra user đã được gán role hoặc group chưa
2. Kiểm tra role/group đã được gán menu permissions chưa
3. Kiểm tra menu có IsActive = true không
4. Mở browser console xem lỗi API call

### Circular Dependency giữa modules
Nếu gặp lỗi circular dependency:
- Kiểm tra project references trong `.csproj` files
- Đảm bảo dependency flow đúng: BuildingBlocks ← Identity/Authorization/Users ← Menus ← Api
- Users module chứa Group, GroupPermission
- Menus module reference Users để sử dụng GroupPermission

## Scripts hỗ trợ

### Backend
```bash
# Run API
cd backend/src/Api
dotnet run

# Run with watch (auto-reload)
dotnet watch run

# Build
cd backend
dotnet build AssetManagement.sln
```

### Frontend
```bash
# Development server
npm start

# Build
npm run build

# Run tests
npm test

# Lint
npm run lint
```

## FAQ - Phân quyền

**Q: User nếu thuộc Role Admin + Group "Phòng IT" thì thấy menu nào?**
A: Thấy menu = Menu của Admin role UNION Menu của Phòng IT group

**Q: Làm sao để user của "Phòng IT" không thấy menu "Settings"?**
A: Chỉ cần không gán `GroupMenuPermissions` cho "Phòng IT" với menu "Settings"

**Q: Có thể ghi đè quyền Role bằng Group không?**
A: Không. Union logic → nếu Role có quyền, user vẫn có dù Group không có

**Q: GroupPermission nằm ở đâu trong cấu trúc?**
A: GroupPermission entity nằm trong **Users module** (Users/Entities/GroupPermission.cs) vì liên quan trực tiếp đến quản lý nhóm người dùng.

## Roadmap

- [x] Implement Identity module (Login, Register, JWT)
- [x] Implement Authorization module (Roles, Permissions, RBAC)
- [x] Implement Menus module (Menu-based permissions)
- [x] Implement Menu Management UI (TreeTable, CRUD)
- [x] Implement Menu Permissions Setup UI
- [x] Implement Role Menu Permissions UI
- [x] Implement Dynamic Menu Rendering
- [x] Implement MenuPermissionGuard
- [x] Implement Group-Based Permissions ⭐
- [x] Implement GroupPermission entity & configurations
- [x] Implement GroupMenuPermission entity
- [x] Update GetUserMenus to support Role + Group
- [ ] Implement Groups Management UI
- [ ] Implement Group Permissions Assignment UI
- [ ] Implement Users module với CRUD operations
- [ ] Implement Inventory module với quản lý kho
- [ ] Implement Maintenance module với workflow
- [ ] Thêm Reports & Analytics
- [ ] Export dữ liệu ra Excel
- [ ] Dashboard với charts
- [ ] Notifications real-time
- [ ] Mobile app

## Liên hệ & Đóng góp

Dự án tham khảo: [warehouse-fullstack](https://github.com/CanThanh/warehouse-fullstack)

## License

MIT License
