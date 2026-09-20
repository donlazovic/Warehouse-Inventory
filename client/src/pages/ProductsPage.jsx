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
import DataTable from "../components/common/DataTable";
import PageHeader from "../components/common/PageHeader";
import { useToast } from "../components/common/Toast";
import usePagedQuery from "../hooks/usePagedQuery";
import { monoFont } from "../theme";
import { formatMoney, formatQuantity, unitLabels } from "../utils/format";

const stockStatusOptions = [
  { value: "", label: "Sve zalihe" },
  { value: 1, label: "Nema na stanju" },
  { value: 2, label: "Ispod minimuma" },
  { value: 3, label: "U granicama" },
  { value: 4, label: "Iznad maksimuma" },
];

export default function ProductsPage() {
  const { can } = useAuth();
  const toast = useToast();
  const [categories, setCategories] = useState([]);

  const fetcher = useCallback((params) => productsApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "name", sortDesc: false });

  useEffect(() => {
    categoriesApi
      .list({ pageSize: 100, isActive: true })
      .then((result) => setCategories(result.items))
      .catch(() => setCategories([]));
  }, []);

  const toggleFavorite = async (event, product) => {
    event.stopPropagation();
    try {
      await productsApi.toggleFavorite(product.id);
      query.reload();
    } catch (err) {
      toast.error(err.message);
    }
  };

  const columns = [
    {
      field: "isFavorite",
      headerName: "",
      width: 48,
      render: (row) => (
        <IconButton size="small" onClick={(event) => toggleFavorite(event, row)}>
          {row.isFavorite ? (
            <StarIcon fontSize="small" sx={{ color: "#B4541A" }} />
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
      sortable: false,
      render: (row) => {
        const below = row.totalStock < row.minStock;
        return (
          <>
            <Box
              component="span"
              sx={{
                fontFamily: monoFont,
                fontSize: "0.8rem",
                color: below ? "#B4541A" : "inherit",
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
      field: "isActive",
      headerName: "Status",
      render: (row) =>
        row.isActive ? (
          <Chip size="small" label="Aktivan" color="success" variant="outlined" />
        ) : (
          <Chip size="small" label="Neaktivan" variant="outlined" />
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
            <Button variant="contained">Dodaj proizvod</Button>
          )
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} alignItems={{ md: "center" }}>
          <TextField
            label="Pretraga po nazivu ili SKU"
            value={query.filter.search ?? ""}
            onChange={(event) => query.patchFilter({ search: event.target.value })}
            sx={{ minWidth: 260 }}
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

          <FormControlLabel
            control={
              <Switch
                checked={Boolean(query.filter.onlyFavorites)}
                onChange={(event) => query.patchFilter({ onlyFavorites: event.target.checked })}
              />
            }
            label="Samo omiljeni"
          />
        </Stack>
      </Paper>

      {query.error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {query.error}
        </Alert>
      )}

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

      <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
        Kolicine ispod minimalne zalihe prikazane su naglaseno.
      </Typography>
    </Box>
  );
}
