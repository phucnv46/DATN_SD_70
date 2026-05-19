using DATN_70.Models.Products;
using DATN_70.Services;
using Microsoft.AspNetCore.Mvc;

namespace DATN_70.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IStoreRepository _storeRepository;

    public ProductsController(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductListItemResponse>>> GetProducts(
        CancellationToken cancellationToken)
    {
        var products = await _storeRepository.GetProductsAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDetailResponse>> GetProductById(
        string productId,
        CancellationToken cancellationToken)
    {
        var product = await _storeRepository.GetProductDetailAsync(productId, cancellationToken);

        if (product is null)
        {
            return NotFound(new { message = $"Khong tim thay san pham {productId}." });
        }

        return Ok(product);
    }
}
