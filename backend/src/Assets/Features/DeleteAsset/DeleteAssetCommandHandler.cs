using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.DeleteAsset;

public class DeleteAssetCommandHandler : ICommandHandler<DeleteAssetCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteAssetCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Set<Asset>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset is null)
        {
            return Result.Failure("Asset not found");
        }

        _context.Set<Asset>().Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
