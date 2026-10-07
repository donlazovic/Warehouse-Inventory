import { Alert, Box, Button, Chip, MenuItem, Paper, Stack, TextField } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { locationsApi, storesApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ConfirmDialog from "../components/common/ConfirmDialog";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DataTable from "../components/common/DataTable";
import SearchField from "../components/common/SearchField";
import PageHeader from "../components/common/PageHeader";
import RowActions from "../components/common/RowActions";
import LocationFormDialog from "../components/forms/LocationFormDialog";
import useCrudPage from "../hooks/useCrudPage";
import usePagedQuery from "../hooks/usePagedQuery";
import { monoFont } from "../theme";
import { locationTypeLabels } from "../utils/format";

export default function LocationsPage() {
  const { can } = useAuth();
  const [stores, setStores] = useState([]);

  const fetcher = useCallback((params) => locationsApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "code" });

  const crud = useCrudPage({
    api: locationsApi,
    reload: query.reload,
    labels: { deleted: "Lokacija je obrisana." },
  });

  useEffect(() => {
    storesApi
      .list({ pageSize: 100, isActive: true })
      .then((result) => setStores(result.items))
      .catch(() => setStores([]));
  }, []);

  const columns = [
    {
      field: "code",
      headerName: "Sifra",
      sortable: true,
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {row.code}
        </Box>
      ),
    },
    { field: "name", headerName: "Naziv", sortable: true },
    {
      field: "locationType",
      headerName: "Tip",
      sortable: true,
      render: (row) => (
        <Chip
          size="small"
          variant="outlined"
          color={row.locationType === 1 ? "secondary" : "default"}
          label={locationTypeLabels[row.locationType]}
        />
      ),
    },
    { field: "storeName", headerName: "Objekat", render: (row) => row.storeName ?? "—" },
    { field: "zone", headerName: "Zona", render: (row) => row.zone ?? "—" },
    { field: "productCount", headerName: "Artikala", align: "right" },
    {
      field: "isActive",
      headerName: "Status",
      render: (row) => (
        <Chip
          size="small"
          variant="outlined"
          color={row.isActive ? "success" : "default"}
          label={row.isActive ? "Aktivna" : "Neaktivna"}
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
          onEdit={can("stock.update") ? () => crud.openEdit(row) : undefined}
          onDelete={can("stock.update") ? () => crud.setDeleting(row) : undefined}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        title="Lokacije"
        description="Mesta na kojima stoji roba — zone centralnog magacina i skladisni prostori prodajnih objekata. Svaki objekat pri kreiranju dobija svoju lokaciju automatski."
        actions={
          can("stock.update") && (
            <Button variant="contained" onClick={crud.openCreate}>
              Dodaj lokaciju
            </Button>
          )
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
          <SearchField
            label="Pretraga po sifri ili nazivu"
            value={query.filter.search}
            onSearch={(value) => query.patchFilter({ search: value })}
            sx={{minWidth: 260 }}
          />
          <TextField
            select
            label="Tip"
            value={query.filter.locationType ?? ""}
            onChange={(event) => query.patchFilter({ locationType: event.target.value })}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">Svi tipovi</MenuItem>
            <MenuItem value={1}>Centralni magacin</MenuItem>
            <MenuItem value={2}>Prodajni objekat</MenuItem>
          </TextField>
          <TextField
            select
            label="Status"
            value={query.filter.isActive ?? ""}
            onChange={(event) => query.patchFilter({ isActive: event.target.value })}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">Sve</MenuItem>
            <MenuItem value="true">Aktivne</MenuItem>
            <MenuItem value="false">Neaktivne</MenuItem>
          </TextField>
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
        emptyTitle="Nema lokacija"
        emptyHint="Dodajte bar jednu lokaciju centralnog magacina da biste mogli da primate robu."
      />

      <LocationFormDialog
        open={crud.formOpen}
        location={crud.editing}
        stores={stores}
        onClose={crud.closeForm}
        onSaved={(message) => {
          crud.toast.success(message);
          crud.closeForm();
          query.reload();
        }}
      />

      <ConfirmDialog
        open={Boolean(crud.deleting)}
        title="Brisanje lokacije"
        message={`Da li zelite da obrisete lokaciju "${crud.deleting?.name}"? Ako na njoj ima zaliha ili istorije kretanja, brisanje nece biti moguce.`}
        confirmLabel="Obrisi"
        destructive
        loading={crud.busy}
        onConfirm={crud.confirmDelete}
        onClose={() => crud.setDeleting(null)}
      />
    </Box>
  );
}
