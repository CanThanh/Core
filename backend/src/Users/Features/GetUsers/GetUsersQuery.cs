using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Models;

namespace Users.Features.GetUsers;

public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    Guid? GroupId = null
) : IQuery<PagedResult<UserDto>>;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    List<string> Roles,
    List<string> Groups
);
