import { MenuItem, Stack, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import FormDialog from "../common/FormDialog";

const supplierFields = {
  emptyForm: {
    name: "",
    taxNumber: "",
    contactPerson: "",
    email: "",
    phone: "",
    address: "",
    city: "",
    isActive: true,
  },
  title: (editing) => (editing ? "Izmena dobavljaca" : "Novi dobavljac"),
  saved: (editing) => (editing ? "Dobavljac je izmenjen." : "Dobavljac je dodat."),
};

const storeFields = {
  emptyForm: {
    code: "",
    name: "",
    managerName: "",
    email: "",
    phone: "",
    address: "",
    city: "",
    isActive: true,
  },
  title: (editing) => (editing ? "Izmena objekta" : "Novi prodajni objekat"),
  saved: (editing) => (editing ? "Objekat je izmenjen." : "Objekat je dodat."),
};

export default function PartnerFormDialog({ open, kind, entity, api, onClose, onSaved }) {
  const config = kind === "store" ? storeFields : supplierFields;

  const [form, setForm] = useState(config.emptyForm);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);

    if (!entity) {
      setForm(config.emptyForm);
      return;
    }

    setForm(
      Object.fromEntries(
        Object.keys(config.emptyForm).map((key) => [key, entity[key] ?? (key === "isActive" ? true : "")])
      )
    );
  }, [open, entity, config.emptyForm]);

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    const payload = Object.fromEntries(
      Object.entries(form).map(([key, value]) =>
        key === "isActive" ? [key, value] : [key, typeof value === "string" ? value.trim() || null : value]
      )
    );

    try {
      if (entity) {
        await api.update(entity.id, payload);
      } else {
        await api.create(payload);
      }
      onSaved(config.saved(Boolean(entity)));
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={config.title(Boolean(entity))}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Stack direction="row" spacing={2}>
        {kind === "store" && (
          <TextField
            label="Sifra"
            value={form.code ?? ""}
            onChange={setField("code")}
            required
            sx={{ width: 160 }}
            helperText="Npr. NIS-01"
          />
        )}
        <TextField label="Naziv" value={form.name} onChange={setField("name")} required fullWidth autoFocus />
      </Stack>

      <Stack direction="row" spacing={2}>
        {kind === "store" ? (
          <TextField
            label="Odgovorno lice"
            value={form.managerName ?? ""}
            onChange={setField("managerName")}
            fullWidth
          />
        ) : (
          <>
            <TextField
              label="PIB"
              value={form.taxNumber ?? ""}
              onChange={setField("taxNumber")}
              sx={{ width: 160 }}
            />
            <TextField
              label="Kontakt osoba"
              value={form.contactPerson ?? ""}
              onChange={setField("contactPerson")}
              fullWidth
            />
          </>
        )}
      </Stack>

      <Stack direction="row" spacing={2}>
        <TextField label="Email" type="email" value={form.email ?? ""} onChange={setField("email")} fullWidth />
        <TextField label="Telefon" value={form.phone ?? ""} onChange={setField("phone")} fullWidth />
      </Stack>

      <Stack direction="row" spacing={2}>
        <TextField label="Adresa" value={form.address ?? ""} onChange={setField("address")} fullWidth />
        <TextField label="Grad" value={form.city ?? ""} onChange={setField("city")} sx={{ width: 200 }} />
      </Stack>

      <TextField
        select
        label="Status"
        value={form.isActive}
        onChange={(event) =>
          setForm((current) => ({ ...current, isActive: event.target.value === "true" }))
        }
        sx={{ width: 200 }}
      >
        <MenuItem value="true">Aktivan</MenuItem>
        <MenuItem value="false">Neaktivan</MenuItem>
      </TextField>
    </FormDialog>
  );
}
