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
  // Zalihe svih proizvoda iz stavki po zonama centralnog magacina, za predlog izvorne lokacije
  const [warehouseStock, setWarehouseStock] = useState({});
  const productKey = [...new Set(lines.map((line) => Number(line.productId)).filter(Boolean))].sort().join(",");

  useEffect(() => {
    // bez stavki ili za ulazni nalog predlog se ne racuna, pa stari podaci ne smetaju
    if (!open || isInbound || productKey === "") return;

    let active = true;
    const ids = productKey.split(",").map(Number);
    Promise.all(ids.map((productId) => stockApi.list({ productId, locationType: 1, pageSize: 100 })))
      .then((results) => {
        if (!active) return;
        const next = {};
        ids.forEach((productId, index) => {
          next[productId] = Object.fromEntries(
            results[index].items.map((item) => [item.storageLocationId, item.quantity])
          );
        });
        setWarehouseStock(next);
      })
      .catch(() => active && setWarehouseStock({}));

    return () => {
      active = false;
    };
  }, [open, isInbound, productKey]);

  const warehouseLocations = useMemo(
    () => locations.filter((location) => location.locationType === 1),
    [locations]
  );

  const requested = lines
    .filter((line) => line.productId !== "")
    .map((line) => ({ productId: Number(line.productId), quantity: Number(line.quantity) || 0 }));
  const loaded = requested.length > 0 && requested.every((line) => warehouseStock[line.productId]);

  const coverageOf = (locationId) => {
    let full = 0;
    let some = 0;
    requested.forEach((line) => {
      const qty = warehouseStock[line.productId]?.[locationId] ?? 0;
      if (qty > 0) some += 1;
      if (qty > 0 && qty >= line.quantity) full += 1;
    });
    return { full, some };
  };

  const suggestion = useMemo(() => {
    if (isInbound || !loaded) return null;
    const ranked = warehouseLocations
      .map((location) => ({
        location,
        ...coverageOf(location.id),
        firstQty: warehouseStock[requested[0].productId]?.[location.id] ?? 0,
      }))
      .filter((entry) => entry.some > 0)
      .sort((a, b) => b.full - a.full || b.some - a.some || b.firstQty - a.firstQty);
    return ranked[0]?.location ?? null;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isInbound, loaded, warehouseStock, warehouseLocations, lines]);

  const missingEverywhere = loaded
    ? requested
        .filter((line) => !Object.values(warehouseStock[line.productId] ?? {}).some((qty) => qty > 0))
        .map((line) => products.find((product) => product.id === line.productId)?.name)
        .filter(Boolean)
    : [];

  // Dok korisnik sam ne izabere izvornu lokaciju, koristi se predlog
  const autoSource = !isInbound && sourceLocationId === "" && Boolean(suggestion);
  const effectiveSourceId = autoSource ? suggestion.id : sourceLocationId;

  const [available, setAvailable] = useState({});

  useEffect(() => {
    if (!open || isInbound || effectiveSourceId === "") {
      setAvailable({});
      return;
    }

    let active = true;
    stockApi
      .list({ storageLocationId: effectiveSourceId, pageSize: 100 })
      .then((result) => {
        if (!active) return;
        setAvailable(Object.fromEntries(result.items.map((item) => [item.productId, item.quantity])));
      })
      .catch(() => active && setAvailable({}));

    return () => {
      active = false;
    };
  }, [open, isInbound, effectiveSourceId]);

  const showStock = !isInbound && effectiveSourceId !== "";
  const stockOf = (productId) => available[Number(productId)] ?? 0;
  const usedIds = new Set(lines.map((line) => Number(line.productId)).filter(Boolean));
  const shortLines = showStock
    ? lines.filter((line) => line.productId !== "" && Number(line.quantity) > stockOf(line.productId)).length
    : 0;


  const coverageLabel = (locationId) => {
    if (requested.length === 0 || !loaded) return null;
    if (requested.length === 1) {
      const product = products.find((item) => item.id === requested[0].productId);
      const qty = warehouseStock[requested[0].productId]?.[locationId] ?? 0;
      return {
        text: `${formatQuantity(qty)} ${unitLabels[product?.unitOfMeasure] ?? ""}`.trim(),
        tone: qty <= 0 ? "text.disabled" : qty >= requested[0].quantity ? "success.main" : "warning.main",
      };
    }
    const { full, some } = coverageOf(locationId);
    return {
      text: `${full}/${requested.length} stavki`,
      tone: full === requested.length ? "success.main" : some > 0 ? "warning.main" : "text.disabled",
    };
  };

  const locationsOfStore = (id) =>
    id === "" ? [] : locations.filter((location) => location.locationType === 2 && location.storeId === Number(id));

  const storeLocations = locationsOfStore(storeId);

  const onlyId = (list) => (list.length === 1 ? list[0].id : "");

  const handleTypeChange = (value) => {
    const warehouses = locations.filter((location) => location.locationType === 1);
    setOrderType(value);
    setSupplierId("");
    setStoreId("");
    if (Number(value) === 1) {
      setSourceLocationId("");
      setDestinationLocationId(onlyId(warehouses));
    } else {
      setSourceLocationId(onlyId(warehouses));
      setDestinationLocationId("");
    }
  };

  const handleStoreChange = (value) => {
    setStoreId(value);
    const own = locationsOfStore(value);
    setDestinationLocationId((current) =>
      own.some((location) => location.id === Number(current)) ? current : onlyId(own)
    );
  };

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
      setDestinationLocationId(
        initial.destinationLocationId ?? onlyId(locationsOfStore(initial.storeId ?? ""))
      );
      setNote(initial.note ?? "");
      setLines(
        initial.lines?.length
          ? initial.lines.map((line) => ({
              productId: line.productId,
              quantity: line.quantity,
              unitPrice: products.find((product) => product.id === Number(line.productId))?.price ?? "",
            }))
          : [{ ...emptyLine }]
      );
    } else {
      setOrderType(1);
      setSupplierId("");
      setStoreId("");
      setSourceLocationId("");
      setDestinationLocationId(onlyId(locations.filter((location) => location.locationType === 1)));
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
      sourceLocationId: !isInbound && effectiveSourceId !== "" ? Number(effectiveSourceId) : null,
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
          onChange={(event) => handleTypeChange(event.target.value)}
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
            onChange={(event) => handleStoreChange(event.target.value)}
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
            value={effectiveSourceId}
            onChange={(event) => setSourceLocationId(event.target.value)}
            required
            fullWidth
            helperText={
              autoSource
                ? "Predlog: zona sa najvise trazene robe"
                : "Odakle se roba skida"
            }
            slotProps={{
              select: {
                renderValue: (selected) => {
                  const location = warehouseLocations.find((item) => item.id === Number(selected));
                  return location ? `${location.code} — ${location.name}` : "";
                },
              },
            }}
          >
            {warehouseLocations.map((location) => {
              const coverage = coverageLabel(location.id);
              return (
                <MenuItem key={location.id} value={location.id} sx={{ gap: 2 }}>
                  <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                    {location.code} — {location.name}
                  </Box>
                  {coverage && (
                    <Typography sx={{ fontFamily: monoFont, fontSize: "0.75rem", color: coverage.tone, whiteSpace: "nowrap" }}>
                      {coverage.text}
                    </Typography>
                  )}
                </MenuItem>
              );
            })}
          </TextField>
        )}

        <TextField
          select
          label="Odredisna lokacija"
          value={destinationLocationId}
          onChange={(event) => setDestinationLocationId(event.target.value)}
          required
          fullWidth
          disabled={!isInbound && storeId === ""}
          helperText={
            !isInbound && storeId === ""
              ? "Prvo izaberite prodajni objekat"
              : !isInbound && storeLocations.length > 1
                ? "Objekat ima vise lokacija — izaberite gde roba staje"
                : "Kuda roba ide"
          }
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

        {!isInbound && missingEverywhere.length > 0 && (
          <Alert
            severity="info"
            variant="outlined"
            sx={{ mb: 1.5 }}
            action={
              !order && (
                <Button color="inherit" size="small" sx={{ whiteSpace: "nowrap" }} onClick={() => handleTypeChange(1)}>
                  Napravi ulazni nalog
                </Button>
              )
            }
          >
            {missingEverywhere.length === 1
              ? `Artikla „${missingEverywhere[0]}“ nema ni u jednoj zoni centralnog magacina.`
              : `Artikala ${missingEverywhere.map((name) => `„${name}“`).join(", ")} nema ni u jednoj zoni centralnog magacina.`}{" "}
            Prvo je potrebno napraviti ulazni nalog od dobavljaca.
          </Alert>
        )}

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
