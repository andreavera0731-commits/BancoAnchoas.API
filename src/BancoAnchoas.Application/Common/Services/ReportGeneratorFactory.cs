using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Application.Common.Services;

public class ReportGeneratorFactory : IReportGeneratorFactory
{
    private readonly IEnumerable<IReportGenerator> _generators;

    public ReportGeneratorFactory(IEnumerable<IReportGenerator> generators)
        => _generators = generators;

    public IReportGenerator Create(ReportFormat format)
    {
        var extension = format switch
        {
            ReportFormat.Csv => "csv",
            ReportFormat.Excel => "xlsx",
            ReportFormat.Pdf => "pdf",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return _generators.FirstOrDefault(g => g.FileExtension == extension)
            ?? throw new InvalidOperationException($"No report generator registered for format '{format}'.");
    }
}
