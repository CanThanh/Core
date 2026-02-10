using Authorization.Entities;
using BuildingBlocks.Common.Models;

namespace Menus.Entities;

public class MenuPermission : BaseEntity
{
    public Guid MenuId { get; set; }
    public string PermissionType { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }

    // Navigation properties
    public Menu Menu { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
