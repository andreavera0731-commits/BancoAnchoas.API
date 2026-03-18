namespace BancoAnchoas.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, string name, string role);
}
