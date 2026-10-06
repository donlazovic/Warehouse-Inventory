import { Alert, Box, MenuItem, Stack, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { stockApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { formatQuantity, unitLabels } from "../../utils/format";
import FormDialog from "../common/FormDialog";

export default function AdjustStockDialog({ open, row, products, locations, onClose, onSaved }) {
  const [productId, setProductId] = useState("");
  const [locationId, setLocationId] = useState("");
  const [newQuantity, setNewQuantity] = useState("");
  const [note, setNote] = useState("");
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setNote("");
    setProductId(row?.productId ?? "");
    setLocationId(row?.storageLocationId ?? "");
    setNewQuantity(row ? String(row.quantity) : "");
  }, [open, row]);

  const current = row?.quantity ?? 0;
  const difference = newQuantity === "" ? 0 : Number(newQuantity) - current;
  const unit = unitLabels[row?.unitOfMeasure ?? products.find((p) => p.id === Number(productId))?.unitOfMeasure];

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      await stockApi.adjust({
        productId: Number(productId),
        storageLocationId: Number(locationId),
        newQuantity: Number(newQuantity),
        note: note.trim(),
      });
      onSaved(row ? "Stanje je korigovano." : "Stanje je upisano.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={row ? "Korekcija stanja" : "Unos stanja"}
      submitLabel={row ? "Sacuvaj korekciju" : "Upisi stanje"}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      {row ? (
        <Box>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>
            {row.productName}
          </Typography>
          <Typography variant="body2" sx={{
            color: "text.secondary"
          }}>
            {row.locationCode} — {row.locationName}
          </Typography>
        </Box>
      ) : (
        <>
          <Typography variant="body2" sx={{
            color: "text.secondary"
          }}>
            Koristite za pocetno stanje artikla na lokaciji ili za uskladjivanje posle popisa. Robu
            od dobavljaca unosite kroz ulazni nalog.
          </Typography>
          <TextField select label="Proizvod" value={productId} onChange={(e) => setProductId(e.target.value)} required fullWidth>
            {products.map((product) => (
              <MenuItem key={product.id} value={product.id}>
                {product.sku} — {product.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Lokacija" value={locationId} onChange={(e) => setLocationId(e.target.value)} required fullWidth>
            {locations.map((location) => (
              <MenuItem key={location.id} value={location.id}>
                {location.code} — {location.name}
              </MenuItem>
            ))}
          </TextField>
        </>
      )}

      <Stack direction="row" spacing={2} sx={{
        alignItems: "flex-start"
      }}>
        {row && (
          <TextField
            label="Trenutno"
            value={`${formatQuantity(current)} ${unit ?? ""}`}
            disabled
            sx={{ width: 160 }}
          />
        )}
        <TextField
          label={row ? "Novo stanje" : "Kolicina"}
          type="number"
          value={newQuantity}
          onChange={(e) => setNewQuantity(e.target.value)}
          required
          autoFocus={Boolean(row)}
          fullWidth
          slotProps={{ htmlInput: { min: 0, step: "0.001" } }}
        />
      </Stack>

      {row && newQuantity !== "" && difference !== 0 && (
        <Alert severity={difference < 0 ? "warning" : "info"} variant="outlined">
          Razlika:{" "}
          <Box component="span" sx={{ fontFamily: monoFont, fontWeight: 600 }}>
            {difference > 0 ? "+" : ""}
            {formatQuantity(difference)} {unit}
          </Box>
        </Alert>
      )}

      <TextField
        label="Razlog"
        value={note}
        onChange={(e) => setNote(e.target.value)}
        required
        multiline
        minRows={2}
        fullWidth
        helperText="Obavezno. Npr. „Popis 30.09 — utvrdjen manjak” ili „Pocetno stanje pri uvodjenju sistema”."
      />
    </FormDialog>
  );
}
