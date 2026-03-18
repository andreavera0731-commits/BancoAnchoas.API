using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Users.Commands.CreateUser;
using BancoAnchoas.Application.Features.Users.Commands.DeactivateUser;
using BancoAnchoas.Application.Features.Users.Commands.UpdateUser;
using BancoAnchoas.Application.Features.Users.DTOs;
using BancoAnchoas.Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAll()
    {
        var result = await Mediator.Send(new GetUsersQuery());
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> Create(CreateUserCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id }, ApiResponse<string>.Success(id));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        await Mediator.Send(new DeactivateUserCommand(id));
        return NoContent();
    }
}
