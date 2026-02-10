using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.UpdateMenu;

public class UpdateMenuCommandHandler : ICommandHandler<UpdateMenuCommand, UpdateMenuResponse>
{
    private readonly ApplicationDbContext _context;

    public UpdateMenuCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateMenuResponse>> Handle(
        UpdateMenuCommand request,
        CancellationToken cancellationToken)
    {
        var menu = await _context.Set<Menu>()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menu == null)
        {
            return Result.Failure<UpdateMenuResponse>(
                $"Menu with ID {request.Id} not found");
        }

        // Validate parent exists if provided and is not self-reference
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
            {
                return Result.Failure<UpdateMenuResponse>(
                    "Menu cannot be its own parent");
            }

            var parentExists = await _context.Set<Menu>()
                .AnyAsync(m => m.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                return Result.Failure<UpdateMenuResponse>(
                    $"Parent menu with ID {request.ParentId} not found");
            }
        }

        menu.Name = request.Name;
        menu.Icon = request.Icon;
        menu.Route = request.Route;
        menu.DisplayOrder = request.DisplayOrder;
        menu.IsActive = request.IsActive;
        menu.ParentId = request.ParentId;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateMenuResponse(
            menu.Id,
            menu.Name,
            menu.Route
        );

        return Result.Success(response);
    }
}
