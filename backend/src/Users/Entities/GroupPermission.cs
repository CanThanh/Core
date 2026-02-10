using System;
using Authorization.Entities;

namespace Users.Entities;

public class GroupPermission
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Group Group { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
