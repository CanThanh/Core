using BuildingBlocks.Common.Models;

namespace Users.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserGroup> UserGroups { get; set; } = [];
}
