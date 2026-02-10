using Assets.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Features.GetAssets;

public class GetAssetsQueryHandler : IQueryHandler<GetAssetsQuery, PagedResult<AssetDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAssetsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AssetDto>>> Handle(
        GetAssetsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Asset>()
            .Include(a => a.Category)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a =>
                a.Code.Contains(request.SearchTerm) ||
                a.Name.Contains(request.SearchTerm) ||
                (a.SerialNumber != null && a.SerialNumber.Contains(request.SearchTerm)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var assets = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AssetDto(
                a.Id,
                a.Code,
                a.Name,
                a.Category.Name,
                a.PurchasePrice,
                a.DepreciationRate,
                a.Status,
                a.PurchaseDate,
                a.Location
            ))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AssetDto>
        {
            Items = assets,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result.Success(result);
    }
}
