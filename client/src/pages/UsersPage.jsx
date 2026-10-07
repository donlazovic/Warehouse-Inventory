import CheckIcon from "@mui/icons-material/Check";
import CloseIcon from "@mui/icons-material/Close";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import KeyOutlinedIcon from "@mui/icons-material/KeyOutlined";
import {
  Alert,
  Avatar,
  Badge,
  Box,
  Button,
  Chip,
  IconButton,
  MenuItem,
  Paper,
  Stack,
  Tab,
  Tabs,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { rolesApi, usersApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import ConfirmDialog from "../components/common/ConfirmDialog";
import ClearFiltersButton from "../components/common/ClearFiltersButton";
import DataTable from "../components/common/DataTable";
import SearchField from "../components/common/SearchField";
import PageHeader from "../components/common/PageHeader";
import { useToast } from "../components/common/Toast";
import ApproveUserDialog from "../components/users/ApproveUserDialog";
import ResetPasswordDialog from "../components/users/ResetPasswordDialog";
import RoleEditDialog from "../components/users/RoleEditDialog";
import UserFormDialog from "../components/users/UserFormDialog";
import usePagedQuery from "../hooks/usePagedQuery";
import useRealtimeEvent from "../hooks/useRealtimeEvent";
import { formatDateTime } from "../utils/format";

const statusTabs = [
  { value: 0, label: "Svi" },
  { value: 1, label: "Na cekanju" },
  { value: 2, label: "Aktivni" },
  { value: 3, label: "Neaktivni" },
];

function UserStatus({ user }) {
  if (user.isPendingApproval) return <Chip size="small" label="Na cekanju" color="warning" />;
  if (!user.isActive) return <Chip size="small" variant="outlined" label="Neaktivan" />;
  return <Chip size="small" variant="outlined" color="success" label="Aktivan" />;
}

function RolesPanel({ roles, onEdit }) {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr", lg: "repeat(4, 1fr)" },
        gap: 2,
      }}
    >
      {roles.map((role) => (
        <Paper key={role.id} variant="outlined" sx={{ p: 2.5, display: "flex", flexDirection: "column" }}>
          <Stack
            direction="row"
            sx={{
              justifyContent: "space-between",
              alignItems: "flex-start"
            }}>
            <Typography variant="h3">{role.name}</Typography>
            {role.isProtected && <Chip size="small" variant="outlined" label="Zakljucana" />}
          </Stack>

          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mt: 1,
              flexGrow: 1,
              minHeight: 40
            }}>
            {role.description ?? "Bez opisa"}
          </Typography>

          <Stack direction="row" spacing={3} sx={{ mt: 2, mb: 2 }}>
            <Box>
              <Typography sx={{ fontSize: "1.3rem", fontWeight: 600 }}>{role.permissionCount}</Typography>
              <Typography
                variant="body2"
                sx={{
                  color: "text.secondary",
                  fontSize: "0.75rem"
                }}>
                dozvola
              </Typography>
            </Box>
            <Box>
              <Typography sx={{ fontSize: "1.3rem", fontWeight: 600 }}>{role.userCount}</Typography>
              <Typography
                variant="body2"
                sx={{
                  color: "text.secondary",
                  fontSize: "0.75rem"
                }}>
                korisnika
              </Typography>
            </Box>
          </Stack>

          <Button variant="outlined" onClick={() => onEdit(role.id)} fullWidth>
            {role.isProtected ? "Pregled dozvola" : "Uredi dozvole"}
          </Button>
        </Paper>
      ))}
    </Box>
  );
}

export default function UsersPage() {
  const { user: currentUser, can } = useAuth();
  const toast = useToast();

  const [tab, setTab] = useState("users");
  const [roles, setRoles] = useState([]);
  const [tree, setTree] = useState([]);
  const [pendingCount, setPendingCount] = useState(0);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [approving, setApproving] = useState(null);
  const [rejecting, setRejecting] = useState(null);
  const [resetting, setResetting] = useState(null);
  const [editingRoleId, setEditingRoleId] = useState(null);
  const [busy, setBusy] = useState(false);

  const fetcher = useCallback((params) => usersApi.list(params), []);
  const query = usePagedQuery(fetcher, { sortBy: "lastname", status: 0 });

  const loadMeta = useCallback(() => {
    rolesApi.list().then(setRoles).catch(() => {});
    usersApi.pendingCount().then(setPendingCount).catch(() => {});
  }, []);

  useEffect(() => {
    loadMeta();
    rolesApi.tree().then(setTree).catch(() => {});
  }, [loadMeta]);

  useRealtimeEvent("pendingUsersChanged", () => {
    query.reload();
    loadMeta();
  });

  const afterChange = (message) => {
    toast.success(message);
    query.reload();
    loadMeta();
  };

  const confirmReject = async () => {
    setBusy(true);
    try {
      await usersApi.reject(rejecting.id);
      setRejecting(null);
      afterChange("Zahtev je odbijen.");
    } catch (err) {
      toast.error(err.message);
    } finally {
      setBusy(false);
    }
  };

  const columns = [
    {
      field: "name",
      headerName: "Korisnik",
      sortable: true,
      render: (row) => (
        <Stack direction="row" spacing={1.5} sx={{
          alignItems: "center"
        }}>
          <Avatar sx={{ width: 30, height: 30, fontSize: "0.72rem", fontWeight: 600, bgcolor: "avatar.bg", color: "avatar.fg" }}>
            {`${row.firstName[0] ?? ""}${row.lastName[0] ?? ""}`.toUpperCase()}
          </Avatar>
          <Box>
            <Typography variant="body2" sx={{ fontWeight: 500 }}>
              {row.firstName} {row.lastName}
              {row.id === currentUser?.id && (
                <Typography component="span" variant="body2" sx={{ color: "text.secondary", ml: 0.75 }}>
                  (vi)
                </Typography>
              )}
            </Typography>
            <Typography
              variant="body2"
              sx={{
                color: "text.secondary",
                fontSize: "0.78rem"
              }}>
              {row.email}
            </Typography>
          </Box>
        </Stack>
      ),
    },
    {
      field: "role",
      headerName: "Uloga",
      sortable: true,
      render: (row) => (row.isPendingApproval ? "—" : row.roleName),
    },
    { field: "status", headerName: "Status", render: (row) => <UserStatus user={row} /> },
    {
      field: "lastLoginAt",
      headerName: "Poslednja prijava",
      render: (row) => formatDateTime(row.lastLoginAt),
    },
    {
      field: "createdAt",
      headerName: "Registrovan",
      sortable: true,
      render: (row) => formatDateTime(row.createdAt),
    },
    {
      field: "actions",
      headerName: "",
      align: "right",
      render: (row) => {
        const isSelf = row.id === currentUser?.id;
        const locked =
          row.isOwner || (row.roleName === "Admin" && !currentUser?.isOwner && !isSelf);

        if (row.isPendingApproval) {
          return (
            <Stack direction="row" spacing={0.5} sx={{
              justifyContent: "flex-end"
            }}>
              {can("users.update") && (
                <Button size="small" variant="contained" startIcon={<CheckIcon />} onClick={() => setApproving(row)}>
                  Odobri
                </Button>
              )}
              {can("users.delete") && (
                <Tooltip title="Odbij zahtev">
                  <IconButton size="small" onClick={() => setRejecting(row)}>
                    <CloseIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              )}
            </Stack>
          );
        }

        if (locked && !isSelf) {
          return row.isOwner ? (
            <Stack direction="row" sx={{
              justifyContent: "flex-end"
            }}>
              <Chip size="small" variant="outlined" color="primary" label="Vlasnik" />
            </Stack>
          ) : (
            <Stack direction="row" sx={{
              justifyContent: "flex-end"
            }}>
              <Tooltip title="Samo vlasnik sistema moze menjati administratore">
                <Chip size="small" variant="outlined" label="Zakljucan" />
              </Tooltip>
            </Stack>
          );
        }

        return (
          <Stack
            direction="row"
            spacing={0.5}
            sx={{
              justifyContent: "flex-end",
              alignItems: "center"
            }}>
            {row.isOwner && <Chip size="small" variant="outlined" color="primary" label="Vlasnik" />}
            {can("users.update") && !isSelf && (
              <Tooltip title="Nova lozinka">
                <IconButton size="small" onClick={() => setResetting(row)}>
                  <KeyOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {can("users.update") && (
              <Tooltip title="Izmeni">
                <IconButton
                  size="small"
                  onClick={() => {
                    setEditing(row);
                    setFormOpen(true);
                  }}
                >
                  <EditOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        );
      },
    },
  ];

  return (
    <Box>
      <PageHeader
        title="Korisnici i uloge"
        description="Upravljanje nalozima, odobravanje zahteva za pristup i dodela dozvola po ulogama."
        actions={
          tab === "users" &&
          can("users.create") && (
            <Button
              variant="contained"
              onClick={() => {
                setEditing(null);
                setFormOpen(true);
              }}
            >
              Dodaj korisnika
            </Button>
          )
        }
      />

      <Tabs
        value={tab}
        onChange={(_, next) => setTab(next)}
        sx={{ mb: 2, borderBottom: 1, borderColor: "divider", "& .MuiTab-root": { minHeight: 48, px: 2.5 } }}
      >
        <Tab
          value="users"
          label={
            <Stack direction="row" sx={{ alignItems: "center", gap: 1 }}>
              Korisnici
              {pendingCount > 0 && (
                <Box
                  component="span"
                  title={`${pendingCount} na cekanju`}
                  sx={{
                    minWidth: 20,
                    height: 20,
                    px: 0.75,
                    borderRadius: 10,
                    bgcolor: "warning.main",
                    color: "warning.contrastText",
                    fontSize: "0.72rem",
                    fontWeight: 600,
                    lineHeight: "20px",
                    textAlign: "center",
                  }}
                >
                  {pendingCount}
                </Box>
              )}
            </Stack>
          }
        />
        <Tab value="roles" label="Uloge i dozvole" />
      </Tabs>

      {tab === "users" ? (
        <>
          {pendingCount > 0 && query.filter.status !== 1 && (
            <Alert
              severity="warning"
              sx={{ mb: 2 }}
              action={
                <Button color="inherit" size="small" onClick={() => query.patchFilter({ status: 1 })}>
                  Prikazi
                </Button>
              }
            >
              {pendingCount === 1
                ? "Jedan zahtev za pristup ceka odobrenje."
                : `${pendingCount} zahteva za pristup ceka odobrenje.`}
            </Alert>
          )}

          <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
            <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{
              alignItems: { md: "center" }
            }}>
              <SearchField
            label="Pretraga po imenu ili email-u"
            value={query.filter.search}
            onSearch={(value) => query.patchFilter({ search: value })}
            sx={{minWidth: 260 }}
          />
              <TextField
                select
                label="Uloga"
                value={query.filter.roleId ?? ""}
                onChange={(event) => query.patchFilter({ roleId: event.target.value })}
                sx={{ minWidth: 180 }}
              >
                <MenuItem value="">Sve uloge</MenuItem>
                {roles.map((role) => (
                  <MenuItem key={role.id} value={role.id}>
                    {role.name}
                  </MenuItem>
                ))}
              </TextField>
              <Tabs
                value={query.filter.status ?? 0}
                onChange={(_, next) => query.patchFilter({ status: next })}
                sx={{ minHeight: 36, "& .MuiTab-root": { minHeight: 36, py: 0.5 } }}
              >
                {statusTabs.map((option) => (
                  <Tab key={option.value} value={option.value} label={option.label} />
                ))}
              </Tabs>
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
            emptyTitle={query.filter.status === 1 ? "Nema zahteva na cekanju" : "Nema korisnika"}
          />
        </>
      ) : (
        <RolesPanel roles={roles} onEdit={setEditingRoleId} />
      )}

      <UserFormDialog
        open={formOpen}
        user={editing}
        roles={currentUser?.isOwner ? roles : roles.filter((role) => role.name !== "Admin" || editing?.roleName === "Admin")}
        isSelf={editing?.id === currentUser?.id}
        onClose={() => {
          setFormOpen(false);
          setEditing(null);
        }}
        onSaved={(message) => {
          setFormOpen(false);
          setEditing(null);
          afterChange(message);
        }}
      />

      <ApproveUserDialog
        user={approving}
        roles={currentUser?.isOwner ? roles : roles.filter((role) => role.name !== "Admin")}
        onClose={() => setApproving(null)}
        onSaved={(message) => {
          setApproving(null);
          afterChange(message);
        }}
      />

      <ResetPasswordDialog
        user={resetting}
        onClose={() => setResetting(null)}
        onSaved={(message) => {
          setResetting(null);
          toast.success(message);
        }}
      />

      <RoleEditDialog
        roleId={editingRoleId}
        tree={tree}
        onClose={() => setEditingRoleId(null)}
        onSaved={(message) => {
          setEditingRoleId(null);
          afterChange(message);
        }}
      />

      <ConfirmDialog
        open={Boolean(rejecting)}
        title="Odbijanje zahteva"
        message={`Zahtev korisnika ${rejecting?.firstName} ${rejecting?.lastName} bice obrisan. Moci ce da posalje novi zahtev.`}
        confirmLabel="Odbij"
        destructive
        loading={busy}
        onConfirm={confirmReject}
        onClose={() => setRejecting(null)}
      />
    </Box>
  );
}
