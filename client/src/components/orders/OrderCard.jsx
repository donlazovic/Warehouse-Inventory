import CallMadeIcon from "@mui/icons-material/CallMade";
import CallReceivedIcon from "@mui/icons-material/CallReceived";
import { Box, Stack, Typography } from "@mui/material";
import { alpha, useTheme } from "@mui/material/styles";
import { monoFont } from "../../theme";
import { formatMoney } from "../../utils/format";

const shortCode = (code) => code?.replace(/-MAIN$/, "") ?? null;

const sameDay = (a, b) => a.toDateString() === b.toDateString();

const shortTime = (value) => {
  const date = new Date(value);
  const today = new Date();
  const yesterday = new Date();
  yesterday.setDate(today.getDate() - 1);

  const time = date.toLocaleTimeString("sr-Latn-RS", { hour: "2-digit", minute: "2-digit" });
  if (sameDay(date, today)) return `danas ${time}`;
  if (sameDay(date, yesterday)) return `juce ${time}`;
  return `${date.toLocaleDateString("sr-Latn-RS", { day: "2-digit", month: "2-digit" })} ${time}`;
};

export default function OrderCard({
  order,
  draggable = false,
  dragging = false,
  highlighted = false,
  onDragStart,
  onDragEnd,
  onClick,
  sx,
}) {
  const theme = useTheme();
  const inbound = order.orderType === 1;
  const counterparty = inbound ? order.supplierName : order.storeName;
  const route = inbound
    ? `Dobavljac → ${shortCode(order.destinationLocationCode) ?? "magacin"}`
    : `${shortCode(order.sourceLocationCode) ?? "magacin"} → ${shortCode(order.destinationLocationCode) ?? "objekat"}`;
  const pulse = theme.palette.primary.main;

  return (
    <Box
      draggable={draggable}
      onDragStart={draggable ? (event) => onDragStart(event, order) : undefined}
      onDragEnd={draggable ? onDragEnd : undefined}
      onClick={() => onClick?.(order)}
      sx={{
        px: 1.25,
        py: 1,
        border: "1px solid",
        borderColor: "divider",
        borderStyle: dragging ? "dashed" : "solid",
        borderRadius: 1.5,
        bgcolor: highlighted ? "surface.highlight" : "background.paper",
        opacity: dragging ? 0.35 : 1,
        cursor: draggable ? "grab" : "pointer",
        userSelect: "none",
        transition: "border-color 120ms, box-shadow 120ms, background-color 600ms, opacity 120ms",
        "@keyframes cardPulse": {
          "0%": { boxShadow: `0 0 0 0 ${alpha(pulse, 0.45)}` },
          "100%": { boxShadow: `0 0 0 8px ${alpha(pulse, 0)}` },
        },
        animation: highlighted ? "cardPulse 1.1s ease-out 2" : "none",
        "&:hover": {
          borderColor: "primary.main",
          boxShadow: `0 2px 8px ${alpha(theme.palette.common.black, theme.palette.mode === "dark" ? 0.35 : 0.08)}`,
        },
        "&:active": { cursor: draggable ? "grabbing" : "pointer" },
        ...sx,
      }}
    >
      <Stack direction="row" sx={{ alignItems: "center", gap: 0.75 }}>
        {inbound ? (
          <CallReceivedIcon sx={{ fontSize: 14, color: "success.main" }} titleAccess="Ulazni nalog" />
        ) : (
          <CallMadeIcon sx={{ fontSize: 14, color: "secondary.main" }} titleAccess="Izlazni nalog" />
        )}
        <Typography sx={{ fontFamily: monoFont, fontSize: "0.74rem", fontWeight: 600, flexGrow: 1 }}>
          {order.orderNumber}
        </Typography>
        <Typography sx={{ fontSize: "0.7rem", color: "text.secondary", whiteSpace: "nowrap" }}>
          {shortTime(order.createdAt)}
        </Typography>
      </Stack>

      <Typography noWrap title={counterparty ?? ""} sx={{ fontSize: "0.84rem", fontWeight: 500, mt: 0.5 }}>
        {counterparty ?? "—"}
      </Typography>

      <Stack direction="row" sx={{ alignItems: "baseline", gap: 1, mt: 0.25 }}>
        <Typography noWrap sx={{ fontFamily: monoFont, fontSize: "0.7rem", color: "text.secondary", flexGrow: 1, minWidth: 0 }}>
          {route} · {order.itemCount} {order.itemCount === 1 ? "stavka" : "stavki"}
        </Typography>
        <Typography sx={{ fontFamily: monoFont, fontSize: "0.74rem", fontWeight: 600, whiteSpace: "nowrap" }}>
          {formatMoney(order.totalValue)}
        </Typography>
      </Stack>
    </Box>
  );
}
