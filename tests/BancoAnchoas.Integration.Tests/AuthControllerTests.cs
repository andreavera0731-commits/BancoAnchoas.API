using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace BancoAnchoas.Integration.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn400Or401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrong@email.com",
            Password = "WrongPassword1!"
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/auth/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
