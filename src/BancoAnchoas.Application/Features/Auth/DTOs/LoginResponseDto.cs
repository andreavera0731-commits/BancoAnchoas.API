namespace BancoAnchoas.Application.Features.Auth.DTOs;

public record LoginResponseDto(string Token, UserInfoDto User);

public record UserInfoDto(string Id, string Email, string Name, string Role);
