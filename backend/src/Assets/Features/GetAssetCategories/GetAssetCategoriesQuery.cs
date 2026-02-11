using BuildingBlocks.Common.Abstractions;

namespace Assets.Features.GetAssetCategories;

public record GetAssetCategoriesQuery : IQuery<List<AssetCategoryDto>>;

public record AssetCategoryDto(
    Guid Id,
    string Name,
    string? Description
);
