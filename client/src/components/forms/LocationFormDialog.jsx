import { MenuItem, Stack, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { locationsApi } from "../../api/endpoints";
import FormDialog from "../common/FormDialog";

const emptyForm = {
  code: "",
  name: "",
  zone: "",
  locationType: 1,
  storeId: "",
  isActive: true,
};

export default function LocationFormDialog({ open, location, stores, onClose, onSaved }) {
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setForm(
      location
        ? {
            code: location.code,
            name: location.name,
            zone: location.zone ?? "",
            locationType: location.locationType,
            storeId: location.storeId ?? "",
            isActive: location.isActive,
          }
        : emptyForm
    );
  }, [open, location]);

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const isStore = Number(form.locationType) === 2;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    const payload = {
      code: form.code.trim(),
      name: form.name.trim(),
      zone: form.zone.trim() || null,
      locationType: Number(form.locationType),
      storeId: isStore && form.storeId !== "" ? Number(form.storeId) : null,
      isActive: form.isActive,
    };

    try {
      if (location) {
        await locationsApi.update(location.id, payload);
      } else {
        await locationsApi.create(payload);
      }
      onSaved(location ? "Lokacija je izmenjena." : "Lokacija je dodata.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={location ? "Izmena lokacije" : "Nova lokacija"}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Stack direction="row" spacing={2}>
        <TextField
          label="Sifra"
          value={form.code}
          onChange={setField("code")}
          required
          sx={{ width: 180 }}
          helperText="Npr. CW-01"
        />
        <TextField label="Naziv" value={form.name} onChange={setField("name")} required fullWidth autoFocus />
      </Stack>

      <Stack direction="row" spacing={2}>
        <TextField
          select
          label="Tip lokacije"
          value={form.locationType}
          onChange={setField("locationType")}
          required
          fullWidth
        >
          <MenuItem value={1}>Centralni magacin</MenuItem>
          <MenuItem value={2}>Prodajni objekat</MenuItem>
        </TextField>

        <TextField
          label="Zona"
          value={form.zone}
          onChange={setField("zone")}
          sx={{ width: 160 }}
          helperText="Opciono"
        />
      </Stack>

      {isStore && (
        <TextField
          select
          label="Prodajni objekat"
          value={form.storeId}
          onChange={setField("storeId")}
          required
          fullWidth
          helperText="Lokacija tipa objekta mora pripadati nekom objektu"
        >
          {stores.map((store) => (
            <MenuItem key={store.id} value={store.id}>
              {store.code} — {store.name}
            </MenuItem>
          ))}
        </TextField>
      )}

      <TextField
        select
        label="Status"
        value={form.isActive}
        onChange={(event) =>
          setForm((current) => ({ ...current, isActive: event.target.value === "true" }))
        }
        sx={{ width: 200 }}
      >
        <MenuItem value="true">Aktivna</MenuItem>
        <MenuItem value="false">Neaktivna</MenuItem>
      </TextField>
    </FormDialog>
  );
}
