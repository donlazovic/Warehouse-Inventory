import AddIcon from "@mui/icons-material/Add";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import {
  Box,
  Button,
  Divider,
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
import { useEffect, useMemo, useState } from "react";
import { ordersApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { formatMoney } from "../../utils/format";
import FormDialog from "../common/FormDialog";

const emptyLine = { productId: "", quantity: "", unitPrice: "" };

export default function OrderFormDialog({
  open,
  order,
  initial,
  products,
  suppliers,
  stores,
  locations,
  onClose,
  onSaved,
}) {
  const [orderType, setOrderType] = useState(1);
  const [supplierId, setSupplierId] = useState("");
  const [storeId, setStoreId] = useState("");
  const [sourceLocationId, setSourceLocationId] = useState("");
  const [destinationLocationId, setDestinationLocationId] = useState("");
  const [note, setNote] = useState("");
  const [lines, setLines] = useState([{ ...emptyLine }]);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const isInbound = Number(orderType) === 1;

  const warehouseLocations = useMemo(
    () => locations.filter((location) => location.locationType === 1),
    [locations]
  );

  const storeLocations = useMemo(
    () => locations.filter((location) => location.locationType === 2),
    [locations]
  );

  useEffect(() => {
    if (!open) return;
    setError(null);

    if (order) {
      setOrderType(order.order.orderType);
      setSupplierId(order.order.supplierId ?? "");
      setStoreId(order.order.storeId ?? "");
      setSourceLocationId(order.order.sourceLocationId ?? "");
      setDestinationLocationId(order.order.destinationLocationId ?? "");
      setNote(order.order.note ?? "");
      setLines(
        order.items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
        }))
      );
    } else if (initial) {
      const warehouses = locations.filter((location) => location.locationType === 1);
      setOrderType(initial.orderType ?? 2);
      setSupplierId("");
      setStoreId(initial.storeId ?? "");
      setSourceLocationId(initial.sourceLocationId ?? (warehouses.length === 1 ? warehouses[0].id : ""));
      setDestinationLocationId(initial.destinationLocationId ?? "");
      setNote(initial.note ?? "");
      setLines(
        initial.lines?.length
          ? initial.lines.map((line) => ({ productId: line.productId, quantity: line.quantity, unitPrice: "" }))
          : [{ ...emptyLine }]
      );
    } else {
      setOrderType(1);
      setSupplierId("");
      setStoreId("");
      setSourceLocationId("");
      setDestinationLocationId("");
      setNote("");
      setLines([{ ...emptyLine }]);
    }
  }, [open, order, initial]);

  const setLine = (index, field, value) =>
    setLines((current) =>
      current.map((line, position) => (position === index ? { ...line, [field]: value } : line))
    );

  const handleProductChange = (index, productId) => {
    if (lines.some((line, position) => position !== index && Number(line.productId) === Number(productId))) {
      setError("Taj proizvod je vec dodat. Povecajte kolicinu u postojecoj stavci.");
      return;
    }
    setError(null);
    const product = products.find((item) => item.id === Number(productId));
    setLines((current) =>
      current.map((line, position) =>
        position === index
          ? { ...line, productId, unitPrice: line.unitPrice || product?.price || "" }
          : line
      )
    );
  };

  const total = lines.reduce(
    (sum, line) => sum + (Number(line.quantity) || 0) * (Number(line.unitPrice) || 0),
    0
  );

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    const items = lines
      .filter((line) => line.productId !== "" && Number(line.quantity) > 0)
      .map((line) => ({
        productId: Number(line.productId),
        quantity: Number(line.quantity),
        unitPrice: line.unitPrice === "" ? null : Number(line.unitPrice),
      }));

    if (items.length === 0) {
      setError("Dodajte bar jednu stavku sa kolicinom vecom od nule.");
      return;
    }

    const payload = {
      supplierId: isInbound && supplierId !== "" ? Number(supplierId) : null,
      storeId: !isInbound && storeId !== "" ? Number(storeId) : null,
      sourceLocationId: !isInbound && sourceLocationId !== "" ? Number(sourceLocationId) : null,
      destinationLocationId: destinationLocationId !== "" ? Number(destinationLocationId) : null,
      note: note.trim() || null,
      items,
    };

    setSubmitting(true);

    try {
      if (order) {
        await ordersApi.update(order.order.id, payload);
      } else {
        await ordersApi.create({ ...payload, orderType: Number(orderType) });
      }
      onSaved(order ? "Nalog je izmenjen." : "Nalog je kreiran.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={order ? `Izmena naloga ${order.order.orderNumber}` : "Novi nalog"}
      submitLabel={order ? "Sacuvaj izmene" : "Kreiraj nalog"}
      error={error}
      submitting={submitting}
      maxWidth="md"
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Stack direction="row" spacing={2}>
        <TextField
          select
          label="Tip naloga"
          value={orderType}
          onChange={(event) => setOrderType(event.target.value)}
          disabled={Boolean(order)}
          sx={{ width: 200 }}
          helperText={order ? "Tip se ne menja" : "Ulazni od dobavljaca, izlazni ka objektu"}
        >
          <MenuItem value={1}>Ulazni</MenuItem>
          <MenuItem value={2}>Izlazni</MenuItem>
        </TextField>

        {isInbound ? (
          <TextField
            select
            label="Dobavljac"
            value={supplierId}
            onChange={(event) => setSupplierId(event.target.value)}
            required
            fullWidth
          >
            {suppliers.map((supplier) => (
              <MenuItem key={supplier.id} value={supplier.id}>
                {supplier.name}
              </MenuItem>
            ))}
          </TextField>
        ) : (
          <TextField
            select
            label="Prodajni objekat"
            value={storeId}
            onChange={(event) => setStoreId(event.target.value)}
            required
            fullWidth
          >
            {stores.map((store) => (
              <MenuItem key={store.id} value={store.id}>
                {store.code} — {store.name}
              </MenuItem>
            ))}
          </TextField>
        )}
      </Stack>

      <Stack direction="row" spacing={2}>
        {!isInbound && (
          <TextField
            select
            label="Izvorna lokacija"
            value={sourceLocationId}
            onChange={(event) => setSourceLocationId(event.target.value)}
            required
            fullWidth
            helperText="Odakle se roba skida"
          >
            {warehouseLocations.map((location) => (
              <MenuItem key={location.id} value={location.id}>
                {location.code} — {location.name}
              </MenuItem>
            ))}
          </TextField>
        )}

        <TextField
          select
          label="Odredisna lokacija"
          value={destinationLocationId}
          onChange={(event) => setDestinationLocationId(event.target.value)}
          required
          fullWidth
          helperText="Kuda roba ide"
        >
          {(isInbound ? warehouseLocations : storeLocations).map((location) => (
            <MenuItem key={location.id} value={location.id}>
              {location.code} — {location.name}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      <TextField
        label="Napomena"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        multiline
        minRows={2}
        fullWidth
      />

      <Divider />

      <Box>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
          <Typography variant="h3">Stavke</Typography>
          <Button
            size="small"
            startIcon={<AddIcon />}
            onClick={() => setLines((current) => [...current, { ...emptyLine }])}
          >
            Dodaj stavku
          </Button>
        </Stack>

        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Proizvod</TableCell>
              <TableCell sx={{ width: 130 }}>Kolicina</TableCell>
              <TableCell sx={{ width: 140 }}>Cena</TableCell>
              <TableCell align="right" sx={{ width: 120 }}>
                Ukupno
              </TableCell>
              <TableCell sx={{ width: 48 }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {lines.map((line, index) => (
              <TableRow key={index}>
                <TableCell>
                  <TextField
                    select
                    value={line.productId}
                    onChange={(event) => handleProductChange(index, event.target.value)}
                    fullWidth
                  >
                    {products.map((product) => (
                      <MenuItem key={product.id} value={product.id}>
                        {product.sku} — {product.name}
                      </MenuItem>
                    ))}
                  </TextField>
                </TableCell>
                <TableCell>
                  <TextField
                    type="number"
                    value={line.quantity}
                    onChange={(event) => setLine(index, "quantity", event.target.value)}
                    fullWidth
                  />
                </TableCell>
                <TableCell>
                  <TextField
                    type="number"
                    value={line.unitPrice}
                    onChange={(event) => setLine(index, "unitPrice", event.target.value)}
                    fullWidth
                  />
                </TableCell>
                <TableCell align="right" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
                  {formatMoney((Number(line.quantity) || 0) * (Number(line.unitPrice) || 0))}
                </TableCell>
                <TableCell>
                  <IconButton
                    size="small"
                    disabled={lines.length === 1}
                    onClick={() =>
                      setLines((current) => current.filter((_, position) => position !== index))
                    }
                  >
                    <DeleteOutlinedIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>

        <Stack direction="row" justifyContent="flex-end" sx={{ mt: 2, pr: 7 }}>
          <Typography variant="body2" sx={{ mr: 2, alignSelf: "center" }}>
            Ukupna vrednost
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontWeight: 600 }}>{formatMoney(total)}</Typography>
        </Stack>
      </Box>
    </FormDialog>
  );
}
