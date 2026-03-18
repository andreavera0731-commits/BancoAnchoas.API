using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterAdjustment;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterMovement;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterRelocation;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterWriteOff;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Application.Features.Stock.Queries.GetMovementHistory;
using BancoAnchoas.Application.Features.Stock.Queries.GetWriteOffHistory;
using BancoAnchoas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class StockController : ApiControllerBase
{
    [HttpPost("movements")]
    public async Task<ActionResult<ApiResponse<int>>> RegisterMovement(RegisterMovementCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<int>.Success(id));
    }

    [HttpPost("write-off")]
    public async Task<ActionResult<ApiResponse<int>>> RegisterWriteOff(RegisterWriteOffCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<int>.Success(id));
    }

    [HttpPost("relocate")]
    public async Task<ActionResult<ApiResponse<int>>> RegisterRelocation(RegisterRelocationCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<int>.Success(id));
    }

    [HttpPost("adjustment")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> RegisterAdjustment(RegisterAdjustmentCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(ApiResponse<int>.Success(id));
    }

    [HttpGet("history")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PaginatedList<StockMovementDto>>>> GetHistory(
        [FromQuery] int? productId, [FromQuery] int? sectorId,
        [FromQuery] MovementType? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetMovementHistoryQuery(productId, sectorId, type, from, to, pageNumber, pageSize));
        return Ok(ApiResponse<PaginatedList<StockMovementDto>>.Success(result));
    }

    [HttpGet("write-offs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StockMovementDto>>>> GetWriteOffs(
        [FromQuery] MovementReason? reason)
    {
        var result = await Mediator.Send(new GetWriteOffHistoryQuery(reason));
        return Ok(ApiResponse<IReadOnlyList<StockMovementDto>>.Success(result));
    }
}
