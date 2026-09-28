using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Inventory;

public interface IStockService
{
    Task<PagedResult<StockItemDto>> GetStockAsync(StockFilterRequest filter, CancellationToken ct = default);
    Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementFilterRequest filter, CancellationToken ct = default);
    Task<StockItemDto> AdjustAsync(AdjustStockRequest request, int currentUserId, CancellationToken ct = default);
    Task<int> IssueAsync(IssueStockRequest request, int currentUserId, CancellationToken ct = default);
    Task<StockItemDto> SetLimitsAsync(int stockItemId, SetStockLimitsRequest request, CancellationToken ct = default);
    Task<StockReconciliationDto> GetReconciliationAsync(CancellationToken ct = default);

    Task ApplyMovementAsync(
        int productId,
        decimal quantity,
        MovementType movementType,
        int? fromLocationId,
        int? toLocationId,
        int userId,
        int? orderId,
        string? note,
        IssueReason? issueReason = null,
        CancellationToken ct = default);
}
