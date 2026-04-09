using Microsoft.AspNetCore.Identity;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
}
