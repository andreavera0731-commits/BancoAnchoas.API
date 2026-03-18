using Microsoft.AspNetCore.Identity;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}
