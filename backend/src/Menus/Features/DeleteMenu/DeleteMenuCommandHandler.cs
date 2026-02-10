using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.DeleteMenu;

public class DeleteMenuCommandHandler : ICommandHandler<DeleteMenuCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteMenuCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeleteMenuCommand request,
        CancellationToken cancellationToken)
    {
        var menu = await _context.Set<Menu>()
            .Include(m => m.Children)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menu == null)
        {
            return Result.Failure<bool>(
                $"Menu with ID {request.Id} not found");
        }

        // Check if menu has children
        if (menu.Children.Any())
        {
            return Result.Failure<bool>(
                "Cannot delete menu with children. Delete or reassign children first.");
        }

        _context.Set<Menu>().Remove(menu);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
