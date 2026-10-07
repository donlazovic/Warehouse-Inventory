import StarIcon from "@mui/icons-material/Star";
import StarBorderIcon from "@mui/icons-material/StarBorder";
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
  Typography,
} from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { categoriesApi, productsApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ConfirmDialog from "../components/common/ConfirmDialog";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DateRangeFields from "../components/common/DateRangeFields";
import DataTable from "../components/common/DataTable";
import SearchField from "../components/common/SearchField";
import PageHeader from "../components/common/PageHeader";
import RowActions from "../components/common/RowActions";
import ProductFormDialog from "../components/forms/ProductFormDialog";
import useCrudPage from "../hooks/useCrudPage";
import usePagedQuery from "../hooks/usePagedQuery";
import { monoFont } from "../theme";
import { formatDate, formatMoney, formatQuantity, unitLabels } from "../utils/format";

const stockStatusOptions = [
  { value: "", label: "Sve zalihe" },
  { value: 1, label: "Nema na stanju" },
  { value: 2, label: "Ispod minimuma" },
  { value: 3, label: "U granicama" },
  { value: 4, label: "Iznad maksimuma" },
];

export default function ProductsPage() {
  const { can } = useAuth();
  const [categories, setCategories] = useState([]);

  const fetcher = useCallback((params) => productsApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "name", sortDesc: false });

  const crud = useCrudPage({
    api: productsApi,
    reload: query.reload,
    labels: { deleted: "Proizvod je obrisan." },
  });

  useEffect(() => {
    categoriesApi
      .list({ pageSize: 100, isActive: true })
      .then((result) => setCategories(result.items))
      .catch(() => setCategories([]));
  }, []);

  const toggleFavorite = async (product) => {
    try {
      await productsApi.toggleFavorite(product.id);
      query.reload();
    } catch (err) {
      crud.toast.error(err.message);
    }
  };

  const columns = [
    {
      field: "isFavorite",
      headerName: "",
      width: 48,
      render: (row) => (
        <IconButton
          size="small"
          onClick={(event) => {
            event.stopPropagation();
            toggleFavorite(row);
          }}
        >
          {row.isFavorite ? (
            <StarIcon fontSize="small" sx={{ color: "warning.main" }} />
          ) : (
            <StarBorderIcon fontSize="small" />
          )}
        </IconButton>
      ),
    },
    {
      field: "sku",
      headerName: "SKU",
      sortable: true,
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {row.sku}
        </Box>
      ),
    },
    { field: "name", headerName: "Naziv", sortable: true },
    { field: "categoryName", headerName: "Kategorija", sortable: true },
    {
      field: "totalStock",
      headerName: "Na stanju",
      align: "right",
      render: (row) => {
        const below = row.totalStock < row.minStock;
        return (
          <>
            <Box
              component="span"
              sx={{
                fontFamily: monoFont,
                fontSize: "0.8rem",
                color: below ? "warning.main" : "inherit",
                fontWeight: below ? 500 : 400,
              }}
            >
              {formatQuantity(row.totalStock)}
            </Box>
            <Box component="span" sx={{ fontSize: "0.8rem", color: "text.secondary", ml: 0.5 }}>
              {unitLabels[row.unitOfMeasure]}
            </Box>
          </>
        );
      },
    },
    {
      field: "price",
      headerName: "Cena",
      align: "right",
      sortable: true,
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {formatMoney(row.price)}
        </Box>
      ),
    },
    {
      field: "createdat",
      headerName: "Dodat",
      sortable: true,
      render: (row) => formatDate(row.createdAt),
    },
    {
      field: "isActive",
      headerName: "Status",
      render: (row) => (
        <Chip
          size="small"
          variant="outlined"
          color={row.isActive ? "success" : "default"}
          label={row.isActive ? "Aktivan" : "Neaktivan"}
        />
      ),
    },
    {
      field: "actions",
      headerName: "",
      align: "right",
      width: 96,
      render: (row) => (
        <RowActions
          onEdit={can("products.update") ? () => crud.openEdit(row) : undefined}
          onDelete={can("products.delete") ? () => crud.setDeleting(row) : undefined}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        title="Proizvodi"
        description="Katalog robe sa trenutnim stanjem zaliha zbirno po svim lokacijama."
        actions={
          can("products.create") && (
            <Button variant="contained" onClick={crud.openCreate}>
              Dodaj proizvod
            </Button>
          )
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} useFlexGap sx={{
          alignItems: { md: "flex-start" },
          flexWrap: "wrap"
        }}>
          <SearchField
            label="Pretraga po nazivu ili SKU"
            value={query.filter.search}
            onSearch={(value) => query.patchFilter({ search: value })}
            sx={{minWidth: 260 }}
          />

          <TextField
            select
            label="Kategorija"
            value={query.filter.categoryId ?? ""}
            onChange={(event) => query.patchFilter({ categoryId: event.target.value })}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">Sve kategorije</MenuItem>
            {categories.map((category) => (
              <MenuItem key={category.id} value={category.id}>
                {category.name}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            select
            label="Stanje zaliha"
            value={query.filter.stockStatus ?? ""}
            onChange={(event) => query.patchFilter({ stockStatus: event.target.value })}
            sx={{ minWidth: 180 }}
          >
            {stockStatusOptions.map((option) => (
              <MenuItem key={option.label} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            select
            label="Period se odnosi na"
            value={query.filter.periodBasis ?? 0}
            onChange={(event) => query.patchFilter({ periodBasis: event.target.value })}
            sx={{ minWidth: 190 }}
          >
            <MenuItem value={0}>Datum dodavanja</MenuItem>
            <MenuItem value={1}>Promet robe</MenuItem>
          </TextField>

          <DateRangeFields
            from={query.filter.dateFrom}
            to={query.filter.dateTo}
            onChange={({ from, to }) => query.patchFilter({ dateFrom: from, dateTo: to })}
          />

          <FormControlLabel
            control={
              <Switch
                checked={Boolean(query.filter.onlyFavorites)}
                onChange={(event) => query.patchFilter({ onlyFavorites: event.target.checked })}
              />
            }
            label="Samo omiljeni"
            sx={{ mt: { md: 1 } }}
          />
          <ClearFiltersButton active={query.hasActiveFilters} onClick={query.resetFilters} />
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
        emptyTitle="Nema proizvoda"
        emptyHint="Promenite filtere ili dodajte prvi proizvod u katalog."
      />

      <Typography
        variant="body2"
        sx={{
          color: "text.secondary",
          mt: 2
        }}>
        Kolicine ispod minimalne zalihe prikazane su naglaseno.
        {query.filter.periodBasis === 1 && (query.filter.dateFrom || query.filter.dateTo) &&
          " Prikazani su proizvodi koji su imali bar jedno kretanje robe u izabranom periodu."}
      </Typography>

      <ProductFormDialog
        open={crud.formOpen}
        product={crud.editing}
        categories={categories}
        onClose={crud.closeForm}
        onSaved={(message) => {
          crud.toast.success(message);
          crud.closeForm();
          query.reload();
        }}
      />

      <ConfirmDialog
        open={Boolean(crud.deleting)}
        title="Brisanje proizvoda"
        message={`Da li zelite da obrisete "${crud.deleting?.name}"? Ako proizvod ima zalihe ili istoriju u nalozima, brisanje nece biti moguce.`}
        confirmLabel="Obrisi"
        destructive
        loading={crud.busy}
        onConfirm={crud.confirmDelete}
        onClose={() => crud.setDeleting(null)}
      />
    </Box>
  );
}
