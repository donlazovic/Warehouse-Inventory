import AddIcon from "@mui/icons-material/Add";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import {
  Box,
  Button,
  IconButton,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { useEffect, useState } from "react";
import { stockApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { formatQuantity, issueReasonLabels, unitLabels } from "../../utils/format";
import FormDialog from "../common/FormDialog";

const emptyLine = { productId: "", quantity: "" };

export default function IssueStockDialog({ open, preset, locations, onClose, onSaved }) {
  const [locationId, setLocationId] = useState("");
  const [reason, setReason] = useState(1);
  const [note, setNote] = useState("");
  const [lines, setLines] = useState([{ ...emptyLine }]);
  const [available, setAvailable] = useState([]);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setNote("");
    setReason(1);
    setLocationId(preset?.storageLocationId ?? "");
    setLines(preset ? [{ productId: preset.productId, quantity: "" }] : [{ ...emptyLine }]);
  }, [open, preset]);

  useEffect(() => {
    if (!open || locationId === "") {
      setAvailable([]);
      return;
    }

    stockApi
      .list({ storageLocationId: locationId, onlyInStock: true, pageSize: 100, sortBy: "name" })
      .then((result) => setAvailable(result.items))
      .catch(() => setAvailable([]));
  }, [open, locationId]);

  const stockFor = (productId) => available.find((item) => item.productId === Number(productId));

  const setLine = (index, field, value) =>
    setLines((current) => current.map((line, i) => (i === index ? { ...line, [field]: value } : line)));

  const usedProducts = new Set(lines.map((line) => Number(line.productId)).filter(Boolean));

  const overLimit = lines.some((line) => {
    const stock = stockFor(line.productId);
    return stock && Number(line.quantity) > stock.quantity;
  });

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    const items = lines
      .filter((line) => line.productId !== "" && Number(line.quantity) > 0)
      .map((line) => ({ productId: Number(line.productId), quantity: Number(line.quantity) }));

    if (items.length === 0) {
      setError("Dodajte bar jednu stavku sa kolicinom vecom od nule.");
      return;
    }

    if (overLimit) {
      setError("Kolicina ne moze biti veca od stanja na lokaciji.");
      return;
    }

    setSubmitting(true);

    try {
      await stockApi.issue({
        storageLocationId: Number(locationId),
        reason: Number(reason),
        note: note.trim() || null,
        items,
      });
      onSaved(items.length === 1 ? "Izlaz je evidentiran." : `Evidentirano ${items.length} stavki izlaza.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title="Izlaz robe"
      submitLabel="Evidentiraj izlaz"
      error={error}
      submitting={submitting}
      maxWidth="md"
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Typography variant="body2" sx={{
        color: "text.secondary"
      }}>
        Roba koja napusta sistem — prodaja u objektu, otpis zbog isteka roka, lom ili interna
        potrosnja. Za prenos izmedju lokacija koristite izlazni nalog.
      </Typography>

      <Stack direction="row" spacing={2}>
        <TextField
          select
          label="Lokacija"
          value={locationId}
          onChange={(e) => {
            setLocationId(e.target.value);
            setLines([{ ...emptyLine }]);
          }}
          required
          fullWidth
          disabled={Boolean(preset)}
        >
          {locations.map((location) => (
            <MenuItem key={location.id} value={location.id}>
              {location.code} — {location.name}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          label="Razlog"
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          required
          sx={{ width: 240 }}
        >
          {Object.entries(issueReasonLabels).map(([value, label]) => (
            <MenuItem key={value} value={Number(value)}>
              {label}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      <TextField
        label="Napomena"
        value={note}
        onChange={(e) => setNote(e.target.value)}
        required={Number(reason) !== 1}
        multiline
        minRows={2}
        fullWidth
        helperText={Number(reason) !== 1 ? "Obavezno za otpis, lom i internu potrosnju" : "Opciono za prodaju"}
      />

      <Box>
        <Stack
          direction="row"
          sx={{
            justifyContent: "space-between",
            alignItems: "center",
            mb: 1
          }}>
          <Typography variant="h3">Stavke</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            disabled={locationId === ""}
            onClick={() => setLines((current) => [...current, { ...emptyLine }])}
          >
            Dodaj stavku
          </Button>
        </Stack>

        {locationId === "" ? (
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              py: 2
            }}>
            Izaberite lokaciju da biste videli artikle koji su na stanju.
          </Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Proizvod</TableCell>
                <TableCell align="right" sx={{ width: 140 }}>
                  Na stanju
                </TableCell>
                <TableCell sx={{ width: 150 }}>Kolicina</TableCell>
                <TableCell sx={{ width: 48 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {lines.map((line, index) => {
                const stock = stockFor(line.productId);
                const tooMuch = stock && Number(line.quantity) > stock.quantity;

                return (
                  <TableRow key={index}>
                    <TableCell>
                      <TextField
                        select
                        value={line.productId}
                        onChange={(e) => setLine(index, "productId", e.target.value)}
                        fullWidth
                      >
                        {available
                          .filter(
                            (item) =>
                              !usedProducts.has(item.productId) || item.productId === Number(line.productId)
                          )
                          .map((item) => (
                            <MenuItem key={item.productId} value={item.productId}>
                              {item.productSku} — {item.productName}
                            </MenuItem>
                          ))}
                      </TextField>
                    </TableCell>
                    <TableCell align="right">
                      {stock ? (
                        <>
                          <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
                            {formatQuantity(stock.quantity)}
                          </Box>
                          <Box component="span" sx={{ ml: 0.5, fontSize: "0.8rem", color: "text.secondary" }}>
                            {unitLabels[stock.unitOfMeasure]}
                          </Box>
                        </>
                      ) : (
                        "—"
                      )}
                    </TableCell>
                    <TableCell>
                      <TextField
                        type="number"
                        value={line.quantity}
                        onChange={(e) => setLine(index, "quantity", e.target.value)}
                        error={Boolean(tooMuch)}
                        fullWidth
                        slotProps={{ htmlInput: { min: 0, step: "0.001", max: stock?.quantity } }}
                      />
                    </TableCell>
                    <TableCell>
                      <IconButton
                        size="small"
                        disabled={lines.length === 1}
                        onClick={() => setLines((current) => current.filter((_, i) => i !== index))}
                      >
                        <DeleteOutlinedIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        )}

        {overLimit && (
          <Typography variant="body2" sx={{ color: "error.main", mt: 1 }}>
            Kolicina ne moze biti veca od stanja na lokaciji.
          </Typography>
        )}
      </Box>
    </FormDialog>
  );
}
