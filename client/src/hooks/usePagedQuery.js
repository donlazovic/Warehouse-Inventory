import { useCallback, useEffect, useState } from "react";

export default function usePagedQuery(fetcher, initialFilter = {}) {
  const [filter, setFilter] = useState({ page: 1, pageSize: 20, ...initialFilter });
  const [data, setData] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const cleaned = Object.fromEntries(
        Object.entries(filter).filter(([, value]) => value !== "" && value != null)
      );
      setData(await fetcher(cleaned));
    } catch (err) {
      setError(err.message);
      setData({ items: [], totalCount: 0 });
    } finally {
      setLoading(false);
    }
  }, [fetcher, filter]);

  useEffect(() => {
    load();
  }, [load]);

  const patchFilter = useCallback((changes) => {
    setFilter((current) => ({ ...current, page: 1, ...changes }));
  }, []);

  return {
    filter,
    setFilter,
    patchFilter,
    setPage: (page) => setFilter((current) => ({ ...current, page })),
    setPageSize: (pageSize) => setFilter((current) => ({ ...current, page: 1, pageSize })),
    setSort: (sortBy, sortDesc) => setFilter((current) => ({ ...current, sortBy, sortDesc })),
    rows: data.items,
    totalCount: data.totalCount,
    loading,
    error,
    reload: load,
  };
}
