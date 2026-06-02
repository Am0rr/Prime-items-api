using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;
using PI.DAL.Models.Catalog;

namespace PI.PL.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateAsync([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await _productService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        return Ok(product);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductPagedResponse>> GetPagedAsync([FromQuery] ProductFilterModel filter, CancellationToken cancellationToken)
    {
        var result = await _productService.GetPagedAsync(filter, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await _productService.UpdateAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}