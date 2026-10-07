import ViewKanbanIcon from "@mui/icons-material/ViewKanban";
import ViewListIcon from "@mui/icons-material/ViewList";
import {
  Alert,
  Box,
  Button,
  Chip,
  ListSubheader,
  MenuItem,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
} from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import {
  exportParams,
  exportsApi,
  locationsApi,
  ordersApi,
  productsApi,
  storesApi,
  suppliersApi,
} from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DateRangeFields from "../components/common/DateRangeFields";
import DataTable from "../components/common/DataTable";
import SearchField from "../components/common/SearchField";
import ExportMenu from "../components/common/ExportMenu";
import PageHeader from "../components/common/PageHeader";
import { useToast } from "../components/common/Toast";
import OrderFormDialog from "../components/forms/OrderFormDialog";
import KanbanBoard from "../components/orders/KanbanBoard";
import OrderDetailDialog from "../components/orders/OrderDetailDialog";
import StatusChangeDialog from "../components/orders/StatusChangeDialog";
import usePagedQuery, { isFilterValueActive } from "../hooks/usePagedQuery";
import useUrlFilterSync, { readUrlFilter } from "../hooks/useUrlFilter";
import useRealtimeEvent from "../hooks/useRealtimeEvent";
import { monoFont, statusColors } from "../theme";
import {
  formatDate,
  formatMoney,
  orderStatusLabels,
  orderTypeLabels,
} from "../utils/format";

const permissionForStatus = (status) => {
  if (status === 3) return "orders.approve";
  if (status === 5) return "orders.execute";
  return "orders.update";
};

const ORDER_URL_FILTER = {
  status: "number",
  orderType: "number",
  supplierId: "number",
  storeId: "number",
};

// Filteri koji vaze i za tablu i za listu, pa se prenose pri promeni prikaza.
const SHARED_KEYS = ["search", "orderType", "supplierId", "storeId", "createdFrom", "createdTo"];

const pickShared = (filter) =>
  Object.fromEntries(SHARED_KEYS.map((key) => [key, filter[key] ?? null]));

const partnerValue = (filter) =>
  filter.supplierId ? `s:${filter.supplierId}` : filter.storeId ? `o:${filter.storeId}` : "";

const partnerFilter = (value) => {
  if (!value) return { supplierId: null, storeId: null };
  const [kind, id] = value.split(":");
  return kind === "s"
    ? { supplierId: Number(id), storeId: null }
    : { supplierId: null, storeId: Number(id) };
};

export default function OrdersPage() {
  const { can } = useAuth();
  const toast = useToast();
  const routerLocation = useLocation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [view, setView] = useState(() =>
    ["status", "supplierId", "storeId"].some((key) => searchParams.has(key)) ? "list" : "kanban"
  );
  const [kanbanColumns, setKanbanColumns] = useState([]);
  const [kanbanLoading, setKanbanLoading] = useState(true);
  const [kanbanFilter, setKanbanFilter] = useState({});

  const [lookups, setLookups] = useState({ products: [], suppliers: [], stores: [], locations: [] });
  const [partners, setPartners] = useState({ suppliers: [], stores: [] });
  const [detail, setDetail] = useState(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [initial, setInitial] = useState(null);
  const [highlighted, setHighlighted] = useState(new Set());
  const [statusRequest, setStatusRequest] = useState(null);
  const [statusError, setStatusError] = useState(null);
  const [statusSubmitting, setStatusSubmitting] = useState(false);

  const fetcher = useCallback((params) => ordersApi.list(params), []);
  const query = usePagedQuery(fetcher, readUrlFilter(searchParams, ORDER_URL_FILTER));
  useUrlFilterSync(ORDER_URL_FILTER, (filter) => {
    setView("list");
    query.patchFilter(filter);
  });

  const loadKanban = useCallback(() => {
    setKanbanLoading(true);
    const cleaned = Object.fromEntries(
      Object.entries(kanbanFilter).filter(([, value]) => value !== "" && value != null)
    );
    ordersApi
      .kanban(cleaned)
      .then(setKanbanColumns)
      .catch((err) => toast.error(err.message))
      .finally(() => setKanbanLoading(false));
  }, [kanbanFilter, toast]);

  useEffect(() => {
    if (view === "kanban") loadKanban();
  }, [view, loadKanban]);

  useEffect(() => {
    Promise.all([
      productsApi.list({ pageSize: 100, isActive: true }),
      suppliersApi.list({ pageSize: 100, sortBy: "name" }),
      storesApi.list({ pageSize: 100, sortBy: "name" }),
      locationsApi.lookup(),
    ])
      .then(([products, suppliers, stores, locations]) => {
        // Forma nudi samo aktivne partnere, a filter i neaktivne — da bi se videla i njihova istorija.
        setPartners({ suppliers: suppliers.items, stores: stores.items });
        setLookups({
          products: products.items,
          suppliers: suppliers.items.filter((item) => item.isActive),
          stores: stores.items.filter((item) => item.isActive),
          locations,
        });
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    const replenish = routerLocation.state?.replenish;
    if (!replenish || lookups.locations.length === 0) return;

    setEditing(null);
    setInitial({
      orderType: 2,
      storeId: replenish.storeId,
      destinationLocationId: replenish.destinationLocationId,
      note: `Dopuna zaliha: ${replenish.productName}`,
      lines: [{ productId: replenish.productId, quantity: replenish.quantity }],
    });
    setFormOpen(true);
    navigate(routerLocation.pathname, { replace: true, state: null });
  }, [routerLocation.state, routerLocation.pathname, lookups.locations.length, navigate]);

  useEffect(() => {
    const orderId = Number(searchParams.get("nalog"));
    if (!orderId) return;

    ordersApi
      .byId(orderId)
      .then(setDetail)
      .catch((err) => toast.error(err.message));
    setSearchParams({}, { replace: true });
  }, [searchParams, setSearchParams, toast]);

  useRealtimeEvent("ordersChanged", (payload) => {
    refreshAll();

    const ids = payload?.orderIds ?? [];
    if (ids.length === 0) return;

    setHighlighted((current) => new Set([...current, ...ids]));
    setTimeout(() => {
      setHighlighted((current) => {
        const next = new Set(current);
        ids.forEach((id) => next.delete(id));
        return next;
      });
    }, 2600);

    if (detail && ids.includes(detail.order.id)) {
      ordersApi
        .byId(detail.order.id)
        .then(setDetail)
        .catch(() => setDetail(null));
    }
  });

  const refreshAll = () => {
    if (view === "kanban") loadKanban();
    else query.reload();
  };

  const openDetail = async (order) => {
    try {
      setDetail(await ordersApi.byId(order.id));
    } catch (err) {
      toast.error(err.message);
    }
  };

  const submitStatusChange = async (note) => {
    setStatusSubmitting(true);
    setStatusError(null);

    try {
      const updated = await ordersApi.changeStatus(statusRequest.order.id, {
        status: statusRequest.status,
        note: note.trim() || null,
      });
      toast.success(`Nalog je prebacen u status "${orderStatusLabels[statusRequest.status]}".`);
      setStatusRequest(null);
      if (detail) setDetail(updated);
      refreshAll();
    } catch (err) {
      setStatusError(err.message);
    } finally {
      setStatusSubmitting(false);
    }
  };

  const columns = [
    {
      field: "orderNumber",
      headerName: "Broj naloga",
      sortable: true,
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {row.orderNumber}
        </Box>
      ),
    },
    {
      field: "orderType",
      headerName: "Tip",
      render: (row) => <Chip size="small" variant="outlined" label={orderTypeLabels[row.orderType]} />,
    },
    {
      field: "counterparty",
      headerName: "Druga strana",
      render: (row) => row.supplierName ?? row.storeName ?? "—",
    },
    { field: "itemCount", headerName: "Stavki", align: "right" },
    {
      field: "totalValue",
      headerName: "Vrednost",
      align: "right",
      sortable: true,
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {formatMoney(row.totalValue)}
        </Box>
      ),
    },
    {
      field: "status",
      headerName: "Status",
      sortable: true,
      render: (row) => (
        <Chip
          size="small"
          label={orderStatusLabels[row.status]}
          sx={{ bgcolor: statusColors[row.status], color: "#FFFFFF" }}
        />
      ),
    },
    { field: "createdByName", headerName: "Kreirao" },
    { field: "createdAt", headerName: "Datum", render: (row) => formatDate(row.createdAt) },
  ];

  const currentFilter = view === "kanban" ? kanbanFilter : query.filter;

  const applyFilter = (changes) =>
    view === "kanban"
      ? setKanbanFilter((current) => ({ ...current, ...changes }))
      : query.patchFilter(changes);

  const switchView = (next) => {
    if (!next || next === view) return;
    if (next === "list") query.patchFilter(pickShared(kanbanFilter));
    else setKanbanFilter(pickShared(query.filter));
    setView(next);
  };

  const changeOrderType = (orderType) => {
    const changes = { orderType };
    // Ulazni nalog nema prodajni objekat, izlazni nema dobavljaca — nekompatibilan izbor se brise.
    if (orderType === 1) changes.storeId = null;
    if (orderType === 2) changes.supplierId = null;
    applyFilter(changes);
  };

  const showSuppliers = currentFilter.orderType !== 2;
  const showStores = currentFilter.orderType !== 1;

  const partnerOptions = [
    ...(showSuppliers && partners.suppliers.length > 0
      ? [<ListSubheader key="h-s">Dobavljaci</ListSubheader>]
      : []),
    ...(showSuppliers
      ? partners.suppliers.map((supplier) => (
          <MenuItem key={`s:${supplier.id}`} value={`s:${supplier.id}`}>
            {supplier.name}
            {!supplier.isActive && " (neaktivan)"}
          </MenuItem>
        ))
      : []),
    ...(showStores && partners.stores.length > 0
      ? [<ListSubheader key="h-o">Prodajni objekti</ListSubheader>]
      : []),
    ...(showStores
      ? partners.stores.map((store) => (
          <MenuItem key={`o:${store.id}`} value={`o:${store.id}`}>
            {store.code} — {store.name}
            {!store.isActive && " (neaktivan)"}
          </MenuItem>
        ))
      : []),
  ];

  const filterControls = (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={2}
      useFlexGap
      sx={{ flexWrap: "wrap", alignItems: { md: "flex-start" } }}
    >
      <SearchField
        label="Pretraga po broju naloga"
        value={currentFilter.search}
        onSearch={(value) => applyFilter({ search: value })}
        sx={{ minWidth: 240 }}
      />
      <TextField
        select
        label="Tip"
        value={currentFilter.orderType ?? ""}
        onChange={(event) => changeOrderType(event.target.value)}
        sx={{ minWidth: 140 }}
      >
        <MenuItem value="">Svi</MenuItem>
        <MenuItem value={1}>Ulazni</MenuItem>
        <MenuItem value={2}>Izlazni</MenuItem>
      </TextField>

      <TextField
        select
        label="Druga strana"
        value={partners.suppliers.length + partners.stores.length > 0 ? partnerValue(currentFilter) : ""}
        onChange={(event) => applyFilter(partnerFilter(event.target.value))}
        sx={{ minWidth: 220, maxWidth: 300 }}
      >
        <MenuItem value="">Svi dobavljaci i objekti</MenuItem>
        {partnerOptions}
      </TextField>

      {view === "list" && (
        <TextField
          select
          label="Status"
          value={query.filter.status ?? ""}
          onChange={(event) => query.patchFilter({ status: event.target.value })}
          sx={{ minWidth: 170 }}
        >
          <MenuItem value="">Svi statusi</MenuItem>
          {Object.entries(orderStatusLabels).map(([value, label]) => (
            <MenuItem key={value} value={Number(value)}>
              {label}
            </MenuItem>
          ))}
        </TextField>
      )}

      <DateRangeFields
        fromLabel="Kreiran od"
        toLabel="Kreiran do"
        from={currentFilter.createdFrom}
        to={currentFilter.createdTo}
        onChange={({ from, to }) => applyFilter({ createdFrom: from, createdTo: to })}
      />

      <ClearFiltersButton
        active={
          view === "kanban"
            ? Object.values(kanbanFilter).some(isFilterValueActive)
            : query.hasActiveFilters
        }
        onClick={() => (view === "kanban" ? setKanbanFilter({}) : query.resetFilters())}
      />
    </Stack>
  );

  return (
    <Box>
      <PageHeader
        title="Nalozi"
        description="Ulazni nalozi od dobavljaca i izlazni nalozi ka prodajnim objektima. Prevucite karticu da biste promenili status."
        actions={
          <>
            <ToggleButtonGroup
              size="small"
              exclusive
              value={view}
              onChange={(_, next) => switchView(next)}
            >
              <ToggleButton value="kanban">
                <ViewKanbanIcon fontSize="small" sx={{ mr: 0.5 }} /> Tabla
              </ToggleButton>
              <ToggleButton value="list">
                <ViewListIcon fontSize="small" sx={{ mr: 0.5 }} /> Lista
              </ToggleButton>
            </ToggleButtonGroup>

            {can("orders.export") && (
              <ExportMenu
                fileName={`nalozi-${new Date().toISOString().slice(0, 10)}`}
                previewTitle="Nalozi"
                load={(format) =>
                  exportsApi.orders(format, exportParams(view === "kanban" ? kanbanFilter : query.filter))
                }
              />
            )}

            {can("orders.create") && (
              <Button
                variant="contained"
                onClick={() => {
                  setEditing(null);
                  setInitial(null);
                  setFormOpen(true);
                }}
              >
                Novi nalog
              </Button>
            )}
          </>
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        {filterControls}
      </Paper>

      {query.error && view === "list" && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {query.error}
        </Alert>
      )}

      {view === "kanban" ? (
        kanbanLoading && kanbanColumns.length === 0 ? (
          <Paper variant="outlined" sx={{ p: 6, textAlign: "center" }}>
            Ucitavanje table...
          </Paper>
        ) : (
          <KanbanBoard
            columns={kanbanColumns}
            canMove={can("orders.update")}
            canMoveTo={(status) => can(permissionForStatus(status))}
            highlighted={highlighted}
            onShowAll={(status) => {
              setView("list");
              query.patchFilter({ ...pickShared(kanbanFilter), status });
            }}
            onMove={(order, status) => setStatusRequest({ order, status })}
            onOpen={openDetail}
          />
        )
      ) : (
        <DataTable
          columns={columns}
          rows={query.rows}
          loading={query.loading}
          totalCount={query.totalCount}
          page={query.filter.page}
          pageSize={query.filter.pageSize}
          sortBy={query.filter.sortBy}
          sortDesc={query.filter.sortDesc}
          onPageChange={query.setPage}
          onPageSizeChange={query.setPageSize}
          onSortChange={query.setSort}
          onRowClick={openDetail}
          emptyTitle="Nema naloga"
          emptyHint="Kreirajte prvi nalog da biste primili ili poslali robu."
        />
      )}

      <OrderFormDialog
        open={formOpen}
        order={editing}
        initial={initial}
        products={lookups.products}
        suppliers={lookups.suppliers}
        stores={lookups.stores}
        locations={lookups.locations}
        onClose={() => {
          setFormOpen(false);
          setEditing(null);
          setInitial(null);
        }}
        onSaved={(message) => {
          toast.success(message);
          setFormOpen(false);
          setEditing(null);
          setInitial(null);
          setDetail(null);
          refreshAll();
        }}
      />

      <OrderDetailDialog
        detail={detail}
        canMoveTo={(status) => can(permissionForStatus(status))}
        onChangeStatus={(order, status) => setStatusRequest({ order, status })}
        onEdit={
          can("orders.update")
            ? (current) => {
                setEditing(current);
                setDetail(null);
                setFormOpen(true);
              }
            : undefined
        }
        onClose={() => setDetail(null)}
      />

      <StatusChangeDialog
        request={statusRequest}
        submitting={statusSubmitting}
        error={statusError}
        onConfirm={submitStatusChange}
        onClose={() => {
          setStatusRequest(null);
          setStatusError(null);
        }}
      />
    </Box>
  );
}
