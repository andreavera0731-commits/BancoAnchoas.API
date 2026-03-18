using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Auth.Commands.Login;
using BancoAnchoas.Application.Features.Auth.DTOs;
using BancoAnchoas.Application.Features.Auth.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(ApiResponse<LoginResponseDto>.Success(result));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
    {
        var result = await Mediator.Send(new GetProfileQuery());
        return Ok(ApiResponse<UserProfileDto>.Success(result));
    }
}
