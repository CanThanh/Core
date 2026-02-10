using BuildingBlocks.Common.Abstractions;

namespace Assets.Features.CreateAsset;

public record CreateAssetCommand(
    string Code,
    string Name,
    Guid CategoryId,
    string? Manufacturer,
    string? SerialNumber,
    decimal PurchasePrice,
    DateTime PurchaseDate,
    decimal DepreciationRate,
    string? Location
) : ICommand<CreateAssetResponse>;

public record CreateAssetResponse(
    Guid Id,
    string Code,
    string Name
);
