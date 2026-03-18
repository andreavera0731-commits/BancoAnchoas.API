using System.Net.Http.Headers;
using System.Net.Http.Json;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Auth.DTOs;

namespace BancoAnchoas.Integration.Tests;

/// <summary>
/// Base class for integration tests. Provides authenticated HttpClient helpers.
/// The factory seeds an Admin user (admin@bancodeanchoas.com / Admin123!) on startup.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Authenticates with the seeded Admin user and sets the Bearer token on the client.
    /// </summary>
    protected async Task AuthenticateAsAdminAsync()
    {
        var token = await GetTokenAsync("admin@bancodeanchoas.com", "Admin123!");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetTokenAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        return body!.Data!.Token;
    }
}
