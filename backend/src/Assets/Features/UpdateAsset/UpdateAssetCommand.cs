using BuildingBlocks.Common.Abstractions;

namespace Assets.Features.UpdateAsset;

public record UpdateAssetCommand(
    Guid Id,
    string Name,
    Guid CategoryId,
    string? Manufacturer,
    string? SerialNumber,
    decimal PurchasePrice,
    DateTime PurchaseDate,
    decimal DepreciationRate,
    string? Location,
    string Status,
    bool IsActive
) : ICommand<UpdateAssetResponse>;

public record UpdateAssetResponse(
    Guid Id,
    string Code,
    string Name
);
