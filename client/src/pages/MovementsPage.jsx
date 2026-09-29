import ArrowRightAltIcon from "@mui/icons-material/ArrowRightAlt";
import { Alert, Box, Chip, MenuItem, Paper, Stack, TextField, Typography } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { exportParams, exportsApi, locationsApi, stockApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import DataTable from "../components/common/DataTable";
import ExportMenu from "../components/common/ExportMenu";
import PageHeader from "../components/common/PageHeader";
import usePagedQuery from "../hooks/usePagedQuery";
import useRealtimeEvent from "../hooks/useRealtimeEvent";
import { monoFont } from "../theme";
import {
  endOfDayIso,
  formatDateTime,
  formatQuantity,
  issueReasonLabels,
  movementTypeColors,
  movementTypeLabels,
  startOfDayIso,
  unitLabels,
} from "../utils/format";

const sign = (row, locationId) => {
  if (!locationId) return "";
  if (row.toLocationId === Number(locationId)) return "+";
  if (row.fromLocationId === Number(locationId)) return "−";
  return "";
};

export default function MovementsPage() {
  const { can } = useAuth();
  const [locations, setLocations] = useState([]);
  const [range, setRange] = useState({ from: "", to: "" });

  const fetcher = useCallback((params) => stockApi.movements(params), []);
  const query = usePagedQuery(fetcher, {});

  useEffect(() => {
    locationsApi.lookup().then(setLocations).catch(() => {});
  }, []);

  useRealtimeEvent("stockChanged", () => query.reload());

  const setDate = (field) => (event) => {
    const value = event.target.value;
    setRange((current) => ({ ...current, [field]: value }));
    query.patchFilter(field === "from" ? { dateFrom: startOfDayIso(value) } : { dateTo: endOfDayIso(value) });
  };

  const columns = [
    {
      field: "createdat",
      headerName: "Vreme",
      sortable: true,
      render: (row) => (
        <Typography variant="body2" sx={{ whiteSpace: "nowrap", fontSize: "0.8rem" }}>
          {formatDateTime(row.createdAt)}
        </Typography>
      ),
    },
    {
      field: "movementType",
      headerName: "Tip",
      render: (row) => (
        <Chip
          size="small"
          label={movementTypeLabels[row.movementType]}
          sx={{ bgcolor: movementTypeColors[row.movementType], color: "#FFFFFF" }}
        />
      ),
    },
    {
      field: "product",
      headerName: "Proizvod",
      sortable: true,
      render: (row) => (
        <Box>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>
            {row.productName}
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontSize: "0.72rem", color: "text.secondary" }}>
            {row.productSku}
          </Typography>
        </Box>
      ),
    },
    {
      field: "quantity",
      headerName: "Kolicina",
      align: "right",
      sortable: true,
      render: (row) => (
        <Box sx={{ whiteSpace: "nowrap" }}>
          <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.82rem" }}>
            {sign(row, query.filter.locationId)}
            {formatQuantity(row.quantity)}
          </Box>
          <Box component="span" sx={{ ml: 0.5, fontSize: "0.8rem", color: "text.secondary" }}>
            {unitLabels[row.unitOfMeasure]}
          </Box>
        </Box>
      ),
    },
    {
      field: "route",
      headerName: "Sa → na",
      render: (row) => (
        <Stack direction="row" alignItems="center" spacing={0.5} sx={{ fontSize: "0.8rem" }}>
          <Typography variant="body2" sx={{ fontSize: "0.8rem", color: row.fromLocationName ? "text.primary" : "text.disabled" }}>
            {row.fromLocationName ?? (row.movementType === 2 ? "Dobavljac" : "—")}
          </Typography>
          <ArrowRightAltIcon sx={{ fontSize: 16, color: "text.disabled" }} />
          <Typography variant="body2" sx={{ fontSize: "0.8rem", color: row.toLocationName ? "text.primary" : "text.disabled" }}>
            {row.toLocationName ?? (row.movementType === 3 ? "Van sistema" : "—")}
          </Typography>
        </Stack>
      ),
    },
    {
      field: "details",
      headerName: "Osnov",
      render: (row) => (
        <Box sx={{ maxWidth: 260 }}>
          {row.orderNumber && (
            <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{row.orderNumber}</Typography>
          )}
          {row.issueReason && (
            <Typography variant="body2" sx={{ fontSize: "0.8rem", fontWeight: 500 }}>
              {issueReasonLabels[row.issueReason]}
            </Typography>
          )}
          {row.note && !row.orderNumber && (
            <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.78rem" }} noWrap title={row.note}>
              {row.note}
            </Typography>
          )}
        </Box>
      ),
    },
    { field: "userName", headerName: "Korisnik", render: (row) => row.userName },
  ];

  return (
    <Box>
      <PageHeader
        title="Kretanje robe"
        description="Nepromenljiv dnevnik svake promene zaliha. Zapisi se ne mogu menjati ni brisati — ispravka se radi novom korekcijom."
        actions={
          can("stock.export") && (
            <ExportMenu
              fileName={`kretanje-robe-${new Date().toISOString().slice(0, 10)}`}
              previewTitle="Kretanje robe"
              load={(format) => exportsApi.movements(format, exportParams(query.filter))}
            />
          )
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", lg: "row" }} spacing={2}>
          <TextField
            label="Proizvod, SKU ili broj naloga"
            value={query.filter.search ?? ""}
            onChange={(e) => query.patchFilter({ search: e.target.value })}
            sx={{ minWidth: 240 }}
          />
          <TextField
            select
            label="Tip"
            value={query.filter.movementType ?? ""}
            onChange={(e) => query.patchFilter({ movementType: e.target.value })}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">Svi tipovi</MenuItem>
            {Object.entries(movementTypeLabels).map(([value, label]) => (
              <MenuItem key={value} value={Number(value)}>
                {label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Razlog izlaza"
            value={query.filter.issueReason ?? ""}
            onChange={(e) => query.patchFilter({ issueReason: e.target.value })}
            sx={{ minWidth: 170 }}
          >
            <MenuItem value="">Svi razlozi</MenuItem>
            {Object.entries(issueReasonLabels).map(([value, label]) => (
              <MenuItem key={value} value={Number(value)}>
                {label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Lokacija"
            value={query.filter.locationId ?? ""}
            onChange={(e) => query.patchFilter({ locationId: e.target.value })}
            sx={{ minWidth: 190 }}
          >
            <MenuItem value="">Sve lokacije</MenuItem>
            {locations.map((location) => (
              <MenuItem key={location.id} value={location.id}>
                {location.code} — {location.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Od"
            type="date"
            value={range.from}
            onChange={setDate("from")}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            label="Do"
            type="date"
            value={range.to}
            onChange={setDate("to")}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Stack>
      </Paper>

      {query.filter.locationId && (
        <Alert severity="info" variant="outlined" sx={{ mb: 2 }}>
          Kolicine su oznacene sa + i − u odnosu na izabranu lokaciju.
        </Alert>
      )}

      {query.error && <Alert severity="error" sx={{ mb: 2 }}>{query.error}</Alert>}

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
        emptyTitle="Nema kretanja"
        emptyHint="Promenite filtere ili realizujte prvi nalog."
      />
    </Box>
  );
}
