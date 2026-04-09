using BancoAnchoas.Application.Features.Stock.DTOs;

namespace BancoAnchoas.Application.Common.Interfaces;

public interface IReportGenerator
{
    byte[] Generate(IReadOnlyList<StockMovementDto> movements);
    string ContentType { get; }
    string FileExtension { get; }
}
