using System;

namespace Menus.Entities;

public class GroupMenuPermission
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid MenuId { get; set; }
    public Guid PermissionId { get; set; }
    public string PermissionType { get; set; } = string.Empty; // View, Create, Edit, Delete
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Menu Menu { get; set; } = null!;
    public MenuPermission MenuPermission { get; set; } = null!;
}
