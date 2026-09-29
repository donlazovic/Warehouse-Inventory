import ArrowRightAltIcon from "@mui/icons-material/ArrowRightAlt";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import { Box, Paper, Stack, Typography } from "@mui/material";
import { monoFont, statusColors } from "../../theme";
import { formatDateTime, formatMoney } from "../../utils/format";

const routeOf = (order) =>
  order.orderType === 1
    ? [order.supplierName ?? "Dobavljac", order.destinationLocationName ?? "Magacin"]
    : [order.sourceLocationName ?? "Magacin", order.storeName ?? "Objekat"];

export default function OrderCard({ order, draggable, highlighted = false, onDragStart, onDragEnd, onClick }) {
  const [from, to] = routeOf(order);

  return (
    <Paper
      variant="outlined"
      draggable={draggable}
      onDragStart={draggable ? (event) => onDragStart(event, order) : undefined}
      onDragEnd={draggable ? onDragEnd : undefined}
      onClick={() => onClick?.(order)}
      sx={{
        p: 1.5,
        mb: 1,
        cursor: draggable ? "grab" : "pointer",
        borderLeft: "3px solid",
        borderLeftColor: statusColors[order.status],
        "@keyframes cardPulse": {
          "0%": { boxShadow: "0 0 0 0 rgba(31, 95, 75, 0.45)" },
          "100%": { boxShadow: "0 0 0 10px rgba(31, 95, 75, 0)" },
        },
        animation: highlighted ? "cardPulse 1.1s ease-out 2" : "none",
        bgcolor: highlighted ? "#F3F8F6" : "background.paper",
        transition: "background-color 600ms",
        "&:active": { cursor: draggable ? "grabbing" : "pointer" },
        "&:hover": { borderColor: "text.secondary", borderLeftColor: statusColors[order.status] },
      }}
    >
      <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem", fontWeight: 600 }}>
        {order.orderNumber}
      </Typography>

      <Stack direction="row" alignItems="center" spacing={0.5} sx={{ mt: 0.75, minWidth: 0 }}>
        <Typography variant="body2" sx={{ fontSize: "0.78rem" }} noWrap>
          {from}
        </Typography>
        <ArrowRightAltIcon sx={{ fontSize: 16, color: "text.disabled", flexShrink: 0 }} />
        <Typography variant="body2" sx={{ fontSize: "0.78rem", fontWeight: 500 }} noWrap>
          {to}
        </Typography>
      </Stack>

      <Typography variant="body2" sx={{ mt: 0.75, fontSize: "0.72rem", color: "text.secondary" }}>
        {formatDateTime(order.createdAt)}
      </Typography>

      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mt: 1.25 }}>
        <Stack direction="row" alignItems="center" spacing={0.5}>
          <Inventory2OutlinedIcon sx={{ fontSize: 14, color: "text.disabled" }} />
          <Typography variant="body2" sx={{ fontSize: "0.72rem", color: "text.secondary" }}>
            {order.itemCount} {order.itemCount === 1 ? "stavka" : "stavki"}
          </Typography>
        </Stack>
        <Box sx={{ fontFamily: monoFont, fontSize: "0.76rem", fontWeight: 500 }}>
          {formatMoney(order.totalValue)}
        </Box>
      </Stack>
    </Paper>
  );
}
