using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;

namespace Assets.Features.GetAssets;

public record GetAssetsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    string? Status = null
) : IQuery<PagedResult<AssetDto>>;

public record AssetDto(
    Guid Id,
    string Code,
    string Name,
    string CategoryName,
    decimal PurchasePrice,
    decimal DepreciationRate,
    string Status,
    DateTime PurchaseDate,
    string? Location
);
