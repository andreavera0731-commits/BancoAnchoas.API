using System.Net;
using System.Net.Http.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Categories.DTOs;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class CategoriesControllerTests : IntegrationTestBase
{
    public CategoriesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturn201()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            Name = "Conservas",
            Description = "Productos en conserva"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            Name = "",
            Description = "desc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_ShouldReturnList()
    {
        await AuthenticateAsAdminAsync();
        await Client.PostAsJsonAsync("/api/categories", new { Name = "Cat-List-Test" });

        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CategoryDto>>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturnCategory()
    {
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/categories", new { Name = "Cat-ById" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var catId = createBody!.Data;

        var response = await Client.GetAsync($"/api/categories/{catId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(catId);
        body.Data.Name.Should().Be("Cat-ById");
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/categories/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/categories", new { Name = "Cat-ToUpdate" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var catId = createBody!.Data;

        var response = await Client.PutAsJsonAsync($"/api/categories/{catId}", new
        {
            Id = catId,
            Name = "Cat-Updated",
            Description = "Updated description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/categories/{catId}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        body!.Data!.Name.Should().Be("Cat-Updated");
    }

    [Fact]
    public async Task Update_WithMismatchedId_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PutAsJsonAsync("/api/categories/1", new
        {
            Id = 999,
            Name = "Mismatch"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deactivate_AsAdmin_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/categories", new { Name = "Cat-ToDelete" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var catId = createBody!.Data;

        var response = await Client.DeleteAsync($"/api/categories/{catId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
