namespace BancoAnchoas.Application.Common.Interfaces;

public record AuthResult(string UserId, string Email, string Name, string Role);
public record UserResult(string Id, string Email, string Name, string Role, bool IsActive);

public interface IIdentityService
{
    Task<AuthResult?> AuthenticateAsync(string email, string password);
    Task<UserResult?> GetUserByIdAsync(string userId);
    Task<IReadOnlyList<UserResult>> GetUsersAsync();
    Task<string> CreateUserAsync(string email, string name, string password, string role);
    Task UpdateUserAsync(string userId, string? name, string? email, string? role);
    Task DeactivateUserAsync(string userId);
}
