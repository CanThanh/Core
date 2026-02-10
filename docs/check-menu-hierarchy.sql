-- ============================================
-- KIỂM TRA MENU HIERARCHY
-- ============================================
USE CORE;

SELECT '========================================' AS '';
SELECT 'MENU HIERARCHY - ROOT MENUS' AS '';
SELECT '========================================' AS '';

-- Root menus (no parent)
SELECT
    m.Id,
    m.Name,
    m.Icon,
    m.Route,
    m.DisplayOrder,
    m.IsActive,
    (SELECT COUNT(*) FROM Menus child WHERE child.ParentId = m.Id) AS ChildCount
FROM Menus m
WHERE m.ParentId IS NULL
ORDER BY m.DisplayOrder;

SELECT '========================================' AS '';
SELECT 'MENU HIERARCHY - CHILD MENUS' AS '';
SELECT '========================================' AS '';

-- Child menus (with parent)
SELECT
    m.Id,
    m.Name,
    m.Icon,
    m.Route,
    m.DisplayOrder,
    m.IsActive,
    parent.Name AS ParentMenuName,
    parent.Id AS ParentId
FROM Menus m
INNER JOIN Menus parent ON m.ParentId = parent.Id
ORDER BY parent.DisplayOrder, m.DisplayOrder;

SELECT '========================================' AS '';
SELECT 'SETTINGS MENU - DETAILED' AS '';
SELECT '========================================' AS '';

-- Check Settings menu specifically
SELECT
    'Parent (Settings)' AS Type,
    m.Id,
    m.Name,
    m.Icon,
    m.Route,
    m.DisplayOrder,
    m.IsActive
FROM Menus m
WHERE m.Name = 'Settings'

UNION ALL

SELECT
    'Child' AS Type,
    child.Id,
    child.Name,
    child.Icon,
    child.Route,
    child.DisplayOrder,
    child.IsActive
FROM Menus parent
INNER JOIN Menus child ON child.ParentId = parent.Id
WHERE parent.Name = 'Settings'
ORDER BY DisplayOrder;

SELECT '========================================' AS '';
SELECT 'MENU PERMISSIONS CHECK' AS '';
SELECT '========================================' AS '';

-- Check which menus have permissions for admin
SELECT DISTINCT
    m.Name AS MenuName,
    m.Route,
    parent.Name AS ParentMenu,
    COUNT(DISTINCT mp.Id) AS PermissionCount
FROM Menus m
LEFT JOIN Menus parent ON m.ParentId = parent.Id
LEFT JOIN MenuPermissions mp ON m.Id = mp.MenuId
LEFT JOIN RolePermissions rp ON mp.PermissionId = rp.PermissionId
LEFT JOIN UserRoles ur ON rp.RoleId = ur.RoleId
LEFT JOIN Users u ON ur.UserId = u.Id
WHERE u.Username = 'admin' OR u.Username IS NULL
GROUP BY m.Id, m.Name, m.Route, parent.Name
ORDER BY parent.Name, m.Name;
