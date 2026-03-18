using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Products.DTOs;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<int> CreateCategoryAsync(string name = "Test Category")
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new { Name = name, Description = "desc" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return body!.Data;
    }

    private async Task<int> CreateProductAsync(int categoryId, string name = "Anchoa Premium", string? barcode = null)
    {
        var response = await Client.PostAsJsonAsync("/api/products", new
        {
            Name = name,
            Description = "Anchoa de alta calidad",
            Barcode = barcode ?? $"BAR-{Guid.NewGuid():N}"[..20],
            Price = 12.50m,
            Unit = "kg",
            Stock = 100,
            MinimumStock = 10,
            CategoryId = categoryId
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return body!.Data;
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedList()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-GetAll");
        await CreateProductAsync(catId, "Prod-GetAll");

        var response = await Client.GetAsync("/api/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = json.GetProperty("data");
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
        data.GetProperty("pageNumber").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Create_ShouldReturn201WithId()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-Create");

        var response = await Client.PostAsJsonAsync("/api/products", new
        {
            Name = "Producto Nuevo",
            Unit = "un",
            Stock = 50,
            MinimumStock = 5,
            CategoryId = catId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/products", new
        {
            Name = "",       // empty name
            Unit = "xyz",    // invalid unit
            Stock = -1,      // negative
            MinimumStock = 0,
            CategoryId = 0   // invalid
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ShouldReturnProduct()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-GetById");
        var productId = await CreateProductAsync(catId, "Prod-GetById");

        var response = await Client.GetAsync($"/api/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(productId);
        body.Data.Name.Should().Be("Prod-GetById");
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/products/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByBarcode_ShouldReturnProduct()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-Barcode");
        var barcode = "UNIQUE-BAR-001";
        await CreateProductAsync(catId, "Prod-Barcode", barcode);

        var response = await Client.GetAsync($"/api/products/by-barcode/{barcode}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Barcode.Should().Be(barcode);
    }

    [Fact]
    public async Task Update_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-Update");
        var productId = await CreateProductAsync(catId, "Prod-Update");

        var response = await Client.PutAsJsonAsync($"/api/products/{productId}", new
        {
            Id = productId,
            Name = "Prod-Updated",
            Unit = "g",
            MinimumStock = 20,
            CategoryId = catId
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await Client.GetAsync($"/api/products/{productId}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body!.Data!.Name.Should().Be("Prod-Updated");
    }

    [Fact]
    public async Task Update_WithMismatchedId_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PutAsJsonAsync("/api/products/1", new
        {
            Id = 999,
            Name = "Mismatch",
            Unit = "kg",
            MinimumStock = 0,
            CategoryId = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deactivate_AsAdmin_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-Deactivate");
        var productId = await CreateProductAsync(catId, "Prod-Deactivate");

        var response = await Client.DeleteAsync($"/api/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ShouldFilterResults()
    {
        await AuthenticateAsAdminAsync();
        var catId = await CreateCategoryAsync("Cat-Search");
        await CreateProductAsync(catId, "Anchoa-Especial-XYZ");
        await CreateProductAsync(catId, "Otro-Producto-ABC");

        var response = await Client.GetAsync("/api/products?search=Especial");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("data").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        items.EnumerateArray().Should().Contain(i => i.GetProperty("name").GetString()!.Contains("Especial"));
    }
}
