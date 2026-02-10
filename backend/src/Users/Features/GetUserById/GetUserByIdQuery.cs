using BuildingBlocks.Common.Abstractions;
using Users.Features.GetUsers;

namespace Users.Features.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
