using BuildingBlocks.Common.Models;

namespace Assets.Entities;

public class AssetCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = [];
}
