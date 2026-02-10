using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.GetAssetById;

public class GetAssetByIdQueryHandler : IQueryHandler<GetAssetByIdQuery, AssetDetailDto>
{
    private readonly ApplicationDbContext _context;

    public GetAssetByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AssetDetailDto>> Handle(
        GetAssetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Set<Asset>()
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset is null)
        {
            return Result.Failure<AssetDetailDto>("Asset not found");
        }

        var dto = new AssetDetailDto(
            asset.Id,
            asset.Code,
            asset.Name,
            asset.CategoryId,
            asset.Category.Name,
            asset.Manufacturer,
            asset.SerialNumber,
            asset.PurchasePrice,
            asset.PurchaseDate,
            asset.DepreciationRate,
            asset.Location,
            asset.Status,
            asset.IsActive,
            asset.CreatedAt,
            asset.UpdatedAt
        );

        return Result.Success(dto);
    }
}
