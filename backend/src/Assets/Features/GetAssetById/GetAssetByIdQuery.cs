using BuildingBlocks.Common.Abstractions;

namespace Assets.Features.GetAssetById;

public record GetAssetByIdQuery(Guid Id) : IQuery<AssetDetailDto>;

public record AssetDetailDto(
    Guid Id,
    string Code,
    string Name,
    Guid CategoryId,
    string CategoryName,
    string? Manufacturer,
    string? SerialNumber,
    decimal PurchasePrice,
    DateTime PurchaseDate,
    decimal DepreciationRate,
    string? Location,
    string Status,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
