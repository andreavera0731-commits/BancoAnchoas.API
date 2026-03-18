using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BancoAnchoas.Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BancoAnchoas.API.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
        => _configuration = configuration;

    public string GenerateToken(string userId, string email, string name, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };

        var expirationMinutes = int.Parse(
            _configuration["Jwt:ExpirationInMinutes"] ?? "1440");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
