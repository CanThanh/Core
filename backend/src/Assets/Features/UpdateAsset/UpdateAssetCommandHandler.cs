using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.UpdateAsset;

public class UpdateAssetCommandHandler : ICommandHandler<UpdateAssetCommand, UpdateAssetResponse>
{
    private readonly ApplicationDbContext _context;

    public UpdateAssetCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateAssetResponse>> Handle(
        UpdateAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Set<Asset>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset is null)
        {
            return Result.Failure<UpdateAssetResponse>("Asset not found");
        }

        // Check if category exists
        var categoryExists = await _context.Set<AssetCategory>()
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<UpdateAssetResponse>("Category not found");
        }

        asset.Name = request.Name;
        asset.CategoryId = request.CategoryId;
        asset.Manufacturer = request.Manufacturer;
        asset.SerialNumber = request.SerialNumber;
        asset.PurchasePrice = request.PurchasePrice;
        asset.PurchaseDate = request.PurchaseDate;
        asset.DepreciationRate = request.DepreciationRate;
        asset.Location = request.Location;
        asset.Status = request.Status;
        asset.IsActive = request.IsActive;
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateAssetResponse(asset.Id, asset.Code, asset.Name);

        return Result.Success(response);
    }
}
