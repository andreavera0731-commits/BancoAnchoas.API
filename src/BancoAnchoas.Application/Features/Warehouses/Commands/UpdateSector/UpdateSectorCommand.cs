using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.UpdateSector;

public record UpdateSectorCommand(int Id, string Name, List<int>? CategoryIds = null) : IRequest;

public class UpdateSectorCommandValidator : FluentValidation.AbstractValidator<UpdateSectorCommand>
{
    public UpdateSectorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.CategoryIds).GreaterThan(0).When(x => x.CategoryIds is not null);
    }
}

public class UpdateSectorCommandHandler : IRequestHandler<UpdateSectorCommand>
{
    private readonly IRepository<Sector> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSectorCommandHandler(IRepository<Sector> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSectorCommand request, CancellationToken ct)
    {
        var sector = await _repository.Query()
            .Include(s => s.SectorCategories)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Sector), request.Id);

        sector.Name = request.Name;
        sector.UpdatedAt = DateTime.UtcNow;

        if (request.CategoryIds is not null)
        {
            sector.SectorCategories.Clear();

            foreach (var categoryId in request.CategoryIds.Distinct())
            {
                sector.SectorCategories.Add(new SectorCategory
                {
                    SectorId = sector.Id,
                    CategoryId = categoryId
                });
            }
        }

        _repository.Update(sector);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
