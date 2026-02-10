using BuildingBlocks.Common.Abstractions;

namespace Users.Features.DeleteGroup;

public record DeleteGroupCommand(Guid GroupId) : ICommand<bool>;
