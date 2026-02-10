using BuildingBlocks.Common.Models;

namespace Assets.Entities;

public class Asset : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal DepreciationRate { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = "InUse"; // InUse, Maintenance, Broken, Disposed
    public bool IsActive { get; set; } = true;

    public AssetCategory Category { get; set; } = null!;
    public ICollection<AssetDepreciation> Depreciations { get; set; } = [];
}
