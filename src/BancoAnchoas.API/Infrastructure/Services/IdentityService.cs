using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
        => _userManager = userManager;

    public async Task<AuthResult?> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        var valid = await _userManager.CheckPasswordAsync(user, password);
        if (!valid) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new AuthResult(user.Id, user.Email!, user.Name, roles.FirstOrDefault() ?? "Almacenista");
    }

    public async Task<UserResult?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserResult(user.Id, user.Email!, user.Name, roles.FirstOrDefault() ?? "Almacenista", user.LockoutEnabled == false);
    }

    public async Task<IReadOnlyList<UserResult>> GetUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var results = new List<UserResult>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(new UserResult(
                user.Id, user.Email!, user.Name,
                roles.FirstOrDefault() ?? "Almacenista",
                user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow));
        }

        return results;
    }

    public async Task<string> CreateUserAsync(string email, string name, string password, string role)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            Name = name
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure("Identity", e.Description));
            throw new Application.Common.Exceptions.ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, role);
        return user.Id;
    }

    public async Task UpdateUserAsync(string userId, string? name, string? email, string? role)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (name is not null) user.Name = name;
        if (email is not null)
        {
            user.Email = email;
            user.UserName = email;
        }

        await _userManager.UpdateAsync(user);

        if (role is not null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    public async Task DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        // Lock the user out indefinitely
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }
}
