using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Categories.Commands.CreateCategory;
using BancoAnchoas.Application.Features.Categories.Commands.DeactivateCategory;
using BancoAnchoas.Application.Features.Categories.Commands.UpdateCategory;
using BancoAnchoas.Application.Features.Categories.DTOs;
using BancoAnchoas.Application.Features.Categories.Queries.GetCategories;
using BancoAnchoas.Application.Features.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class CategoriesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetAll()
    {
        var result = await Mediator.Send(new GetCategoriesQuery());
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(ApiResponse<CategoryDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateCategoryCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<int>.Success(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateCategoryCommand(id));
        return NoContent();
    }
}
