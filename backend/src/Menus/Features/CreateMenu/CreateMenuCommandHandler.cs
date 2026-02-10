using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.CreateMenu;

public class CreateMenuCommandHandler : ICommandHandler<CreateMenuCommand, CreateMenuResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateMenuCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateMenuResponse>> Handle(
        CreateMenuCommand request,
        CancellationToken cancellationToken)
    {
        // Validate parent exists if provided
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Set<Menu>()
                .AnyAsync(m => m.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                return Result.Failure<CreateMenuResponse>(
                    $"Parent menu with ID {request.ParentId} not found");
            }
        }

        var menu = new Menu
        {
            Name = request.Name,
            Icon = request.Icon,
            Route = request.Route,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            ParentId = request.ParentId
        };

        _context.Set<Menu>().Add(menu);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateMenuResponse(
            menu.Id,
            menu.Name,
            menu.Route,
            menu.DisplayOrder
        );

        return Result.Success(response);
    }
}
