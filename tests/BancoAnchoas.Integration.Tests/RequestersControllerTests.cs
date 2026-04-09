using System.Net;
using System.Net.Http.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Requesters.DTOs;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class RequestersControllerTests : IntegrationTestBase
{
    public RequestersControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturn201()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/requesters", new
        {
            Name = "Cocina",
            Description = "Sector cocina"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        body!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/requesters", new
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
        await Client.PostAsJsonAsync("/api/requesters", new { Name = "Req-List-Test" });

        var response = await Client.GetAsync("/api/requesters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<RequesterDto>>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturnRequester()
    {
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/requesters", new { Name = "Req-ById" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reqId = createBody!.Data;

        var response = await Client.GetAsync($"/api/requesters/{reqId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<RequesterDto>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Id.Should().Be(reqId);
        body.Data.Name.Should().Be("Req-ById");
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/requesters/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturn204()
    {
        await AuthenticateAsAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/requesters", new { Name = "Req-ToUpdate" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reqId = createBody!.Data;

        var response = await Client.PutAsJsonAsync($"/api/requesters/{reqId}", new
        {
            Id = reqId,
            Name = "Req-Updated",
            Description = "Updated description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/requesters/{reqId}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<RequesterDto>>();
        body!.Data!.Name.Should().Be("Req-Updated");
    }

    [Fact]
    public async Task Update_WithMismatchedId_ShouldReturn400()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.PutAsJsonAsync("/api/requesters/1", new
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
        var createResponse = await Client.PostAsJsonAsync("/api/requesters", new { Name = "Req-ToDelete" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<int>>();
        var reqId = createBody!.Data;

        var response = await Client.DeleteAsync($"/api/requesters/{reqId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
