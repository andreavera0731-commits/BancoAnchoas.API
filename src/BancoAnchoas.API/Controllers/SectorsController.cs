using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateSector;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Application.Features.Warehouses.Queries.GetSectorById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class SectorsController : ApiControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SectorDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetSectorByIdQuery(id));
        return Ok(ApiResponse<SectorDto>.Success(result));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateSectorCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateSectorCommand(id));
        return NoContent();
    }
}
