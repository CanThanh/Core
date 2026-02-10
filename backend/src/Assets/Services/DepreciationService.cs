using Assets.Entities;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Assets.Services;

/// <summary>
/// Tính khấu hao tài sản theo phương pháp đường thẳng (Straight-line depreciation)
/// Theo Thông tư 45/2013/TT-BTC và Thông tư 200/2014/TT-BTC
/// </summary>
public class DepreciationService
{
    private readonly ApplicationDbContext _context;

    public DepreciationService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tính khấu hao hàng tháng cho tài sản
    /// Công thức: Khấu hao tháng = Nguyên giá × Tỷ lệ khấu hao năm / 12
    /// </summary>
    public async Task<AssetDepreciation> CalculateMonthlyDepreciationAsync(
        Guid assetId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var asset = await _context.Set<Asset>()
            .Include(a => a.Category)
            .Include(a => a.Depreciations)
            .FirstOrDefaultAsync(a => a.Id == assetId, cancellationToken)
            ?? throw new InvalidOperationException($"Asset {assetId} not found");

        // Khấu hao tháng = Nguyên giá × (Tỷ lệ khấu hao năm / 100) / 12
        var monthlyDepreciationAmount = asset.PurchasePrice * (asset.DepreciationRate / 100m) / 12m;

        // Tính tổng khấu hao lũy kế
        var previousDepreciation = asset.Depreciations
            .Where(d => d.Year < year || (d.Year == year && d.Month < month))
            .OrderByDescending(d => d.Year)
            .ThenByDescending(d => d.Month)
            .FirstOrDefault();

        var accumulatedDepreciation = (previousDepreciation?.AccumulatedDepreciation ?? 0m) + monthlyDepreciationAmount;

        // Giá trị còn lại = Nguyên giá - Khấu hao lũy kế
        var bookValue = asset.PurchasePrice - accumulatedDepreciation;

        // Đảm bảo giá trị còn lại không âm
        if (bookValue < 0)
        {
            bookValue = 0;
            accumulatedDepreciation = asset.PurchasePrice;
            monthlyDepreciationAmount = asset.PurchasePrice - (previousDepreciation?.AccumulatedDepreciation ?? 0m);
        }

        var depreciation = new AssetDepreciation
        {
            AssetId = assetId,
            Year = year,
            Month = month,
            DepreciationAmount = Math.Round(monthlyDepreciationAmount, 2),
            AccumulatedDepreciation = Math.Round(accumulatedDepreciation, 2),
            BookValue = Math.Round(bookValue, 2)
        };

        return depreciation;
    }

    /// <summary>
    /// Tính khấu hao cho tất cả tài sản trong một tháng cụ thể
    /// </summary>
    public async Task CalculateDepreciationForAllAssetsAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var assets = await _context.Set<Asset>()
            .Include(a => a.Category)
            .Include(a => a.Depreciations)
            .Where(a => a.Status != "Disposed")
            .ToListAsync(cancellationToken);

        var depreciations = new List<AssetDepreciation>();

        foreach (var asset in assets)
        {
            // Chỉ tính khấu hao nếu tài sản đã được mua trước hoặc trong tháng này
            var purchaseDate = asset.PurchaseDate;
            if (purchaseDate.Year > year || (purchaseDate.Year == year && purchaseDate.Month > month))
            {
                continue;
            }

            // Kiểm tra xem đã tính khấu hao cho tháng này chưa
            var exists = await _context.Set<AssetDepreciation>()
                .AnyAsync(d => d.AssetId == asset.Id && d.Year == year && d.Month == month, cancellationToken);

            if (exists)
            {
                continue;
            }

            var depreciation = await CalculateMonthlyDepreciationAsync(asset.Id, year, month, cancellationToken);
            depreciations.Add(depreciation);
        }

        if (depreciations.Any())
        {
            _context.Set<AssetDepreciation>().AddRange(depreciations);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
