-- ============================================
-- ASSET MANAGEMENT SYSTEM (QLTS)
-- COMPLETE DATABASE SCHEMA + SEED DATA
-- ============================================
-- Database: MySQL 8.0+
-- ORM: Entity Framework Core 8.0
-- Architecture: Code-First with Migrations
--
-- Cách sử dụng:
-- 1. Tạo database: CREATE DATABASE Core;
-- 2. Chạy file này: mysql -u root -p Core < database-schema.sql
--
-- ⚠️ LƯU Ý QUAN TRỌNG:
-- - File này được đồng bộ với EF Core Configurations
-- - Các Many-to-Many tables dùng COMPOSITE PRIMARY KEY:
--   * UserRoles(UserId, RoleId)
--   * RolePermissions(RoleId, PermissionId)
--   * UserGroups(UserId, GroupId)
--   * GroupPermissions(GroupId, PermissionId)
-- - MenuPermissions và GroupMenuPermissions dùng Id PRIMARY KEY
-- - THỨ TỰ TẠO BẢNG ĐÃ ĐƯỢC SẮP XẾP ĐỂ TRÁNH FOREIGN KEY ERRORS:
--   1. Base tables: Users, Roles, Permissions, Groups
--   2. Many-to-Many: UserRoles, RolePermissions, UserGroups, GroupPermissions
--   3. Menus (self-referencing)
--   4. Menu relations: MenuPermissions, GroupMenuPermissions
--   5. Assets: AssetCategories, Assets, AssetDepreciations
-- - Trong thực tế, nên dùng EF Core Migrations thay vì chạy script này
-- ============================================

USE CORE;

-- ============================================
-- PHẦN 1: CREATE TABLES
-- ============================================

-- --------------------------------------------
-- Table: Users
-- Quản lý thông tin người dùng trong hệ thống
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS Users (
    Id CHAR(36) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL COMMENT 'Mật khẩu đã hash với BCrypt',
    FullName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    IsActive BOOLEAN DEFAULT TRUE,
    LastLoginAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    INDEX IX_Users_Username (Username),
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Quản lý người dùng hệ thống';

-- --------------------------------------------
-- Table: RefreshTokens
-- Lưu trữ refresh tokens cho JWT authentication
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    Token VARCHAR(500) NOT NULL COMMENT 'Refresh token đã hash',
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    RevokedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_RefreshTokens_UserId (UserId),
    INDEX IX_RefreshTokens_Token (Token),
    INDEX IX_RefreshTokens_ExpiresAt (ExpiresAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='JWT Refresh Tokens';

-- --------------------------------------------
-- Table: PasswordResetTokens
-- Lưu trữ tokens để reset mật khẩu
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS PasswordResetTokens (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    Token VARCHAR(500) NOT NULL COMMENT 'Reset token đã hash',
    ExpiresAt DATETIME NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    UsedAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_PasswordResetTokens_UserId (UserId),
    INDEX IX_PasswordResetTokens_Token (Token),
    INDEX IX_PasswordResetTokens_ExpiresAt (ExpiresAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Password Reset Tokens';

-- --------------------------------------------
-- Table: Roles
-- Định nghĩa các vai trò trong hệ thống
-- Ví dụ: SuperAdmin, Admin, AssetManager, Technician, User
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS Roles (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    INDEX IX_Roles_Name (Name),
    INDEX IX_Roles_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Vai trò trong hệ thống';

-- --------------------------------------------
-- Table: Permissions
-- Định nghĩa các quyền hạn cụ thể
-- Format: {Module}.{Action} (vd: Users.View, Assets.Create)
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS Permissions (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE COMMENT 'Format: Module.Action',
    Module VARCHAR(50) NOT NULL COMMENT 'Module: Users, Assets, Roles, Menus, Groups',
    Description VARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    INDEX IX_Permissions_Name (Name),
    INDEX IX_Permissions_Module (Module)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Quyền hạn cụ thể';

-- --------------------------------------------
-- Table: UserRoles
-- Liên kết Many-to-Many giữa Users và Roles
-- Một user có thể có nhiều roles
-- NOTE: Sử dụng COMPOSITE PRIMARY KEY (UserId, RoleId) theo EF Core config
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS UserRoles (
    UserId CHAR(36) NOT NULL,
    RoleId CHAR(36) NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    INDEX IX_UserRoles_UserId (UserId),
    INDEX IX_UserRoles_RoleId (RoleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User ↔ Role (Many-to-Many)';

-- --------------------------------------------
-- Table: RolePermissions
-- Liên kết Many-to-Many giữa Roles và Permissions
-- Một role có thể có nhiều permissions
-- NOTE: Sử dụng COMPOSITE PRIMARY KEY (RoleId, PermissionId) theo EF Core config
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS RolePermissions (
    RoleId CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    INDEX IX_RolePermissions_RoleId (RoleId),
    INDEX IX_RolePermissions_PermissionId (PermissionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Role ↔ Permission (Many-to-Many)';

-- --------------------------------------------
-- Table: Groups
-- Quản lý nhóm người dùng (phòng ban, đơn vị)
-- Ví dụ: Phòng Hành chính, Phòng Y tế, Phòng CNTT
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS `Groups` (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(255),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    INDEX IX_Groups_Name (Name),
    INDEX IX_Groups_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Nhóm người dùng (phòng ban)';

-- --------------------------------------------
-- Table: UserGroups
-- Liên kết Many-to-Many giữa Users và Groups
-- NOTE: Sử dụng COMPOSITE PRIMARY KEY (UserId, GroupId) theo EF Core config
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS UserGroups (
    UserId CHAR(36) NOT NULL,
    GroupId CHAR(36) NOT NULL,
    JoinedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, GroupId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (GroupId) REFERENCES `Groups`(Id) ON DELETE CASCADE,
    INDEX IX_UserGroups_UserId (UserId),
    INDEX IX_UserGroups_GroupId (GroupId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User ↔ Group (Many-to-Many)';

-- ============================================
-- ⭐ NEW TABLE: GroupPermissions
-- Gán quyền cho Nhóm (Group)
-- Mỗi nhóm có thể có quyền riêng độc lập với Role
-- User trong nhóm sẽ kế thừa quyền của nhóm + quyền từ role
-- NOTE: Sử dụng COMPOSITE PRIMARY KEY (GroupId, PermissionId) theo EF Core config
-- ============================================

CREATE TABLE IF NOT EXISTS GroupPermissions (
    Id CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Entity có property Id nhưng PK là composite',
    GroupId CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (GroupId, PermissionId),
    FOREIGN KEY (GroupId) REFERENCES `Groups`(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    INDEX IX_GroupPermissions_GroupId (GroupId),
    INDEX IX_GroupPermissions_PermissionId (PermissionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Group ↔ Permission (Many-to-Many)';

-- --------------------------------------------
-- Table: Menus ⭐ (MENU-BASED PERMISSIONS)
-- Cấu trúc menu phân cấp với self-referencing relationship
-- ParentId = NULL → Menu gốc
-- ParentId = {MenuId} → Menu con
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS Menus (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL COMMENT 'Tên menu hiển thị trên UI',
    Icon VARCHAR(50) COMMENT 'PrimeNG icon class (vd: pi pi-home)',
    Route VARCHAR(255) COMMENT 'Angular route path (vd: /dashboard, /assets)',
    DisplayOrder INT NOT NULL DEFAULT 0 COMMENT 'Thứ tự hiển thị (ASC)',
    IsActive BOOLEAN DEFAULT TRUE,
    ParentId CHAR(36) COMMENT 'ID của menu cha (NULL = root menu)',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    FOREIGN KEY (ParentId) REFERENCES Menus(Id) ON DELETE CASCADE,
    INDEX IX_Menus_ParentId (ParentId),
    INDEX IX_Menus_DisplayOrder (DisplayOrder),
    INDEX IX_Menus_IsActive (IsActive),
    INDEX IX_Menus_Route (Route)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Menu phân cấp (hierarchical)';

-- --------------------------------------------
-- Table: MenuPermissions ⭐ (MENU-BASED PERMISSIONS)
-- Liên kết Menus với Permissions, kèm PermissionType
-- PermissionType: View, Create, Edit, Delete
-- User chỉ thấy menu nếu có permission type "View"
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS MenuPermissions (
    Id CHAR(36) PRIMARY KEY,
    MenuId CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    PermissionType VARCHAR(20) NOT NULL COMMENT 'View, Create, Edit, Delete',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    FOREIGN KEY (MenuId) REFERENCES Menus(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    UNIQUE KEY UK_MenuPermission_MenuId_PermissionId_Type (MenuId, PermissionId, PermissionType),
    INDEX IX_MenuPermissions_MenuId (MenuId),
    INDEX IX_MenuPermissions_PermissionId (PermissionId),
    INDEX IX_MenuPermissions_PermissionType (PermissionType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Menu ↔ Permission với PermissionType';

-- ============================================
-- ⭐ NEW TABLE: GroupMenuPermissions
-- Gán quyền menu cho Nhóm
-- Mỗi nhóm có menu và quyền riêng
-- User trong nhóm sẽ chỉ thấy menu của nhóm + menu từ role
-- NOTE: Tạo SAU Menus vì có FOREIGN KEY tham chiếu đến Menus
-- ============================================

CREATE TABLE IF NOT EXISTS GroupMenuPermissions (
    Id CHAR(36) PRIMARY KEY,
    GroupId CHAR(36) NOT NULL,
    MenuId CHAR(36) NOT NULL,
    PermissionId CHAR(36) NOT NULL,
    PermissionType VARCHAR(20) NOT NULL COMMENT 'View, Create, Edit, Delete',
    AssignedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GroupId) REFERENCES `Groups`(Id) ON DELETE CASCADE,
    FOREIGN KEY (MenuId) REFERENCES Menus(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    UNIQUE KEY UK_GroupMenuPermission_GroupId_MenuId_PermissionId_Type (GroupId, MenuId, PermissionId, PermissionType),
    INDEX IX_GroupMenuPermissions_GroupId (GroupId),
    INDEX IX_GroupMenuPermissions_MenuId (MenuId),
    INDEX IX_GroupMenuPermissions_PermissionId (PermissionId),
    INDEX IX_GroupMenuPermissions_PermissionType (PermissionType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Group ↔ Menu ↔ Permission (Menu-based permissions for groups)';

-- --------------------------------------------
-- Table: AssetCategories
-- Phân loại tài sản
-- Ví dụ: Hành chính, Y tế, Công nghệ thông tin
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS AssetCategories (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    INDEX IX_AssetCategories_Name (Name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Phân loại tài sản';

-- --------------------------------------------
-- Table: Assets
-- Quản lý tài sản
-- Status: InUse, Maintenance, Broken, Disposed
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS Assets (
    Id CHAR(36) PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE COMMENT 'Mã tài sản (unique)',
    Name VARCHAR(200) NOT NULL,
    CategoryId CHAR(36) NOT NULL,
    Manufacturer VARCHAR(100) COMMENT 'Nhà sản xuất',
    SerialNumber VARCHAR(100) COMMENT 'Số serial',
    PurchasePrice DECIMAL(18,2) NOT NULL COMMENT 'Giá mua (VND)',
    PurchaseDate DATE NOT NULL,
    DepreciationRate DECIMAL(5,2) NOT NULL COMMENT 'Tỷ lệ khấu hao năm (%)',
    Location VARCHAR(255) COMMENT 'Vị trí đặt tài sản',
    Status VARCHAR(20) NOT NULL COMMENT 'InUse, Maintenance, Broken, Disposed',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    FOREIGN KEY (CategoryId) REFERENCES AssetCategories(Id),
    INDEX IX_Assets_Code (Code),
    INDEX IX_Assets_CategoryId (CategoryId),
    INDEX IX_Assets_Status (Status),
    INDEX IX_Assets_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Quản lý tài sản';

-- --------------------------------------------
-- Table: AssetDepreciations
-- Tính khấu hao tài sản theo tháng
-- Công thức: Khấu hao tháng = Nguyên giá × Tỷ lệ khấu hao năm / 12
-- --------------------------------------------
CREATE TABLE IF NOT EXISTS AssetDepreciations (
    Id CHAR(36) PRIMARY KEY,
    AssetId CHAR(36) NOT NULL,
    Month INT NOT NULL COMMENT 'Tháng (1-12)',
    Year INT NOT NULL COMMENT 'Năm',
    DepreciationAmount DECIMAL(18,2) NOT NULL COMMENT 'Số tiền khấu hao trong tháng',
    AccumulatedDepreciation DECIMAL(18,2) NOT NULL COMMENT 'Khấu hao lũy kế',
    BookValue DECIMAL(18,2) NOT NULL COMMENT 'Giá trị còn lại',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AssetId) REFERENCES Assets(Id) ON DELETE CASCADE,
    UNIQUE KEY UK_AssetDepreciation_AssetId_Month_Year (AssetId, Month, Year),
    INDEX IX_AssetDepreciations_AssetId (AssetId),
    INDEX IX_AssetDepreciations_Year_Month (Year, Month)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Khấu hao tài sản theo tháng';

-- ============================================
-- PHẦN 2: SEED DATA
-- ============================================

/*
 * Bước 1: Tạo Admin User (Người dùng quản trị)
 * Username: admin
 * Password: Admin@123
 * Email: admin@qlts.vn
 *
 * LƯU Ý: Password hash dưới đây là BCrypt hash của "Admin@123"
 * Nếu muốn đổi password, dùng API /api/auth/register hoặc hash bằng BCrypt tool
 */
SET @adminUserId = UUID();

INSERT INTO Users (Id, Username, Email, PasswordHash, FullName, PhoneNumber, IsActive, CreatedAt) VALUES
(@adminUserId, 'admin', 'admin@qlts.vn', '$2a$11$BMpGGqNDFHU9137cTAwb3uIdEuQMBDL7brdTUNhAB99DPNIIeaTBm', 'Quản Trị Viên', '0123456789', TRUE, NOW());

/*
 * Bước 2: Tạo Roles (Vai trò)
 * - SuperAdmin: Toàn quyền hệ thống
 * - Admin: Quản trị viên
 * - AssetManager: Quản lý tài sản
 * - Technician: Kỹ thuật viên bảo trì
 * - User: Người dùng thông thường
 */
INSERT INTO Roles (Id, Name, Description, IsActive, CreatedAt) VALUES
(UUID(), 'SuperAdmin', 'Full system access', TRUE, NOW()),
(UUID(), 'Admin', 'Administrative access', TRUE, NOW()),
(UUID(), 'AssetManager', 'Asset management access', TRUE, NOW()),
(UUID(), 'Technician', 'Maintenance and repair access', TRUE, NOW()),
(UUID(), 'User', 'Basic user access', TRUE, NOW());

/*
 * Bước 3: Gán role SuperAdmin cho admin user
 * Admin user sẽ có đầy đủ quyền trên toàn hệ thống
 */
SET @superAdminRoleId = (SELECT Id FROM Roles WHERE Name = 'SuperAdmin' LIMIT 1);

INSERT INTO UserRoles (UserId, RoleId, AssignedAt) VALUES
(@adminUserId, @superAdminRoleId, NOW());

/*
 * Bước 4: Tạo Permissions (Quyền hạn)
 * Format: {Module}.{Action}
 * - Users: Users.View, Users.Create, Users.Edit, Users.Delete
 * - Assets: Assets.View, Assets.Create, Assets.Edit, Assets.Delete
 * - Roles: Roles.View, Roles.Create, Roles.Edit, Roles.Delete
 * - Menus: Menus.View, Menus.Create, Menus.Edit, Menus.Delete
 * - Groups: Groups.View, Groups.Create, Groups.Edit, Groups.Delete
 */
INSERT INTO Permissions (Id, Name, Module, Description, CreatedAt) VALUES
-- Quyền quản lý Users
(UUID(), 'Users.View', 'Users', 'View users', NOW()),
(UUID(), 'Users.Create', 'Users', 'Create users', NOW()),
(UUID(), 'Users.Edit', 'Users', 'Edit users', NOW()),
(UUID(), 'Users.Delete', 'Users', 'Delete users', NOW()),
-- Quyền quản lý Assets
(UUID(), 'Assets.View', 'Assets', 'View assets', NOW()),
(UUID(), 'Assets.Create', 'Assets', 'Create assets', NOW()),
(UUID(), 'Assets.Edit', 'Assets', 'Edit assets', NOW()),
(UUID(), 'Assets.Delete', 'Assets', 'Delete assets', NOW()),
-- Quyền quản lý Roles
(UUID(), 'Roles.View', 'Roles', 'View roles', NOW()),
(UUID(), 'Roles.Create', 'Roles', 'Create roles', NOW()),
(UUID(), 'Roles.Edit', 'Roles', 'Edit roles', NOW()),
(UUID(), 'Roles.Delete', 'Roles', 'Delete roles', NOW()),
-- Quyền quản lý Menus
(UUID(), 'Menus.View', 'Menus', 'View menus', NOW()),
(UUID(), 'Menus.Create', 'Menus', 'Create menus', NOW()),
(UUID(), 'Menus.Edit', 'Menus', 'Edit menus', NOW()),
(UUID(), 'Menus.Delete', 'Menus', 'Delete menus', NOW()),
-- Quyền quản lý Groups
(UUID(), 'Groups.View', 'Groups', 'View groups', NOW()),
(UUID(), 'Groups.Create', 'Groups', 'Create groups', NOW()),
(UUID(), 'Groups.Edit', 'Groups', 'Edit groups', NOW()),
(UUID(), 'Groups.Delete', 'Groups', 'Delete groups', NOW());

/*
 * Bước 5: Gán TẤT CẢ quyền cho SuperAdmin role
 * CROSS JOIN tạo tổ hợp giữa role SuperAdmin và tất cả permissions
 * Kết quả: SuperAdmin có full quyền trên toàn hệ thống
 */
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT r.Id, p.Id, NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'SuperAdmin';

/*
 * Bước 6: Tạo Menu nhiều cấp
 *
 * CẤU TRÚC MENU:
 * ├── Tổng quan              (/dashboard)         pi pi-home
 * ├── Quản lý Tài sản        (parent)             pi pi-briefcase
 * │   └── Danh sách Tài sản  (/assets)            pi pi-list
 * ├── Nhân sự                (parent)             pi pi-users
 * │   ├── Người dùng         (/users)             pi pi-user
 * │   └── Nhóm / Phòng ban   (/groups)            pi pi-sitemap
 * └── Hệ thống               (parent)             pi pi-cog
 *     ├── Vai trò             (/roles)             pi pi-shield
 *     ├── Quản lý Menu        (/menus)             pi pi-bars
 *     └── Phân quyền Menu     (/roles/menu-permissions)  pi pi-key
 */

-- Cấp 1: Menu gốc (ParentId = NULL)
INSERT INTO Menus (Id, Name, Icon, Route, DisplayOrder, ParentId, IsActive, CreatedAt) VALUES
(UUID(), 'Tổng quan', 'pi pi-home', '/dashboard', 1, NULL, TRUE, NOW()),
(UUID(), 'Quản lý Tài sản', 'pi pi-briefcase', NULL, 2, NULL, TRUE, NOW()),
(UUID(), 'Nhân sự', 'pi pi-users', NULL, 3, NULL, TRUE, NOW()),
(UUID(), 'Hệ thống', 'pi pi-cog', NULL, 4, NULL, TRUE, NOW());

-- Cấp 2: Menu con của "Quản lý Tài sản"
SET @assetParentId = (SELECT Id FROM Menus WHERE Name = 'Quản lý Tài sản' LIMIT 1);

INSERT INTO Menus (Id, Name, Icon, Route, DisplayOrder, ParentId, IsActive, CreatedAt) VALUES
(UUID(), 'Danh sách Tài sản', 'pi pi-list', '/assets', 1, @assetParentId, TRUE, NOW());

-- Cấp 2: Menu con của "Nhân sự"
SET @hrParentId = (SELECT Id FROM Menus WHERE Name = 'Nhân sự' LIMIT 1);

INSERT INTO Menus (Id, Name, Icon, Route, DisplayOrder, ParentId, IsActive, CreatedAt) VALUES
(UUID(), 'Người dùng', 'pi pi-user', '/users', 1, @hrParentId, TRUE, NOW()),
(UUID(), 'Nhóm / Phòng ban', 'pi pi-sitemap', '/groups', 2, @hrParentId, TRUE, NOW());

-- Cấp 2: Menu con của "Hệ thống"
SET @systemParentId = (SELECT Id FROM Menus WHERE Name = 'Hệ thống' LIMIT 1);

INSERT INTO Menus (Id, Name, Icon, Route, DisplayOrder, ParentId, IsActive, CreatedAt) VALUES
(UUID(), 'Vai trò', 'pi pi-shield', '/roles', 1, @systemParentId, TRUE, NOW()),
(UUID(), 'Quản lý Menu', 'pi pi-bars', '/menus', 2, @systemParentId, TRUE, NOW()),
(UUID(), 'Phân quyền Menu', 'pi pi-key', '/roles/menu-permissions', 3, @systemParentId, TRUE, NOW());

/*
 * Bước 7b: Tạo AssetCategories (Phân loại tài sản)
 * Các loại tài sản cơ bản trong hệ thống
 */
INSERT INTO AssetCategories (Id, Name, Description, CreatedAt) VALUES
(UUID(), 'Máy tính & Laptop', 'Máy tính để bàn, laptop, máy trạm', NOW()),
(UUID(), 'Thiết bị văn phòng', 'Máy in, máy scan, máy photocopy', NOW()),
(UUID(), 'Thiết bị mạng', 'Router, switch, access point, server', NOW()),
(UUID(), 'Phương tiện vận tải', 'Xe ô tô, xe máy, xe tải', NOW()),
(UUID(), 'Nội thất', 'Bàn, ghế, tủ, kệ', NOW()),
(UUID(), 'Thiết bị y tế', 'Máy đo, dụng cụ y tế', NOW()),
(UUID(), 'Thiết bị điện', 'Máy phát điện, UPS, điều hòa', NOW());

/*
 * Bước 8: Gán permissions cho các menu
 * Mỗi menu cần gán CRUD permissions tương ứng
 * PermissionType: View, Create, Edit, Delete
 */

-- 8a: Menu "Tổng quan" - chỉ cần View
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p
WHERE m.Name = 'Tổng quan' AND p.Name = 'Users.View';

-- 8b: Menu "Danh sách Tài sản" - CRUD đầy đủ
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Danh sách Tài sản' AND p.Name = 'Assets.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Create', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Danh sách Tài sản' AND p.Name = 'Assets.Create'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Danh sách Tài sản' AND p.Name = 'Assets.Edit'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Delete', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Danh sách Tài sản' AND p.Name = 'Assets.Delete';

-- 8c: Menu "Người dùng" - CRUD đầy đủ
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Người dùng' AND p.Name = 'Users.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Create', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Người dùng' AND p.Name = 'Users.Create'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Người dùng' AND p.Name = 'Users.Edit'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Delete', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Người dùng' AND p.Name = 'Users.Delete';

-- 8d: Menu "Nhóm / Phòng ban" - CRUD đầy đủ
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Nhóm / Phòng ban' AND p.Name = 'Groups.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Create', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Nhóm / Phòng ban' AND p.Name = 'Groups.Create'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Nhóm / Phòng ban' AND p.Name = 'Groups.Edit'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Delete', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Nhóm / Phòng ban' AND p.Name = 'Groups.Delete';

-- 8e: Menu "Vai trò" - CRUD đầy đủ
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Vai trò' AND p.Name = 'Roles.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Create', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Vai trò' AND p.Name = 'Roles.Create'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Vai trò' AND p.Name = 'Roles.Edit'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Delete', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Vai trò' AND p.Name = 'Roles.Delete';

-- 8f: Menu "Quản lý Menu" - CRUD đầy đủ
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Quản lý Menu' AND p.Name = 'Menus.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Create', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Quản lý Menu' AND p.Name = 'Menus.Create'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Quản lý Menu' AND p.Name = 'Menus.Edit'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Delete', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Quản lý Menu' AND p.Name = 'Menus.Delete';

-- 8g: Menu "Phân quyền Menu" - View + Edit
INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Phân quyền Menu' AND p.Name = 'Roles.View'
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m CROSS JOIN Permissions p WHERE m.Name = 'Phân quyền Menu' AND p.Name = 'Roles.Edit';

/* Bước 14: Tạo Groups (Nhóm người dùng - Phòng ban)
 * Ví dụ: Phòng IT, Phòng Kỹ thuật, Phòng Hành chính
 */
SET @itGroupId = UUID();
SET @techGroupId = UUID();
SET @adminGroupId = UUID();

INSERT INTO `Groups` (Id, Name, Description, IsActive, CreatedAt) VALUES
(@itGroupId, 'Phòng CNTT', 'Phòng Công nghệ thông tin', TRUE, NOW()),
(@techGroupId, 'Phòng Kỹ thuật', 'Phòng Kỹ thuật bảo trì', TRUE, NOW()),
(@adminGroupId, 'Phòng Hành chính', 'Phòng Hành chính - Nhân sự', TRUE, NOW());

/* Bước 15: Gán permissions cho Group "Phòng CNTT"
 * Phòng CNTT có quyền: Users.View, Assets.View, Assets.Create, Assets.Edit
 */
INSERT INTO GroupPermissions (GroupId, PermissionId, AssignedAt)
SELECT g.Id, p.Id, NOW()
FROM (SELECT @itGroupId AS Id) g
CROSS JOIN Permissions p
WHERE p.Name IN ('Users.View', 'Assets.View', 'Assets.Create', 'Assets.Edit');

/* Bước 16: Gán menu permissions cho Group "Phòng CNTT"
 * Phòng CNTT thấy được: Tổng quan, Danh sách Tài sản, Người dùng
 */
INSERT INTO GroupMenuPermissions (Id, GroupId, MenuId, PermissionId, PermissionType, AssignedAt)
SELECT UUID(), g.Id, m.Id, p.Id, 'View', NOW()
FROM (SELECT @itGroupId AS Id) g
CROSS JOIN Menus m
CROSS JOIN Permissions p
WHERE m.Name IN ('Tổng quan', 'Danh sách Tài sản', 'Người dùng') AND p.Name IN ('Users.View', 'Assets.View');

/* Bước 17: Gán permissions cho Group "Phòng Kỹ thuật"
 * Phòng Kỹ thuật có quyền: Assets.View, Assets.Edit
 */
INSERT INTO GroupPermissions (GroupId, PermissionId, AssignedAt)
SELECT g.Id, p.Id, NOW()
FROM (SELECT @techGroupId AS Id) g
CROSS JOIN Permissions p
WHERE p.Name IN ('Assets.View', 'Assets.Edit');

/* Bước 18: Gán menu permissions cho Group "Phòng Kỹ thuật"
 * Phòng Kỹ thuật chỉ thấy: Danh sách Tài sản
 */
INSERT INTO GroupMenuPermissions (Id, GroupId, MenuId, PermissionId, PermissionType, AssignedAt)
SELECT UUID(), g.Id, m.Id, p.Id, 'View', NOW()
FROM (SELECT @techGroupId AS Id) g
CROSS JOIN Menus m
CROSS JOIN Permissions p
WHERE m.Name = 'Danh sách Tài sản' AND p.Name = 'Assets.View';

/* Bước 19: Gán permissions cho Group "Phòng Hành chính"
 * Phòng Hành chính có quyền: Users.View, Groups.View
 */
INSERT INTO GroupPermissions (GroupId, PermissionId, AssignedAt)
SELECT g.Id, p.Id, NOW()
FROM (SELECT @adminGroupId AS Id) g
CROSS JOIN Permissions p
WHERE p.Name IN ('Users.View', 'Groups.View');

/* Bước 20: Gán menu permissions cho Group "Phòng Hành chính"
 * Phòng Hành chính thấy: Tổng quan, Người dùng, Nhóm / Phòng ban
 */
INSERT INTO GroupMenuPermissions (Id, GroupId, MenuId, PermissionId, PermissionType, AssignedAt)
SELECT UUID(), g.Id, m.Id, p.Id, 'View', NOW()
FROM (SELECT @adminGroupId AS Id) g
CROSS JOIN Menus m
CROSS JOIN Permissions p
WHERE m.Name IN ('Tổng quan', 'Người dùng', 'Nhóm / Phòng ban') AND p.Name IN ('Users.View', 'Groups.View');

-- ============================================
-- HOÀN THÀNH! Kiểm tra dữ liệu đã seed
-- ============================================
SELECT '========================================' AS '';
SELECT 'DATABASE SCHEMA + SEED DATA COMPLETED!' AS '';
SELECT '========================================' AS '';

SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Roles', COUNT(*) FROM Roles
UNION ALL
SELECT 'Permissions', COUNT(*) FROM Permissions
UNION ALL
SELECT 'UserRoles', COUNT(*) FROM UserRoles
UNION ALL
SELECT 'RolePermissions', COUNT(*) FROM RolePermissions
UNION ALL
SELECT 'Groups', COUNT(*) FROM `Groups`
UNION ALL
SELECT 'UserGroups', COUNT(*) FROM UserGroups
UNION ALL
SELECT 'GroupPermissions', COUNT(*) FROM GroupPermissions
UNION ALL
SELECT 'AssetCategories', COUNT(*) FROM AssetCategories
UNION ALL
SELECT 'Menus', COUNT(*) FROM Menus
UNION ALL
SELECT 'MenuPermissions', COUNT(*) FROM MenuPermissions
UNION ALL
SELECT 'GroupMenuPermissions', COUNT(*) FROM GroupMenuPermissions;

SELECT '========================================' AS '';
SELECT 'ADMIN LOGIN CREDENTIALS:' AS '';
SELECT 'Username: admin' AS '';
SELECT 'Password: Admin@123' AS '';
SELECT 'Email: admin@qlts.vn' AS '';
SELECT 'Role: SuperAdmin (Full Access)' AS '';
SELECT '========================================' AS '';
SELECT 'Next Steps:' AS '';
SELECT '1. Start backend API: cd backend/src/Api && dotnet run' AS '';
SELECT '2. Start frontend: cd frontend && npm start' AS '';
SELECT '3. Login at http://localhost:4200/login' AS '';
SELECT '4. Verify dynamic menu appears based on permissions' AS '';
SELECT '========================================' AS '';

-- ============================================
-- PHẦN 3: MIGRATION (cho database đã tồn tại)
-- ============================================
-- Nếu database đã được tạo trước khi có các cột mới,
-- chạy các lệnh ALTER bên dưới để đồng bộ schema.
-- Nếu tạo mới từ đầu thì KHÔNG cần chạy phần này.
-- ============================================

/*
-- MenuPermissions: Thiếu cột UpdatedAt (BaseEntity)
-- Đây là nguyên nhân lỗi không lưu được quyền menu
ALTER TABLE MenuPermissions ADD COLUMN UpdatedAt DATETIME NULL;

-- GroupPermissions: Thiếu cột Id (Entity có property Id, PK vẫn là composite)
ALTER TABLE GroupPermissions ADD COLUMN Id CHAR(36) NOT NULL DEFAULT (UUID()) FIRST;
*/

-- ============================================
-- MAINTENANCE QUERIES (Chạy định kỳ)
-- ============================================

/*
-- Cleanup expired refresh tokens (chạy hàng tháng)
DELETE FROM RefreshTokens
WHERE ExpiresAt < NOW() OR IsRevoked = TRUE;

-- Archive old depreciation records (chạy hàng năm)
-- CẢNH BÁO: Backup trước khi xóa!
DELETE FROM AssetDepreciations
WHERE Year < YEAR(NOW()) - 5;

-- Query để lấy menu của user từ ROLE (dùng trong backend)
SELECT DISTINCT m.*
FROM Menus m
INNER JOIN MenuPermissions mp ON m.Id = mp.MenuId
INNER JOIN RolePermissions rp ON mp.PermissionId = rp.PermissionId
INNER JOIN UserRoles ur ON rp.RoleId = ur.RoleId
WHERE ur.UserId = 'USER_ID_HERE'
  AND mp.PermissionType = 'View'
  AND m.IsActive = 1
ORDER BY m.DisplayOrder;

-- Query để lấy menu của user từ GROUP (dùng trong backend)
SELECT DISTINCT m.*
FROM Menus m
INNER JOIN GroupMenuPermissions gmp ON m.Id = gmp.MenuId
INNER JOIN UserGroups ug ON gmp.GroupId = ug.GroupId
WHERE ug.UserId = 'USER_ID_HERE'
  AND gmp.PermissionType = 'View'
  AND m.IsActive = 1
ORDER BY m.DisplayOrder;
*/

-- ============================================
-- KẾT THÚC FILE
-- ============================================
