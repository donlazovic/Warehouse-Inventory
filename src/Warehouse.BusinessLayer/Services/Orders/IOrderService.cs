using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Orders;

namespace Warehouse.BusinessLayer.Services.Orders;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterRequest filter, CancellationToken ct = default);
    Task<List<KanbanColumnDto>> GetKanbanAsync(OrderFilterRequest filter, CancellationToken ct = default);
    Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, int currentUserId, CancellationToken ct = default);
    Task<OrderDetailDto> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<OrderDetailDto> ChangeStatusAsync(
        int id, ChangeOrderStatusRequest request, int currentUserId, Func<string, bool> hasPermission,
        CancellationToken ct = default);
}
