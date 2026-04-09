using BancoAnchoas.Application.Common.Services;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Domain.Enums;
using FluentAssertions;
using QuestPDF.Infrastructure;

namespace BancoAnchoas.Application.Tests.Reports;

public class ReportGeneratorTests
{
    public ReportGeneratorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static List<StockMovementDto> CreateSampleMovements() =>
    [
        new StockMovementDto
        {
            Id = 1,
            Quantity = 50,
            Type = MovementType.Entry,
            ProductId = 1,
            ProductName = "Anchoas del Cantábrico",
            SectorId = 1,
            SectorName = "Sector A",
            UserId = "user-1",
            CreatedAt = new DateTime(2026, 3, 20, 14, 0, 0, DateTimeKind.Utc)
        },
        new StockMovementDto
        {
            Id = 2,
            Quantity = 10,
            Type = MovementType.Exit,
            ProductId = 1,
            ProductName = "Anchoas del Cantábrico",
            SectorId = 1,
            SectorName = "Sector A",
            RequesterId = 1,
            RequesterName = "Restaurante El Faro",
            UserId = "user-1",
            CreatedAt = new DateTime(2026, 3, 21, 9, 0, 0, DateTimeKind.Utc)
        }
    ];

    // ==================== CSV ====================

    [Fact]
    public void CsvGenerator_Should_Return_ValidCsv()
    {
        var generator = new CsvReportGenerator();
        var movements = CreateSampleMovements();

        var bytes = generator.Generate(movements);

        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Id,Fecha,Tipo,Producto,Sector");
        csv.Should().Contain("Anchoas del Cantábrico");
        csv.Should().Contain("Restaurante El Faro");
    }

    [Fact]
    public void CsvGenerator_Should_Have_CorrectContentType()
    {
        var generator = new CsvReportGenerator();

        generator.ContentType.Should().Be("text/csv");
        generator.FileExtension.Should().Be("csv");
    }

    [Fact]
    public void CsvGenerator_Should_Handle_EmptyList()
    {
        var generator = new CsvReportGenerator();

        var bytes = generator.Generate([]);

        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Id,Fecha,Tipo"); // Header still present
        csv.Split('\n').Where(l => l.Trim().Length > 0).Should().HaveCount(1); // Only header
    }

    [Fact]
    public void CsvGenerator_Should_Escape_Commas()
    {
        var generator = new CsvReportGenerator();
        var movements = new List<StockMovementDto>
        {
            new()
            {
                Id = 1, Quantity = 5, Type = MovementType.Entry,
                ProductId = 1, ProductName = "Producto con, coma",
                SectorId = 1, SectorName = "Sector A",
                UserId = "user-1", CreatedAt = DateTime.UtcNow
            }
        };

        var bytes = generator.Generate(movements);

        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("\"Producto con, coma\"");
    }

    // ==================== Excel ====================

    [Fact]
    public void ExcelGenerator_Should_Return_NonEmptyBytes()
    {
        var generator = new ExcelReportGenerator();
        var movements = CreateSampleMovements();

        var bytes = generator.Generate(movements);

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public void ExcelGenerator_Should_Have_CorrectContentType()
    {
        var generator = new ExcelReportGenerator();

        generator.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        generator.FileExtension.Should().Be("xlsx");
    }

    [Fact]
    public void ExcelGenerator_Should_Handle_EmptyList()
    {
        var generator = new ExcelReportGenerator();

        var bytes = generator.Generate([]);

        bytes.Should().NotBeEmpty(); // Still generates file with headers
    }

    // ==================== PDF ====================

    [Fact]
    public void PdfGenerator_Should_Return_NonEmptyBytes()
    {
        var generator = new PdfReportGenerator();
        var movements = CreateSampleMovements();

        var bytes = generator.Generate(movements);

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public void PdfGenerator_Should_Have_CorrectContentType()
    {
        var generator = new PdfReportGenerator();

        generator.ContentType.Should().Be("application/pdf");
        generator.FileExtension.Should().Be("pdf");
    }

    [Fact]
    public void PdfGenerator_Should_Start_With_PdfMagicBytes()
    {
        var generator = new PdfReportGenerator();
        var movements = CreateSampleMovements();

        var bytes = generator.Generate(movements);

        // PDF files start with %PDF
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    // ==================== Factory ====================

    [Theory]
    [InlineData(ReportFormat.Csv, "csv")]
    [InlineData(ReportFormat.Excel, "xlsx")]
    [InlineData(ReportFormat.Pdf, "pdf")]
    public void Factory_Should_Return_CorrectGenerator(ReportFormat format, string expectedExtension)
    {
        var generators = new Common.Interfaces.IReportGenerator[]
        {
            new CsvReportGenerator(),
            new ExcelReportGenerator(),
            new PdfReportGenerator()
        };
        var factory = new ReportGeneratorFactory(generators);

        var result = factory.Create(format);

        result.FileExtension.Should().Be(expectedExtension);
    }
}
