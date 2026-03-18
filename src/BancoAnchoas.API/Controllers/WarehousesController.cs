using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Warehouses.Commands.CreateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.CreateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Application.Features.Warehouses.Queries.GetSectorById;
using BancoAnchoas.Application.Features.Warehouses.Queries.GetSectorsByWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Queries.GetWarehouseById;
using BancoAnchoas.Application.Features.Warehouses.Queries.GetWarehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class WarehousesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WarehouseDto>>>> GetAll()
    {
        var result = await Mediator.Send(new GetWarehousesQuery());
        return Ok(ApiResponse<IReadOnlyList<WarehouseDto>>.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetWarehouseByIdQuery(id));
        return Ok(ApiResponse<WarehouseDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateWarehouseCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<int>.Success(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateWarehouseCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateWarehouseCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/sectors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SectorDto>>>> GetSectors(int id)
    {
        var result = await Mediator.Send(new GetSectorsByWarehouseQuery(id));
        return Ok(ApiResponse<IReadOnlyList<SectorDto>>.Success(result));
    }

    [HttpPost("{id:int}/sectors")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> CreateSector(int id, CreateSectorCommand command)
    {
        if (id != command.WarehouseId) return BadRequest();
        var sectorId = await Mediator.Send(command);
        return Created($"/api/sectors/{sectorId}", ApiResponse<int>.Success(sectorId));
    }
}
