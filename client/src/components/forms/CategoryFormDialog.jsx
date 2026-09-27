import { MenuItem, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { categoriesApi } from "../../api/endpoints";
import FormDialog from "../common/FormDialog";

const emptyForm = { name: "", description: "", parentCategoryId: "", isActive: true };

export default function CategoryFormDialog({ open, category, categories, onClose, onSaved }) {
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setForm(
      category
        ? {
            name: category.name,
            description: category.description ?? "",
            parentCategoryId: category.parentCategoryId ?? "",
            isActive: category.isActive,
          }
        : emptyForm
    );
  }, [open, category]);

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    const payload = {
      name: form.name.trim(),
      description: form.description.trim() || null,
      parentCategoryId: form.parentCategoryId === "" ? null : Number(form.parentCategoryId),
    };

    try {
      if (category) {
        await categoriesApi.update(category.id, { ...payload, isActive: form.isActive });
      } else {
        await categoriesApi.create(payload);
      }
      onSaved(category ? "Kategorija je izmenjena." : "Kategorija je dodata.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const parentOptions = categories.filter((item) => item.id !== category?.id);

  return (
    <FormDialog
      open={open}
      title={category ? "Izmena kategorije" : "Nova kategorija"}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <TextField label="Naziv" value={form.name} onChange={setField("name")} required fullWidth autoFocus />

      <TextField
        label="Opis"
        value={form.description}
        onChange={setField("description")}
        multiline
        minRows={2}
        fullWidth
      />

      <TextField
        select
        label="Nadredjena kategorija"
        value={form.parentCategoryId}
        onChange={setField("parentCategoryId")}
        fullWidth
        helperText="Ostavite prazno za kategoriju najviseg nivoa"
      >
        <MenuItem value="">Bez nadredjene</MenuItem>
        {parentOptions.map((item) => (
          <MenuItem key={item.id} value={item.id}>
            {item.name}
          </MenuItem>
        ))}
      </TextField>

      {category && (
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
      )}
    </FormDialog>
  );
}
