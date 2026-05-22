using DATN_70.Models.Orders;
using DATN_70.Services;
using Microsoft.AspNetCore.Mvc;

namespace DATN_70.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IStoreRepository _storeRepository;

    public OrdersController(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    [HttpPost]
    public async Task<ActionResult<OrderCreatedResponse>> PlaceOrder(
    [FromBody] PlaceOrderRequest request,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _storeRepository.PlaceOrderAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            // Trả thẳng lỗi ra để xem
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
