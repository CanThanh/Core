using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.CreateAsset;

public class CreateAssetCommandHandler : ICommandHandler<CreateAssetCommand, CreateAssetResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateAssetCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateAssetResponse>> Handle(
        CreateAssetCommand request,
        CancellationToken cancellationToken)
    {
        // Check if asset code already exists
        var exists = await _context.Set<Asset>()
            .AnyAsync(a => a.Code == request.Code, cancellationToken);

        if (exists)
        {
            return Result.Failure<CreateAssetResponse>($"Asset code '{request.Code}' already exists");
        }

        // Verify category exists
        var categoryExists = await _context.Set<AssetCategory>()
            .AnyAsync(ac => ac.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<CreateAssetResponse>("Asset category not found");
        }

        var asset = new Asset
        {
            Code = request.Code,
            Name = request.Name,
            CategoryId = request.CategoryId,
            Manufacturer = request.Manufacturer,
            SerialNumber = request.SerialNumber,
            PurchasePrice = request.PurchasePrice,
            PurchaseDate = request.PurchaseDate,
            DepreciationRate = request.DepreciationRate,
            Location = request.Location,
            Status = "InUse"
        };

        _context.Set<Asset>().Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateAssetResponse(asset.Id, asset.Code, asset.Name));
    }
}
