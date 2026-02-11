using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.GetAssetCategories;

public class GetAssetCategoriesQueryHandler : IQueryHandler<GetAssetCategoriesQuery, List<AssetCategoryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAssetCategoriesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AssetCategoryDto>>> Handle(
        GetAssetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _context.Set<AssetCategory>()
            .OrderBy(c => c.Name)
            .Select(c => new AssetCategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);

        return Result.Success(categories);
    }
}
