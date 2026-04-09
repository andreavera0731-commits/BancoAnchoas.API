using BancoAnchoas.Application.Features.Reports.Queries.ExportMovements;
using BancoAnchoas.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Reports;

public class ExportMovementsQueryValidatorTests
{
    private readonly ExportMovementsQueryValidator _validator = new();

    [Fact]
    public void Should_Pass_With_NoFilters()
    {
        var query = new ExportMovementsQuery(null, null, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_With_AllFilters()
    {
        var query = new ExportMovementsQuery(
            ProductId: 1,
            SectorId: 2,
            Type: MovementType.Entry,
            RequesterId: 3,
            From: new DateTime(2026, 1, 1),
            To: new DateTime(2026, 12, 31),
            Format: ReportFormat.Excel);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_ProductId_Is_Zero()
    {
        var query = new ExportMovementsQuery(ProductId: 0, null, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Should_Fail_When_SectorId_Is_Negative()
    {
        var query = new ExportMovementsQuery(null, SectorId: -1, null, null, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.SectorId);
    }

    [Fact]
    public void Should_Fail_When_RequesterId_Is_Zero()
    {
        var query = new ExportMovementsQuery(null, null, null, RequesterId: 0, null, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.RequesterId);
    }

    [Fact]
    public void Should_Fail_When_To_Is_Before_From()
    {
        var query = new ExportMovementsQuery(
            null, null, null, null,
            From: new DateTime(2026, 6, 1),
            To: new DateTime(2026, 1, 1));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Theory]
    [InlineData(ReportFormat.Csv)]
    [InlineData(ReportFormat.Excel)]
    [InlineData(ReportFormat.Pdf)]
    public void Should_Pass_With_ValidFormat(ReportFormat format)
    {
        var query = new ExportMovementsQuery(null, null, null, null, null, null, format);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Format);
    }

    [Fact]
    public void Should_Fail_With_InvalidFormat()
    {
        var query = new ExportMovementsQuery(null, null, null, null, null, null, (ReportFormat)99);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Format);
    }
}
