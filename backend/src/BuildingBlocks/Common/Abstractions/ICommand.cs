using BuildingBlocks.Common.Results;
using MediatR;

namespace BuildingBlocks.Common.Abstractions;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
