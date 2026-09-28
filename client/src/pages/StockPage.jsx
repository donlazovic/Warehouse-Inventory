import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import FactCheckOutlinedIcon from "@mui/icons-material/FactCheckOutlined";
import LocalShippingOutlinedIcon from "@mui/icons-material/LocalShippingOutlined";
import OutputIcon from "@mui/icons-material/Output";
import TuneOutlinedIcon from "@mui/icons-material/TuneOutlined";
import {
  Alert,
  Box,
  Button,
  Chip,
  FormControlLabel,
  IconButton,
  MenuItem,
  Paper,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { categoriesApi, exportParams, exportsApi, locationsApi, productsApi, stockApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import DataTable from "../components/common/DataTable";
import ExportMenu from "../components/common/ExportMenu";
import PageHeader from "../components/common/PageHeader";
import { useToast } from "../components/common/Toast";
import AdjustStockDialog from "../components/stock/AdjustStockDialog";
import IssueStockDialog from "../components/stock/IssueStockDialog";
import ReconciliationDialog from "../components/stock/ReconciliationDialog";
import StockLimitsDialog from "../components/stock/StockLimitsDialog";
import usePagedQuery from "../hooks/usePagedQuery";
import { monoFont } from "../theme";
import { formatDateTime, formatQuantity, locationTypeLabels, unitLabels } from "../utils/format";

const suggestReplenishment = (row) => {
  const target = row.effectiveMaxStock > 0 ? row.effectiveMaxStock : row.effectiveMinStock * 2;
  const missing = Math.round((target - row.quantity) * 1000) / 1000;
  return missing > 0 ? missing : row.effectiveMinStock || 1;
};

export default function StockPage() {
  const { can } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [locations, setLocations] = useState([]);
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);

  const [adjusting, setAdjusting] = useState(null);
  const [adjustOpen, setAdjustOpen] = useState(false);
  const [issueOpen, setIssueOpen] = useState(false);
  const [issuePreset, setIssuePreset] = useState(null);
  const [limitsRow, setLimitsRow] = useState(null);
  const [reconcileOpen, setReconcileOpen] = useState(false);

  const fetcher = useCallback((params) => stockApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "name" });

  useEffect(() => {
    locationsApi.lookup().then(setLocations).catch(() => {});
    categoriesApi.list({ pageSize: 100, isActive: true }).then((r) => setCategories(r.items)).catch(() => {});
    productsApi.list({ pageSize: 100, isActive: true }).then((r) => setProducts(r.items)).catch(() => {});
  }, []);

  const afterSave = (message) => {
    toast.success(message);
    query.reload();
  };

  const replenish = (row) =>
    navigate("/nalozi", {
      state: {
        replenish: {
          storeId: row.storeId,
          destinationLocationId: row.storageLocationId,
          productId: row.productId,
          quantity: suggestReplenishment(row),
          productName: row.productName,
        },
      },
    });

  const columns = [
    {
      field: "name",
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
    { field: "category", headerName: "Kategorija", sortable: true, render: (row) => row.categoryName },
    {
      field: "location",
      headerName: "Lokacija",
      sortable: true,
      render: (row) => (
        <Stack direction="row" spacing={1} alignItems="center">
          <Box>
            <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{row.locationCode}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.75rem" }}>
              {row.storeName ?? locationTypeLabels[row.locationType]}
            </Typography>
          </Box>
        </Stack>
      ),
    },
    {
      field: "quantity",
      headerName: "Stanje",
      align: "right",
      sortable: true,
      render: (row) => (
        <>
          <Box
            component="span"
            sx={{
              fontFamily: monoFont,
              fontSize: "0.82rem",
              fontWeight: row.isBelowMinimum ? 600 : 400,
              color: row.isBelowMinimum ? "warning.main" : "inherit",
            }}
          >
            {formatQuantity(row.quantity)}
          </Box>
          <Box component="span" sx={{ ml: 0.5, fontSize: "0.8rem", color: "text.secondary" }}>
            {unitLabels[row.unitOfMeasure]}
          </Box>
        </>
      ),
    },
    {
      field: "limits",
      headerName: "Min / max",
      align: "right",
      render: (row) => (
        <Tooltip
          title={
            row.minStockOverride != null || row.maxStockOverride != null
              ? "Podeseno za ovu lokaciju"
              : "Vrednost iz kataloga proizvoda"
          }
        >
          <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "text.secondary" }}>
            {formatQuantity(row.effectiveMinStock)} / {formatQuantity(row.effectiveMaxStock)}
            {(row.minStockOverride != null || row.maxStockOverride != null) && " *"}
          </Box>
        </Tooltip>
      ),
    },
    {
      field: "status",
      headerName: "",
      render: (row) =>
        row.quantity === 0 ? (
          <Chip size="small" variant="outlined" label="Nema" />
        ) : row.isBelowMinimum ? (
          <Chip size="small" color="warning" label="Ispod minimuma" />
        ) : null,
    },
    { field: "updatedat", headerName: "Promenjeno", sortable: true, render: (row) => formatDateTime(row.updatedAt) },
    {
      field: "actions",
      headerName: "",
      align: "right",
      render: (row) => (
        <Stack direction="row" spacing={0.25} justifyContent="flex-end">
          {can("orders.create") && row.locationType === 2 && row.isBelowMinimum && (
            <Button size="small" startIcon={<LocalShippingOutlinedIcon />} onClick={() => replenish(row)}>
              Dopuni
            </Button>
          )}
          {can("stock.issue") && row.quantity > 0 && (
            <Tooltip title="Izlaz robe">
              <IconButton
                size="small"
                onClick={() => {
                  setIssuePreset(row);
                  setIssueOpen(true);
                }}
              >
                <OutputIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {can("stock.update") && (
            <Tooltip title="Korekcija stanja">
              <IconButton
                size="small"
                onClick={() => {
                  setAdjusting(row);
                  setAdjustOpen(true);
                }}
              >
                <EditOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {can("stock.update") && (
            <Tooltip title="Granice za lokaciju">
              <IconButton size="small" onClick={() => setLimitsRow(row)}>
                <TuneOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        title="Stanje zaliha"
        description="Kolicina svakog artikla po lokaciji. Svaka promena stanja upisuje se u kretanje robe."
        actions={
          <>
            <Button startIcon={<FactCheckOutlinedIcon />} onClick={() => setReconcileOpen(true)}>
              Provera uskladjenosti
            </Button>
            {can("stock.export") && (
              <ExportMenu
                fileName={`zalihe-${new Date().toISOString().slice(0, 10)}`}
                previewTitle="Stanje zaliha"
                load={(format) => exportsApi.stock(format, exportParams(query.filter))}
              />
            )}
            {can("stock.update") && (
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={() => {
                  setAdjusting(null);
                  setAdjustOpen(true);
                }}
              >
                Unos stanja
              </Button>
            )}
            {can("stock.issue") && (
              <Button
                variant="contained"
                startIcon={<OutputIcon />}
                onClick={() => {
                  setIssuePreset(null);
                  setIssueOpen(true);
                }}
              >
                Izlaz robe
              </Button>
            )}
          </>
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} alignItems={{ md: "center" }}>
          <TextField
            label="Pretraga po nazivu ili SKU"
            value={query.filter.search ?? ""}
            onChange={(e) => query.patchFilter({ search: e.target.value })}
            sx={{ minWidth: 240 }}
          />
          <TextField
            select
            label="Lokacija"
            value={query.filter.storageLocationId ?? ""}
            onChange={(e) => query.patchFilter({ storageLocationId: e.target.value })}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">Sve lokacije</MenuItem>
            {locations.map((location) => (
              <MenuItem key={location.id} value={location.id}>
                {location.code} — {location.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Tip lokacije"
            value={query.filter.locationType ?? ""}
            onChange={(e) => query.patchFilter({ locationType: e.target.value })}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">Svi tipovi</MenuItem>
            <MenuItem value={1}>Centralni magacin</MenuItem>
            <MenuItem value={2}>Prodajni objekat</MenuItem>
          </TextField>
          <TextField
            select
            label="Kategorija"
            value={query.filter.categoryId ?? ""}
            onChange={(e) => query.patchFilter({ categoryId: e.target.value })}
            sx={{ minWidth: 170 }}
          >
            <MenuItem value="">Sve kategorije</MenuItem>
            {categories.map((category) => (
              <MenuItem key={category.id} value={category.id}>
                {category.name}
              </MenuItem>
            ))}
          </TextField>
          <FormControlLabel
            control={
              <Switch
                checked={Boolean(query.filter.onlyBelowMinimum)}
                onChange={(e) => query.patchFilter({ onlyBelowMinimum: e.target.checked })}
              />
            }
            label="Ispod minimuma"
          />
        </Stack>
      </Paper>

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
        emptyTitle="Nema zaliha"
        emptyHint="Roba se pojavljuje ovde kad se realizuje prvi ulazni nalog ili upise pocetno stanje."
      />

      <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
        Zvezdica pored granica znaci da su podesene posebno za tu lokaciju.
      </Typography>

      <AdjustStockDialog
        open={adjustOpen}
        row={adjusting}
        products={products}
        locations={locations}
        onClose={() => setAdjustOpen(false)}
        onSaved={(message) => {
          setAdjustOpen(false);
          afterSave(message);
        }}
      />

      <IssueStockDialog
        open={issueOpen}
        preset={issuePreset}
        locations={locations}
        onClose={() => setIssueOpen(false)}
        onSaved={(message) => {
          setIssueOpen(false);
          afterSave(message);
        }}
      />

      <StockLimitsDialog
        row={limitsRow}
        onClose={() => setLimitsRow(null)}
        onSaved={(message) => {
          setLimitsRow(null);
          afterSave(message);
        }}
      />

      <ReconciliationDialog open={reconcileOpen} onClose={() => setReconcileOpen(false)} />
    </Box>
  );
}
