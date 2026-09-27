import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { monoFont, statusColors } from "../../theme";
import {
  formatDateTime,
  formatMoney,
  formatQuantity,
  orderStatusLabels,
  orderTypeLabels,
  unitLabels,
} from "../../utils/format";

const Field = ({ label, value }) => (
  <Box>
    <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
      {label}
    </Typography>
    <Typography variant="body2">{value ?? "—"}</Typography>
  </Box>
);

export default function OrderDetailDialog({ detail, canMoveTo, onChangeStatus, onEdit, onClose }) {
  if (!detail) return null;

  const { order, items, history, allowedNextStatuses } = detail;

  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle component="div">
        <Stack direction="row" alignItems="center" spacing={1.5}>
          <Typography sx={{ fontFamily: monoFont, fontSize: "1.05rem", fontWeight: 600 }}>
            {order.orderNumber}
          </Typography>
          <Chip
            size="small"
            label={orderStatusLabels[order.status]}
            sx={{ bgcolor: statusColors[order.status], color: "#FFFFFF" }}
          />
          <Chip size="small" variant="outlined" label={orderTypeLabels[order.orderType]} />
        </Stack>
      </DialogTitle>

      <DialogContent dividers>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr 1fr", md: "repeat(4, 1fr)" },
            gap: 2,
            mb: 3,
          }}
        >
          <Field label={order.orderType === 1 ? "Dobavljac" : "Objekat"} value={order.supplierName ?? order.storeName} />
          <Field label="Sa lokacije" value={order.sourceLocationName} />
          <Field label="Na lokaciju" value={order.destinationLocationName} />
          <Field label="Vrednost" value={formatMoney(order.totalValue)} />
          <Field label="Kreirao" value={order.createdByName} />
          <Field label="Kreiran" value={formatDateTime(order.createdAt)} />
          <Field label="Odobrio" value={order.approvedByName} />
          <Field label="Realizovan" value={formatDateTime(order.completedAt)} />
        </Box>

        {order.note && (
          <Box sx={{ mb: 3 }}>
            <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
              Napomena
            </Typography>
            <Typography variant="body2">{order.note}</Typography>
          </Box>
        )}

        <Typography variant="h3" sx={{ mb: 1 }}>
          Stavke
        </Typography>

        <Table size="small" sx={{ mb: 3 }}>
          <TableHead>
            <TableRow>
              <TableCell>SKU</TableCell>
              <TableCell>Proizvod</TableCell>
              <TableCell align="right">Kolicina</TableCell>
              <TableCell align="right">Cena</TableCell>
              <TableCell align="right">Ukupno</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{item.productSku}</TableCell>
                <TableCell>{item.productName}</TableCell>
                <TableCell align="right">
                  <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>
                    {formatQuantity(item.quantity)}
                  </Box>
                  <Box component="span" sx={{ ml: 0.5, color: "text.secondary", fontSize: "0.78rem" }}>
                    {unitLabels[item.unitOfMeasure]}
                  </Box>
                </TableCell>
                <TableCell align="right" sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>
                  {formatMoney(item.unitPrice)}
                </TableCell>
                <TableCell align="right" sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>
                  {formatMoney(item.lineTotal)}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>

        <Typography variant="h3" sx={{ mb: 1 }}>
          Istorija statusa
        </Typography>

        <Stack spacing={1.5}>
          {history.map((entry, index) => (
            <Stack key={index} direction="row" spacing={1.5} alignItems="flex-start">
              <Box
                sx={{
                  width: 8,
                  height: 8,
                  borderRadius: "50%",
                  bgcolor: statusColors[entry.toStatus],
                  mt: 0.75,
                  flexShrink: 0,
                }}
              />
              <Box>
                <Typography variant="body2">
                  {entry.fromStatus
                    ? `${orderStatusLabels[entry.fromStatus]} → ${orderStatusLabels[entry.toStatus]}`
                    : orderStatusLabels[entry.toStatus]}
                </Typography>
                <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
                  {entry.changedByName} · {formatDateTime(entry.changedAt)}
                </Typography>
                {entry.note && (
                  <Typography variant="body2" sx={{ fontSize: "0.8rem", mt: 0.25 }}>
                    {entry.note}
                  </Typography>
                )}
              </Box>
            </Stack>
          ))}
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2, justifyContent: "space-between" }}>
        <Box>
          {order.status === 1 && onEdit && (
            <Button onClick={() => onEdit(detail)}>Izmeni nalog</Button>
          )}
        </Box>

        <Stack direction="row" spacing={1}>
          <Button onClick={onClose}>Zatvori</Button>
          {allowedNextStatuses
            .filter((status) => canMoveTo(status))
            .map((status) => (
              <Button
                key={status}
                variant={status === 6 ? "outlined" : "contained"}
                color={status === 6 ? "error" : "primary"}
                onClick={() => onChangeStatus(order, status)}
              >
                {orderStatusLabels[status]}
              </Button>
            ))}
        </Stack>
      </DialogActions>
    </Dialog>
  );
}
