import { Alert, Box, Button, Chip, MenuItem, Paper, Stack, TextField } from "@mui/material";
import { useCallback } from "react";
import { storesApi, suppliersApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ConfirmDialog from "../components/common/ConfirmDialog";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DataTable from "../components/common/DataTable";
import PageHeader from "../components/common/PageHeader";
import RowActions from "../components/common/RowActions";
import PartnerFormDialog from "../components/forms/PartnerFormDialog";
import useCrudPage from "../hooks/useCrudPage";
import usePagedQuery from "../hooks/usePagedQuery";
import { formatDate, formatMoney } from "../utils/format";

const configs = {
  supplier: {
    api: suppliersApi,
    permissionPrefix: "suppliers",
    title: "Dobavljaci",
    description:
      "Partneri od kojih se nabavlja roba. Statistika prikazuje broj naloga i ukupnu vrednost realizovanih nabavki.",
    addLabel: "Dodaj dobavljaca",
    deleted: "Dobavljac je obrisan.",
    emptyTitle: "Nema dobavljaca",
    emptyHint: "Dodajte dobavljaca da biste mogli da kreirate ulazne naloge.",
  },
  store: {
    api: storesApi,
    permissionPrefix: "stores",
    title: "Prodajni objekti",
    description:
      "Objekti koje snabdeva centralni magacin. Svaki objekat pri kreiranju automatski dobija svoju skladisnu lokaciju.",
    addLabel: "Dodaj objekat",
    deleted: "Objekat je obrisan.",
    emptyTitle: "Nema objekata",
    emptyHint: "Dodajte objekat da biste mogli da kreirate izlazne naloge.",
  },
};

export default function PartnersPage({ kind }) {
  const config = configs[kind];
  const { can } = useAuth();

  const fetcher = useCallback((params) => config.api.list(params), [config.api]);
  const query = usePagedQuery(fetcher, { sortBy: "name" });

  const crud = useCrudPage({
    api: config.api,
    reload: query.reload,
    labels: { deleted: config.deleted },
  });

  const columns = [
    ...(kind === "store" ? [{ field: "code", headerName: "Sifra", sortable: true }] : []),
    { field: "name", headerName: "Naziv", sortable: true },
    kind === "store"
      ? { field: "managerName", headerName: "Odgovorno lice", render: (row) => row.managerName ?? "—" }
      : { field: "contactPerson", headerName: "Kontakt osoba", render: (row) => row.contactPerson ?? "—" },
    { field: "phone", headerName: "Telefon", render: (row) => row.phone ?? "—" },
    { field: "city", headerName: "Grad", sortable: true, render: (row) => row.city ?? "—" },
    { field: "orderCount", headerName: "Naloga", align: "right" },
    ...(kind === "supplier"
      ? [
          {
            field: "totalPurchaseValue",
            headerName: "Vrednost nabavki",
            align: "right",
            render: (row) => formatMoney(row.totalPurchaseValue),
          },
          {
            field: "lastOrderDate",
            headerName: "Poslednji nalog",
            render: (row) => formatDate(row.lastOrderDate),
          },
        ]
      : [{ field: "locationCount", headerName: "Lokacija", align: "right" }]),
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
          onEdit={can(`${config.permissionPrefix}.update`) ? () => crud.openEdit(row) : undefined}
          onDelete={can(`${config.permissionPrefix}.delete`) ? () => crud.setDeleting(row) : undefined}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        title={config.title}
        description={config.description}
        actions={
          can(`${config.permissionPrefix}.create`) && (
            <Button variant="contained" onClick={crud.openCreate}>
              {config.addLabel}
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
            label="Grad"
            value={query.filter.city ?? ""}
            onChange={(event) => query.patchFilter({ city: event.target.value })}
            sx={{ minWidth: 180 }}
          />
          <TextField
            select
            label="Status"
            value={query.filter.isActive ?? ""}
            onChange={(event) => query.patchFilter({ isActive: event.target.value })}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">Svi</MenuItem>
            <MenuItem value="true">Aktivni</MenuItem>
            <MenuItem value="false">Neaktivni</MenuItem>
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
        emptyTitle={config.emptyTitle}
        emptyHint={config.emptyHint}
      />

      <PartnerFormDialog
        open={crud.formOpen}
        kind={kind}
        entity={crud.editing}
        api={config.api}
        onClose={crud.closeForm}
        onSaved={(message) => {
          crud.toast.success(message);
          crud.closeForm();
          query.reload();
        }}
      />

      <ConfirmDialog
        open={Boolean(crud.deleting)}
        title="Brisanje zapisa"
        message={`Da li zelite da obrisete "${crud.deleting?.name}"? Ova akcija je trajna.`}
        confirmLabel="Obrisi"
        destructive
        loading={crud.busy}
        onConfirm={crud.confirmDelete}
        onClose={() => crud.setDeleting(null)}
      />
    </Box>
  );
}
