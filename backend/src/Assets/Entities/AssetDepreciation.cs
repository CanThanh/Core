using BuildingBlocks.Common.Models;

namespace Assets.Entities;

public class AssetDepreciation
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Asset Asset { get; set; } = null!;
}
