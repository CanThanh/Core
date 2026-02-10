-- ============================================
-- FIX ADMIN MENU PERMISSIONS
-- Thêm menu permissions cho user admin
-- ============================================
-- Chạy script này nếu user admin đăng nhập không thấy menu
-- ============================================

USE CORE;

-- ============================================
-- Bước 1: Gán permission cho menu Settings (parent menu)
-- Settings menu cần có ít nhất 1 permission để hiển thị
-- Sử dụng Menus.View để user có quyền xem Settings
-- ============================================

INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m
CROSS JOIN Permissions p
WHERE m.Name = 'Settings' AND p.Name = 'Menus.View'
AND NOT EXISTS (
    SELECT 1 FROM MenuPermissions mp
    WHERE mp.MenuId = m.Id AND mp.PermissionId = p.Id AND mp.PermissionType = 'View'
);

-- ============================================
-- Bước 2: Gán permission cho submenu "Role Menu Permissions"
-- Menu này cho phép quản lý phân quyền menu cho role
-- ============================================

INSERT INTO MenuPermissions (Id, MenuId, PermissionId, PermissionType, CreatedAt)
SELECT UUID(), m.Id, p.Id, 'View', NOW()
FROM Menus m
CROSS JOIN Permissions p
WHERE m.Name = 'Role Menu Permissions' AND p.Name = 'Menus.View'
AND NOT EXISTS (
    SELECT 1 FROM MenuPermissions mp
    WHERE mp.MenuId = m.Id AND mp.PermissionId = p.Id AND mp.PermissionType = 'View'
)
UNION ALL
SELECT UUID(), m.Id, p.Id, 'Edit', NOW()
FROM Menus m
CROSS JOIN Permissions p
WHERE m.Name = 'Role Menu Permissions' AND p.Name = 'Menus.Edit'
AND NOT EXISTS (
    SELECT 1 FROM MenuPermissions mp
    WHERE mp.MenuId = m.Id AND mp.PermissionId = p.Id AND mp.PermissionType = 'Edit'
);

-- ============================================
-- Bước 3: Kiểm tra kết quả - Liệt kê tất cả menu mà admin có quyền xem
-- ============================================

SELECT '========================================' AS '';
SELECT 'MENU PERMISSIONS FOR ADMIN USER' AS '';
SELECT '========================================' AS '';

-- Lấy danh sách menu mà admin user có quyền xem (thông qua role)
SELECT DISTINCT
    m.Name AS MenuName,
    m.Route AS Route,
    m.Icon AS Icon,
    m.DisplayOrder AS DisplayOrder,
    parent.Name AS ParentMenu,
    mp.PermissionType AS PermissionType,
    p.Name AS RequiredPermission
FROM Menus m
LEFT JOIN Menus parent ON m.ParentId = parent.Id
INNER JOIN MenuPermissions mp ON m.Id = mp.MenuId
INNER JOIN Permissions p ON mp.PermissionId = p.Id
INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
INNER JOIN Roles r ON rp.RoleId = r.Id
WHERE r.Name = 'SuperAdmin'
    AND m.IsActive = TRUE
    AND mp.PermissionType = 'View'
ORDER BY
    COALESCE(parent.DisplayOrder, m.DisplayOrder),
    m.DisplayOrder;

SELECT '========================================' AS '';
SELECT 'Tổng số menu hiển thị cho admin:' AS '';
SELECT '========================================' AS '';

SELECT COUNT(DISTINCT m.Id) AS TotalMenus
FROM Menus m
INNER JOIN MenuPermissions mp ON m.Id = mp.MenuId
INNER JOIN Permissions p ON mp.PermissionId = p.Id
INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
INNER JOIN Roles r ON rp.RoleId = r.Id
WHERE r.Name = 'SuperAdmin'
    AND m.IsActive = TRUE
    AND mp.PermissionType = 'View';

SELECT '========================================' AS '';
SELECT 'Script executed successfully!' AS '';
SELECT 'Please logout and login again to see the updated menu' AS '';
SELECT '========================================' AS '';

-- ============================================
-- KẾT THÚC
-- ============================================
