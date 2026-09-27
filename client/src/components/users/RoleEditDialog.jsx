import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { rolesApi } from "../../api/endpoints";
import PermissionTree from "./PermissionTree";

export default function RoleEditDialog({ roleId, tree, onClose, onSaved }) {
  const [detail, setDetail] = useState(null);
  const [selected, setSelected] = useState(new Set());
  const [description, setDescription] = useState("");
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!roleId) return;
    setDetail(null);
    setError(null);

    rolesApi
      .byId(roleId)
      .then((data) => {
        setDetail(data);
        setSelected(new Set(data.permissionIds));
        setDescription(data.role.description ?? "");
      })
      .catch((err) => setError(err.message));
  }, [roleId]);

  const initial = useMemo(() => new Set(detail?.permissionIds ?? []), [detail]);

  const changed = useMemo(() => {
    if (!detail) return 0;
    let count = 0;
    selected.forEach((id) => !initial.has(id) && count++);
    initial.forEach((id) => !selected.has(id) && count++);
    return count;
  }, [selected, initial, detail]);

  const totalLeaves = tree.reduce((sum, module) => sum + module.children.length, 0);
  const readOnly = detail?.role.isProtected ?? false;

  const handleSave = async () => {
    setSaving(true);
    setError(null);

    try {
      await rolesApi.update(roleId, {
        description: description.trim() || null,
        permissionIds: [...selected],
      });
      onSaved(`Dozvole uloge "${detail.role.name}" su sacuvane.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={Boolean(roleId)} onClose={saving ? undefined : onClose} maxWidth="md" fullWidth>
      <DialogTitle component="div">
        <Typography variant="h2">{detail ? `Uloga: ${detail.role.name}` : "Uloga"}</Typography>
        {detail && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            {detail.role.userCount} {detail.role.userCount === 1 ? "korisnik" : "korisnika"} · {selected.size} od{" "}
            {totalLeaves} dozvola
          </Typography>
        )}
      </DialogTitle>

      <DialogContent dividers>
        {!detail && !error && (
          <Box sx={{ display: "grid", placeItems: "center", py: 6 }}>
            <CircularProgress size={28} />
          </Box>
        )}

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {detail && (
          <Stack spacing={2}>
            {readOnly ? (
              <Alert severity="info">
                Administratorska uloga uvek ima sve dozvole i ne moze se menjati. Time se sprecava
                da sistem ostane bez ikoga ko upravlja korisnicima.
              </Alert>
            ) : (
              <Alert severity="warning" variant="outlined">
                Izmene vaze za prijavljene korisnike pri sledecem osvezavanju sesije, najkasnije za 15
                minuta.
              </Alert>
            )}

            <TextField
              label="Opis uloge"
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              disabled={readOnly}
              fullWidth
            />

            <PermissionTree tree={tree} selected={selected} onChange={setSelected} readOnly={readOnly} />
          </Stack>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2, justifyContent: "space-between" }}>
        <Typography variant="body2" color="text.secondary">
          {changed > 0 ? `${changed} ${changed === 1 ? "izmena" : "izmena"} nije sacuvano` : ""}
        </Typography>
        <Stack direction="row" spacing={1}>
          <Button onClick={onClose} disabled={saving}>
            {readOnly ? "Zatvori" : "Odustani"}
          </Button>
          {!readOnly && (
            <Button variant="contained" onClick={handleSave} disabled={saving || !detail}>
              {saving ? "Cuvanje..." : "Sacuvaj dozvole"}
            </Button>
          )}
        </Stack>
      </DialogActions>
    </Dialog>
  );
}
