using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Domain.Enums;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class StockControllerTests : IntegrationTestBase
{
    public StockControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<int> CreateCategoryAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new { Name = $"StockCat-{Guid.NewGuid():N}"[..20] });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;
    }

    private async Task<int> CreateRequesterAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/requesters", new { Name = $"Req-{Guid.NewGuid():N}"[..20] });
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

    // ── Entry / Exit ────────────────────────────────────────────

    [Fact]
    public async Task RegisterEntry_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 50);

        var response = await Client.PostAsJsonAsync("/api/stock/movements", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 25,
            Type = MovementType.Entry,
            Notes = "Recepción lote"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterExit_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 100);
        var requesterId = await CreateRequesterAsync();

        var response = await Client.PostAsJsonAsync("/api/stock/movements", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 10,
            Type = MovementType.Exit,
            Notes = "Despacho pedido",
            RequesterId = requesterId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterExit_ExceedingStock_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 5);
        var requesterId = await CreateRequesterAsync();

        var response = await Client.PostAsJsonAsync("/api/stock/movements", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 999,
            Type = MovementType.Exit,
            RequesterId = requesterId
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterMovement_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/stock/movements", new
        {
            ProductId = 0,
            SectorId = 0,
            Quantity = -1,
            Type = MovementType.Entry
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Write-off ───────────────────────────────────────────────

    [Fact]
    public async Task RegisterWriteOff_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 100);

        var response = await Client.PostAsJsonAsync("/api/stock/write-off", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 5,
            Reason = MovementReason.Damage,
            Notes = "Latas dañadas"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterWriteOff_ExceedingStock_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 2);

        var response = await Client.PostAsJsonAsync("/api/stock/write-off", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 999,
            Reason = MovementReason.Loss
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Relocation ──────────────────────────────────────────────

    [Fact]
    public async Task RegisterRelocation_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (whId, fromSectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 100);

        // Create a second sector in the same warehouse
        var sec2Response = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new { Name = "Destino", WarehouseId = whId });
        sec2Response.EnsureSuccessStatusCode();
        var toSectorId = (await sec2Response.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        var response = await Client.PostAsJsonAsync("/api/stock/relocate", new
        {
            ProductId = productId,
            FromSectorId = fromSectorId,
            SectorId = toSectorId,
            Quantity = 10,
            Notes = "Reubicación"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterRelocation_SameSector_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 100);

        var response = await Client.PostAsJsonAsync("/api/stock/relocate", new
        {
            ProductId = productId,
            FromSectorId = sectorId,
            SectorId = sectorId,
            Quantity = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Adjustment (Admin only) ─────────────────────────────────

    [Fact]
    public async Task RegisterAdjustment_Increase_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 50);

        var response = await Client.PostAsJsonAsync("/api/stock/adjustment", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 10,
            AdjustmentType = AdjustmentType.Increase,
            Reason = MovementReason.Other,
            Notes = "Corrección inventario"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterAdjustment_Decrease_ExceedingStock_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync();
        var (_, sectorId) = await CreateWarehouseAndSectorAsync();
        var productId = await CreateProductAsync(catId, 5);

        var response = await Client.PostAsJsonAsync("/api/stock/adjustment", new
        {
            ProductId = productId,
            SectorId = sectorId,
            Quantity = 999,
            AdjustmentType = AdjustmentType.Decrease
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── History (Admin only) ────────────────────────────────────

    [Fact]
    public async Task GetHistory_AsAdmin_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/stock/history?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("data").GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetWriteOffs_AsAdmin_ShouldReturn200()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/stock/write-offs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<StockMovementDto>>>();
        body!.Data.Should().NotBeNull();
    }
}
