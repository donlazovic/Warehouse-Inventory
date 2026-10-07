import { Alert, Box, Button, Chip, MenuItem, Paper, Stack, TextField } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { categoriesApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ConfirmDialog from "../components/common/ConfirmDialog";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DataTable from "../components/common/DataTable";
import PageHeader from "../components/common/PageHeader";
import RowActions from "../components/common/RowActions";
import CategoryFormDialog from "../components/forms/CategoryFormDialog";
import useCrudPage from "../hooks/useCrudPage";
import usePagedQuery from "../hooks/usePagedQuery";

export default function CategoriesPage() {
  const { can } = useAuth();
  const [allCategories, setAllCategories] = useState([]);

  const fetcher = useCallback((params) => categoriesApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "name" });

  const crud = useCrudPage({
    api: categoriesApi,
    reload: query.reload,
    labels: { deleted: "Kategorija je obrisana." },
  });

  const loadLookup = useCallback(() => {
    categoriesApi
      .list({ pageSize: 100 })
      .then((result) => setAllCategories(result.items))
      .catch(() => setAllCategories([]));
  }, []);

  useEffect(loadLookup, [loadLookup]);

  const columns = [
    { field: "name", headerName: "Naziv", sortable: true },
    {
      field: "parentCategoryName",
      headerName: "Nadredjena",
      render: (row) => row.parentCategoryName ?? "—",
    },
    { field: "description", headerName: "Opis", render: (row) => row.description ?? "—" },
    { field: "productCount", headerName: "Proizvoda", align: "right" },
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
          onEdit={can("categories.update") ? () => crud.openEdit(row) : undefined}
          onDelete={can("categories.delete") ? () => crud.setDeleting(row) : undefined}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        title="Kategorije"
        description="Hijerarhija kategorija robe. Kategorija koja sadrzi proizvode ili podkategorije ne moze se obrisati."
        actions={
          can("categories.create") && (
            <Button variant="contained" onClick={crud.openCreate}>
              Dodaj kategoriju
            </Button>
          )
        }
      />

      <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
          <TextField
            label="Pretraga"
            value={query.filter.search ?? ""}
            onChange={(event) => query.patchFilter({ search: event.target.value })}
            sx={{ minWidth: 260 }}
          />
          <TextField
            select
            label="Status"
            value={query.filter.isActive ?? ""}
            onChange={(event) => query.patchFilter({ isActive: event.target.value })}
            sx={{ minWidth: 180 }}
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
        emptyTitle="Nema kategorija"
        emptyHint="Dodajte prvu kategoriju da biste mogli da unosite proizvode."
      />

      <CategoryFormDialog
        open={crud.formOpen}
        category={crud.editing}
        categories={allCategories}
        onClose={crud.closeForm}
        onSaved={(message) => {
          crud.toast.success(message);
          crud.closeForm();
          query.reload();
          loadLookup();
        }}
      />

      <ConfirmDialog
        open={Boolean(crud.deleting)}
        title="Brisanje kategorije"
        message={`Da li zelite da obrisete kategoriju "${crud.deleting?.name}"? Ova akcija je trajna.`}
        confirmLabel="Obrisi"
        destructive
        loading={crud.busy}
        onConfirm={crud.confirmDelete}
        onClose={() => crud.setDeleting(null)}
      />
    </Box>
  );
}
