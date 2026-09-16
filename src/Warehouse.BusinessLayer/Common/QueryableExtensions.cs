using Microsoft.EntityFrameworkCore;

namespace Warehouse.BusinessLayer.Common;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, request.Page, request.PageSize);
    }

    public static IQueryable<T> ApplySort<T, TKey>(
        this IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, TKey>> keySelector,
        bool descending)
        => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
}
