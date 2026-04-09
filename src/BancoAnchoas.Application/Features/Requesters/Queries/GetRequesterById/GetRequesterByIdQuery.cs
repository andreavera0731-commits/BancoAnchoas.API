using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Requesters.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Queries.GetRequesterById;

public record GetRequesterByIdQuery(int Id) : IRequest<RequesterDto>;

public class GetRequesterByIdQueryHandler : IRequestHandler<GetRequesterByIdQuery, RequesterDto>
{
    private readonly IRepository<Requester> _repository;
    private readonly IMapper _mapper;

    public GetRequesterByIdQueryHandler(IRepository<Requester> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RequesterDto> Handle(GetRequesterByIdQuery request, CancellationToken ct)
    {
        var requester = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Requester), request.Id);

        return _mapper.Map<RequesterDto>(requester);
    }
}
