using DATN_70.Models.Orders;
using DATN_70.Models.Products;

namespace DATN_70.Services;

public interface IStoreRepository
{
    Task<IReadOnlyList<ProductListItemResponse>> GetProductsAsync(CancellationToken cancellationToken);

    Task<ProductDetailResponse?> GetProductDetailAsync(string productId, CancellationToken cancellationToken);

    Task<ServiceResult<OrderCreatedResponse>> PlaceOrderAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken);
}
