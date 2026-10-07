import { useCallback, useEffect, useMemo, useRef, useState } from "react";

const CONTROL_KEYS = ["page", "pageSize", "sortBy", "sortDesc"];

export const isFilterValueActive = (value) =>
  value !== "" && value != null && value !== false && value !== 0;

export default function usePagedQuery(fetcher, initialFilter = {}) {
  const [filter, setFilter] = useState({ page: 1, pageSize: 20, ...initialFilter });
  const [data, setData] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const defaultSort = useRef({ sortBy: initialFilter.sortBy, sortDesc: initialFilter.sortDesc });

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

  const resetFilters = useCallback(() => {
    setFilter((current) => ({
      page: 1,
      pageSize: current.pageSize,
      sortBy: defaultSort.current.sortBy,
      sortDesc: defaultSort.current.sortDesc,
    }));
  }, []);

  const hasActiveFilters = useMemo(
    () =>
      Object.entries(filter).some(([key, value]) => !CONTROL_KEYS.includes(key) && isFilterValueActive(value)),
    [filter]
  );

  return {
    filter,
    setFilter,
    patchFilter,
    resetFilters,
    hasActiveFilters,
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
