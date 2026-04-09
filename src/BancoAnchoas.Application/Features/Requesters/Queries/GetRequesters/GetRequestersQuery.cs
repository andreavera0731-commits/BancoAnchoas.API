using BancoAnchoas.Application.Features.Requesters.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Requesters.Queries.GetRequesters;

public record GetRequestersQuery : IRequest<IReadOnlyList<RequesterDto>>;

public class GetRequestersQueryHandler : IRequestHandler<GetRequestersQuery, IReadOnlyList<RequesterDto>>
{
    private readonly IRepository<Requester> _repository;
    private readonly IMapper _mapper;

    public GetRequestersQueryHandler(IRepository<Requester> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RequesterDto>> Handle(GetRequestersQuery request, CancellationToken ct)
    {
        return await _repository.Query()
            .OrderBy(r => r.Name)
            .ProjectToType<RequesterDto>(_mapper.Config)
            .ToListAsync(ct);
    }
}
