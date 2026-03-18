using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Products.Commands.CreateProduct;
using BancoAnchoas.Application.Features.Products.Commands.DeactivateProduct;
using BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;
using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Application.Features.Products.Queries.GetExpiringProducts;
using BancoAnchoas.Application.Features.Products.Queries.GetLowStockProducts;
using BancoAnchoas.Application.Features.Products.Queries.GetProductByBarcode;
using BancoAnchoas.Application.Features.Products.Queries.GetProductById;
using BancoAnchoas.Application.Features.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class ProductsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductListDto>>>> GetAll(
        [FromQuery] string? search, [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetProductsQuery(search, categoryId, pageNumber, pageSize));
        return Ok(ApiResponse<PaginatedList<ProductListDto>>.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id));
        return Ok(ApiResponse<ProductDto>.Success(result));
    }

    [HttpGet("by-barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetByBarcode(string barcode)
    {
        var result = await Mediator.Send(new GetProductByBarcodeQuery(barcode));
        return Ok(ApiResponse<ProductDto>.Success(result));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductListDto>>>> GetLowStock()
    {
        var result = await Mediator.Send(new GetLowStockProductsQuery());
        return Ok(ApiResponse<IReadOnlyList<ProductListDto>>.Success(result));
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductListDto>>>> GetExpiring()
    {
        var result = await Mediator.Send(new GetExpiringProductsQuery());
        return Ok(ApiResponse<IReadOnlyList<ProductListDto>>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateProductCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<int>.Success(id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateProductCommand(id));
        return NoContent();
    }
}
