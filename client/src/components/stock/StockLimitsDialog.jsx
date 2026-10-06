import { Stack, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { stockApi } from "../../api/endpoints";
import { formatQuantity, unitLabels } from "../../utils/format";
import FormDialog from "../common/FormDialog";

export default function StockLimitsDialog({ row, onClose, onSaved }) {
  const [min, setMin] = useState("");
  const [max, setMax] = useState("");
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!row) return;
    setError(null);
    setMin(row.minStockOverride ?? "");
    setMax(row.maxStockOverride ?? "");
  }, [row]);

  if (!row) return null;

  const unit = unitLabels[row.unitOfMeasure];
  const productMin = row.minStockOverride == null ? row.effectiveMinStock : null;
  const productMax = row.maxStockOverride == null ? row.effectiveMaxStock : null;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      await stockApi.setLimits(row.id, {
        minStockOverride: min === "" ? null : Number(min),
        maxStockOverride: max === "" ? null : Number(max),
      });
      onSaved("Granice zaliha su sacuvane.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open
      title="Granice zaliha za lokaciju"
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Typography variant="body2">
        <strong>{row.productName}</strong> na lokaciji <strong>{row.locationCode}</strong>. Prazno polje
        znaci da vazi vrednost iz kataloga proizvoda.
      </Typography>

      <Stack direction="row" spacing={2}>
        <TextField
          label="Minimalna zaliha"
          type="number"
          value={min}
          onChange={(e) => setMin(e.target.value)}
          fullWidth
          placeholder={productMin != null ? formatQuantity(productMin) : ""}
          helperText={productMin != null ? `Iz kataloga: ${formatQuantity(productMin)} ${unit}` : " "}
          slotProps={{ htmlInput: { min: 0, step: "0.001" }, inputLabel: { shrink: true } }}
        />
        <TextField
          label="Maksimalna zaliha"
          type="number"
          value={max}
          onChange={(e) => setMax(e.target.value)}
          fullWidth
          placeholder={productMax != null ? formatQuantity(productMax) : ""}
          helperText={productMax != null ? `Iz kataloga: ${formatQuantity(productMax)} ${unit}` : " "}
          slotProps={{ htmlInput: { min: 0, step: "0.001" }, inputLabel: { shrink: true } }}
        />
      </Stack>

      <Typography variant="body2" sx={{
        color: "text.secondary"
      }}>
        Prodavnica obicno drzi mnogo manje od centralnog magacina, pa joj je potreban niži prag
        upozorenja od onog u katalogu.
      </Typography>
    </FormDialog>
  );
}
