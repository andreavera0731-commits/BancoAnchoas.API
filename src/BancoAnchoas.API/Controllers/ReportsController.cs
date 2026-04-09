using BancoAnchoas.Application.Features.Reports.Queries.ExportMovements;
using BancoAnchoas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : ApiControllerBase
{
    [HttpGet("movements/export")]
    public async Task<IActionResult> ExportMovements(
        [FromQuery] int? productId,
        [FromQuery] int? sectorId,
        [FromQuery] MovementType? type,
        [FromQuery] int? requesterId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ReportFormat format = ReportFormat.Csv)
    {
        var result = await Mediator.Send(
            new ExportMovementsQuery(productId, sectorId, type, requesterId, from, to, format));

        return File(result.Content, result.ContentType, result.FileName);
    }
}
