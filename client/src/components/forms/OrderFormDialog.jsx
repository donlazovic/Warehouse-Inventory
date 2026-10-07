import AddIcon from "@mui/icons-material/Add";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import {
  Alert,
  Autocomplete,
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
import { ordersApi, stockApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { formatMoney, formatQuantity, unitLabels } from "../../utils/format";
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
  const [available, setAvailable] = useState({});

  useEffect(() => {
    if (!open || isInbound || sourceLocationId === "") {
      setAvailable({});
      return;
    }

    let active = true;
    stockApi
      .list({ storageLocationId: sourceLocationId, pageSize: 100 })
      .then((result) => {
        if (!active) return;
        setAvailable(Object.fromEntries(result.items.map((item) => [item.productId, item.quantity])));
      })
      .catch(() => active && setAvailable({}));

    return () => {
      active = false;
    };
  }, [open, isInbound, sourceLocationId]);

  const showStock = !isInbound && sourceLocationId !== "";
  const stockOf = (productId) => available[Number(productId)] ?? 0;
  const usedIds = new Set(lines.map((line) => Number(line.productId)).filter(Boolean));
  const shortLines = showStock
    ? lines.filter((line) => line.productId !== "" && Number(line.quantity) > stockOf(line.productId)).length
    : 0;

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
    if (productId === "") {
      setLine(index, "productId", "");
      return;
    }
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
            onClick={() => setLines((current) => [...current, { ...emptyLine }])}
          >
            Dodaj stavku
          </Button>
        </Stack>

        {shortLines > 0 && (
          <Alert severity="warning" variant="outlined" sx={{ mb: 1.5 }}>
            {shortLines === 1 ? "Za jednu stavku" : `Za ${shortLines} stavke`} nema dovoljno robe na izvornoj
            lokaciji. Nalog moze da se sacuva, ali nece moci da predje u realizaciju dok se zalihe ne dopune.
          </Alert>
        )}

        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Proizvod</TableCell>
              {showStock && (
                <TableCell align="right" sx={{ width: 110 }}>
                  Na stanju
                </TableCell>
              )}
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
                  <Autocomplete
                    size="small"
                    options={products}
                    value={products.find((product) => product.id === Number(line.productId)) ?? null}
                    onChange={(_, product) => handleProductChange(index, product?.id ?? "")}
                    getOptionLabel={(product) => `${product.sku} — ${product.name}`}
                    isOptionEqualToValue={(option, value) => option.id === value.id}
                    getOptionDisabled={(product) => usedIds.has(product.id) && product.id !== Number(line.productId)}
                    noOptionsText="Nema proizvoda za tu pretragu"
                    renderOption={(props, product) => {
                      const { key, ...rest } = props;
                      const qty = stockOf(product.id);
                      return (
                        <Box component="li" key={key} {...rest} sx={{ display: "flex", gap: 2 }}>
                          <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                            <Typography variant="body2" noWrap>
                              {product.name}
                            </Typography>
                            <Typography sx={{ fontFamily: monoFont, fontSize: "0.72rem", color: "text.secondary" }}>
                              {product.sku}
                              {usedIds.has(product.id) && product.id !== Number(line.productId) ? " · vec dodat" : ""}
                            </Typography>
                          </Box>
                          {showStock && (
                            <Typography
                              sx={{
                                fontFamily: monoFont,
                                fontSize: "0.75rem",
                                whiteSpace: "nowrap",
                                color: qty > 0 ? "text.secondary" : "error.main",
                              }}
                            >
                              {formatQuantity(qty)} {unitLabels[product.unitOfMeasure]}
                            </Typography>
                          )}
                        </Box>
                      );
                    }}
                    renderInput={(params) => <TextField {...params} placeholder="Pretrazi po nazivu ili SKU" />}
                    sx={{ minWidth: 240 }}
                  />
                </TableCell>
                {showStock && (
                  <TableCell align="right" sx={{ fontFamily: monoFont, fontSize: "0.8rem", whiteSpace: "nowrap" }}>
                    {line.productId === "" ? (
                      "—"
                    ) : (
                      <Box
                        component="span"
                        sx={{ color: Number(line.quantity) > stockOf(line.productId) ? "error.main" : "text.secondary" }}
                      >
                        {formatQuantity(stockOf(line.productId))}
                      </Box>
                    )}
                  </TableCell>
                )}
                <TableCell>
                  <TextField
                    type="number"
                    value={line.quantity}
                    onChange={(event) => setLine(index, "quantity", event.target.value)}
                    error={showStock && line.productId !== "" && Number(line.quantity) > stockOf(line.productId)}
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

        <Stack
          direction="row"
          sx={{
            justifyContent: "flex-end",
            mt: 2,
            pr: 7
          }}>
          <Typography variant="body2" sx={{ mr: 2, alignSelf: "center" }}>
            Ukupna vrednost
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontWeight: 600 }}>{formatMoney(total)}</Typography>
        </Stack>
      </Box>
    </FormDialog>
  );
}
