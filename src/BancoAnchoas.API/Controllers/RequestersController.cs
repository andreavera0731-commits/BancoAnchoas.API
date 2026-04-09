using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;
using BancoAnchoas.Application.Features.Requesters.Commands.DeactivateRequester;
using BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;
using BancoAnchoas.Application.Features.Requesters.DTOs;
using BancoAnchoas.Application.Features.Requesters.Queries.GetRequesterById;
using BancoAnchoas.Application.Features.Requesters.Queries.GetRequesters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class RequestersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RequesterDto>>>> GetAll()
    {
        var result = await Mediator.Send(new GetRequestersQuery());
        return Ok(ApiResponse<IReadOnlyList<RequesterDto>>.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RequesterDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetRequesterByIdQuery(id));
        return Ok(ApiResponse<RequesterDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateRequesterCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<int>.Success(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateRequesterCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateRequesterCommand(id));
        return NoContent();
    }
}
