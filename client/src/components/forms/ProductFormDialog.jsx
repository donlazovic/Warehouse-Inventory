import { MenuItem, Stack, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { productsApi } from "../../api/endpoints";
import { unitLabels } from "../../utils/format";
import FormDialog from "../common/FormDialog";

const emptyForm = {
  sku: "",
  name: "",
  description: "",
  unitOfMeasure: 1,
  price: "",
  minStock: "",
  maxStock: "",
  categoryId: "",
  isActive: true,
};

export default function ProductFormDialog({ open, product, categories, onClose, onSaved }) {
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setForm(
      product
        ? {
            sku: product.sku,
            name: product.name,
            description: product.description ?? "",
            unitOfMeasure: product.unitOfMeasure,
            price: product.price,
            minStock: product.minStock,
            maxStock: product.maxStock,
            categoryId: product.categoryId,
            isActive: product.isActive,
          }
        : emptyForm
    );
  }, [open, product]);

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    const payload = {
      sku: form.sku.trim(),
      name: form.name.trim(),
      description: form.description.trim() || null,
      unitOfMeasure: Number(form.unitOfMeasure),
      price: Number(form.price),
      minStock: Number(form.minStock),
      maxStock: Number(form.maxStock),
      categoryId: Number(form.categoryId),
    };

    try {
      if (product) {
        await productsApi.update(product.id, { ...payload, isActive: form.isActive });
      } else {
        await productsApi.create(payload);
      }
      onSaved(product ? "Proizvod je izmenjen." : "Proizvod je dodat.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={product ? "Izmena proizvoda" : "Novi proizvod"}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Stack direction="row" spacing={2}>
        <TextField
          label="SKU"
          value={form.sku}
          onChange={setField("sku")}
          required
          sx={{ width: 180 }}
          helperText="Jedinstvena sifra artikla"
        />
        <TextField label="Naziv" value={form.name} onChange={setField("name")} required fullWidth />
      </Stack>

      <TextField
        label="Opis"
        value={form.description}
        onChange={setField("description")}
        multiline
        minRows={2}
        fullWidth
      />

      <Stack direction="row" spacing={2}>
        <TextField
          select
          label="Kategorija"
          value={form.categoryId}
          onChange={setField("categoryId")}
          required
          fullWidth
        >
          {categories.map((category) => (
            <MenuItem key={category.id} value={category.id}>
              {category.name}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          label="Jedinica mere"
          value={form.unitOfMeasure}
          onChange={setField("unitOfMeasure")}
          required
          sx={{ width: 180 }}
        >
          {Object.entries(unitLabels).map(([value, label]) => (
            <MenuItem key={value} value={Number(value)}>
              {label}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      <Stack direction="row" spacing={2}>
        <TextField
          label="Cena"
          type="number"
          value={form.price}
          onChange={setField("price")}
          required
          inputProps={{ min: 0, step: "0.01" }}
          fullWidth
        />
        <TextField
          label="Minimalna zaliha"
          type="number"
          value={form.minStock}
          onChange={setField("minStock")}
          required
          inputProps={{ min: 0, step: "0.001" }}
          fullWidth
          helperText="Ispod ove kolicine sistem javlja upozorenje"
        />
        <TextField
          label="Maksimalna zaliha"
          type="number"
          value={form.maxStock}
          onChange={setField("maxStock")}
          required
          inputProps={{ min: 0, step: "0.001" }}
          fullWidth
        />
      </Stack>

      {product && (
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
      )}
    </FormDialog>
  );
}
