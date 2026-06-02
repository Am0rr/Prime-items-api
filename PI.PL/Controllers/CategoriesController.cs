using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;

namespace PI.PL.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateAsync([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _categoryService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);

        return Ok(category);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        await _categoryService.UpdateAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}