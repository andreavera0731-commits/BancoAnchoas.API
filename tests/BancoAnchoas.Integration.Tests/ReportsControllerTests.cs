using System.Net;
using System.Net.Http.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Domain.Enums;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class ReportsControllerTests : IntegrationTestBase
{
    public ReportsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<int> CreateCategoryAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new { Name = $"RptCat-{Guid.NewGuid():N}"[..20] });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
    }

    private async Task<(int WarehouseId, int SectorId)> CreateWarehouseAndSectorAsync()
    {
        var whResponse = await Client.PostAsJsonAsync("/api/warehouses", new { Name = $"WH-{Guid.NewGuid():N}"[..20], Location = "Test" });
        whResponse.EnsureSuccessStatusCode();
        var whId = (await whResponse.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        var secResponse = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new { Name = $"Sec-{Guid.NewGuid():N}"[..20], WarehouseId = whId });
        secResponse.EnsureSuccessStatusCode();
        var secId = (await secResponse.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        return (whId, secId);
    }

    private async Task<int> CreateProductAsync(int categoryId, int stock = 100)
    {
        var response = await Client.PostAsJsonAsync("/api/products", new
        {
            Name = $"Prod-{Guid.NewGuid():N}"[..20],
            Unit = "kg",
            Stock = stock,
            MinimumStock = 5,
            CategoryId = categoryId
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
    }

    private async Task CreateEntryMovementAsync(int productId, int sectorId)
    {
        var response = await Client.PostAsJsonAsync("/api/stock/movements", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 10,
            Type = MovementType.Entry,
            Notes = "Test entry"
        });
        response.EnsureSuccessStatusCode();
    }

    // ── CSV ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExportCsv_ShouldReturn_CsvFile()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId);
        await CreateEntryMovementAsync(productId, sectorId);

        var response = await Client.GetAsync("/api/reports/movements/export?format=0");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Id,Fecha,Tipo");
    }

    // ── Excel ───────────────────────────────────────────────────

    [Fact]
    public async Task ExportExcel_ShouldReturn_XlsxFile()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId);
        await CreateEntryMovementAsync(productId, sectorId);

        var response = await Client.GetAsync("/api/reports/movements/export?format=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(100);
    }

    // ── PDF ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExportPdf_ShouldReturn_PdfFile()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId);
        await CreateEntryMovementAsync(productId, sectorId);

        var response = await Client.GetAsync("/api/reports/movements/export?format=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    // ── Filters ─────────────────────────────────────────────────

    [Fact]
    public async Task Export_WithFilters_ShouldReturn_200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId);
        await CreateEntryMovementAsync(productId, sectorId);

        var response = await Client.GetAsync(
            $"/api/reports/movements/export?format=0&productId={productId}&type=0&from=2026-01-01&to=2026-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Empty result ────────────────────────────────────────────

    [Fact]
    public async Task Export_NoMovements_ShouldReturn_EmptyFile()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/reports/movements/export?format=0&productId=999999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Id,Fecha,Tipo"); // Header only
    }

    // ── Content-Disposition ─────────────────────────────────────

    [Fact]
    public async Task Export_ShouldInclude_ContentDisposition()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/reports/movements/export?format=0");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentDisposition.Should().NotBeNull();
        response.Content.Headers.ContentDisposition!.FileName.Should().StartWith("movimientos-");
        response.Content.Headers.ContentDisposition.FileName.Should().EndWith(".csv");
    }

    // ── Auth ────────────────────────────────────────────────────

    [Fact]
    public async Task Export_WithoutAuth_ShouldReturn_401()
    {
        var response = await Client.GetAsync("/api/reports/movements/export?format=0");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
