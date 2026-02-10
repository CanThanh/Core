using BuildingBlocks.Common.Results;
using MediatR;

namespace BuildingBlocks.Common.Abstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
