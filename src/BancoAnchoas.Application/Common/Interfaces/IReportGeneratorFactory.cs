using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Application.Common.Interfaces;

public interface IReportGeneratorFactory
{
    IReportGenerator Create(ReportFormat format);
}
