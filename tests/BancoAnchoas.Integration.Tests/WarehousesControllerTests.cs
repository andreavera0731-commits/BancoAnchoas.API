using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class WarehousesControllerTests : IntegrationTestBase
{
    public WarehousesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<int> CreateWarehouseAsync(string name = "Almacén Principal")
    {
        var response = await Client.PostAsJsonAsync("/api/warehouses", new
        {
            Name = name,
            Location = "Calle Test 123"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return body!.Data;
    }

    private async Task<int> CreateSectorAsync(int warehouseId, string name = "Sector A")
    {
        var response = await Client.PostAsJsonAsync($"/api/warehouses/{warehouseId}/sectors", new
        {
            Name = name,
            WarehouseId = warehouseId
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return body!.Data;
    }

    private async Task<int> CreateCategoryAsync(string name = "Conservas")
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            Name = name,
            Description = "Test category"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return body!.Data;
    }

    // ── Warehouses ──────────────────────────────────────────────

    [Fact]
    public async Task CreateWarehouse_AsAdmin_ShouldReturn201()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/warehouses", new
        {
            Name = "Almacén Nuevo",
            Location = "Madrid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateWarehouse_WithEmptyName_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/warehouses", new
        {
            Name = "",
            Location = "Madrid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllWarehouses_ShouldReturnList()
    {
        await AuthenticateAsAdminAsync();
        await CreateWarehouseAsync("WH-GetAll");

        var response = await Client.GetAsync("/api/warehouses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<WarehouseDto>>>();
        body!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWarehouseById_ShouldReturnWarehouse()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-ById");

        var response = await Client.GetAsync($"/api/warehouses/{whId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<WarehouseDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(whId);
        body.Data.Name.Should().Be("WH-ById");
    }

    [Fact]
    public async Task GetWarehouseById_NotFound_ShouldReturn404()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/warehouses/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWarehouse_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-ToUpdate");

        var response = await Client.PutAsJsonAsync($"/api/warehouses/{whId}", new
        {
            Id = whId,
            Name = "WH-Updated",
            Location = "Barcelona"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/warehouses/{whId}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<WarehouseDto>>();
        body!.Data!.Name.Should().Be("WH-Updated");
    }

    [Fact]
    public async Task UpdateWarehouse_WithMismatchedId_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PutAsJsonAsync("/api/warehouses/1", new
        {
            Id = 999,
            Name = "Mismatch"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivateWarehouse_NoSectors_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-ToDelete");

        var response = await Client.DeleteAsync($"/api/warehouses/{whId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── Sectors ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateSector_AsAdmin_ShouldReturn201()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-Sector-Create");

        var response = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Pasillo 1",
            WarehouseId = whId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateSector_WithMismatchedWarehouseId_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-Sector-Mismatch");

        var response = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Pasillo X",
            WarehouseId = 99999
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSectorsByWarehouse_ShouldReturnList()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-Sectors-List");
        await CreateSectorAsync(whId, "Sector-List-1");

        var response = await Client.GetAsync($"/api/warehouses/{whId}/sectors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<SectorDto>>>();
        body!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSectorById_ShouldReturnSector()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorById");
        var sectorId = await CreateSectorAsync(whId, "Sector-ById");

        var response = await Client.GetAsync($"/api/sectors/{sectorId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<SectorDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(sectorId);
        body.Data.Name.Should().Be("Sector-ById");
    }

    [Fact]
    public async Task UpdateSector_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorUpdate");
        var sectorId = await CreateSectorAsync(whId, "Sector-ToUpdate");

        var response = await Client.PutAsJsonAsync($"/api/sectors/{sectorId}", new
        {
            Id = sectorId,
            Name = "Sector-Updated"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<SectorDto>>();
        body!.Data!.Name.Should().Be("Sector-Updated");
    }

    [Fact]
    public async Task DeactivateSector_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorDelete");
        var sectorId = await CreateSectorAsync(whId, "Sector-ToDelete");

        var response = await Client.DeleteAsync($"/api/sectors/{sectorId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── Sectors + Categories ────────────────────────────────────

    [Fact]
    public async Task CreateSector_WithCategories_ShouldReturn201()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorCat-Create");
        var catId1 = await CreateCategoryAsync("Cat-Sector-1");
        var catId2 = await CreateCategoryAsync("Cat-Sector-2");

        var response = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Sector con Categorías",
            WarehouseId = whId,
            CategoryIds = new[] { catId1, catId2 }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var sectorId = body!.Data;

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var sectorBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categories = sectorBody.GetProperty("data").GetProperty("categories");
        categories.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task CreateSector_WithoutCategories_ShouldReturn201WithEmptyCategories()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorNoCat");

        var response = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Sector sin Categorías",
            WarehouseId = whId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var sectorId = body!.Data;

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var sectorBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categories = sectorBody.GetProperty("data").GetProperty("categories");
        categories.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task UpdateSector_WithCategories_ShouldReplaceCategories()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorCat-Update");
        var catId1 = await CreateCategoryAsync("Cat-UpdA");
        var catId2 = await CreateCategoryAsync("Cat-UpdB");
        var catId3 = await CreateCategoryAsync("Cat-UpdC");

        var createResponse = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Sector-CatUpdate",
            WarehouseId = whId,
            CategoryIds = new[] { catId1 }
        });
        var sectorId = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        var updateResponse = await Client.PutAsJsonAsync($"/api/sectors/{sectorId}", new
        {
            Id = sectorId,
            Name = "Sector-CatUpdated",
            CategoryIds = new[] { catId2, catId3 }
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var sectorBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categories = sectorBody.GetProperty("data").GetProperty("categories");
        categories.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task UpdateSector_WithEmptyCategories_ShouldClearCategories()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorCat-Clear");
        var catId = await CreateCategoryAsync("Cat-ToClear");

        var createResponse = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Sector-CatClear",
            WarehouseId = whId,
            CategoryIds = new[] { catId }
        });
        var sectorId = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        var updateResponse = await Client.PutAsJsonAsync($"/api/sectors/{sectorId}", new
        {
            Id = sectorId,
            Name = "Sector-CatCleared",
            CategoryIds = Array.Empty<int>()
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var sectorBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categories = sectorBody.GetProperty("data").GetProperty("categories");
        categories.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task UpdateSector_WithNullCategories_ShouldKeepExistingCategories()
    {
        await AuthenticateAsAdminAsync();
        var whId = await CreateWarehouseAsync("WH-SectorCat-Null");
        var catId = await CreateCategoryAsync("Cat-ToKeep");

        var createResponse = await Client.PostAsJsonAsync($"/api/warehouses/{whId}/sectors", new
        {
            Name = "Sector-CatKeep",
            WarehouseId = whId,
            CategoryIds = new[] { catId }
        });
        var sectorId = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>())!.Data;

        // Update only name, without sending categoryIds
        var updateResponse = await Client.PutAsJsonAsync($"/api/sectors/{sectorId}", new
        {
            Id = sectorId,
            Name = "Sector-Renamed"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/sectors/{sectorId}");
        var sectorBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        sectorBody.GetProperty("data").GetProperty("name").GetString().Should().Be("Sector-Renamed");
        var categories = sectorBody.GetProperty("data").GetProperty("categories");
        categories.GetArrayLength().Should().Be(1);
    }
}
