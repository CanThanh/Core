using BuildingBlocks.Common.Models;

namespace Menus.Entities;

public class Menu : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }

    // Navigation properties
    public Menu? Parent { get; set; }
    public ICollection<Menu> Children { get; set; } = [];
    public ICollection<MenuPermission> MenuPermissions { get; set; } = [];
}
