import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import MoveDownIcon from "@mui/icons-material/MoveDown";
import { Box, Button, Collapse, Paper, Stack, Typography } from "@mui/material";
import { alpha } from "@mui/material/styles";
import { useState } from "react";
import useFitHeight from "../../hooks/useFitHeight";
import { monoFont, statusColors } from "../../theme";
import { formatMoney } from "../../utils/format";
import OrderCard from "./OrderCard";

const FLOW_STATUSES = [1, 2, 3, 4, 5];
const CANCELLED = 6;

const allowedTransitions = {
  1: [2, 6],
  2: [3, 1, 6],
  3: [4, 6],
  4: [5, 6],
  5: [],
  6: [],
};

function DropHint({ color, active, label }) {
  return (
    <Stack
      direction="row"
      sx={{
        alignItems: "center",
        justifyContent: "center",
        gap: 0.75,
        mx: 1.25,
        mt: 1.25,
        py: 1.25,
        borderRadius: 1.5,
        border: "2px dashed",
        borderColor: color,
        bgcolor: active ? alpha(color, 0.18) : "transparent",
        color: active ? color : "text.secondary",
        fontSize: "0.8rem",
        fontWeight: 600,
        transition: "background-color 120ms, color 120ms",
        flexShrink: 0,
      }}
    >
      <MoveDownIcon sx={{ fontSize: 16 }} />
      {label}
    </Stack>
  );
}

export default function KanbanBoard({
  columns,
  canMove,
  canMoveTo = () => true,
  highlighted = new Set(),
  onMove,
  onOpen,
  onShowAll,
}) {
  const [fitRef, fitHeight] = useFitHeight({ reserve: 24, min: 460 });
  const [dragged, setDragged] = useState(null);
  const [hoveredStatus, setHoveredStatus] = useState(null);
  const [cancelledOpen, setCancelledOpen] = useState(false);

  const flowColumns = columns.filter((column) => FLOW_STATUSES.includes(column.status));
  const cancelledColumn = columns.find((column) => column.status === CANCELLED);

  const handleDragStart = (event, order) => {
    event.dataTransfer.effectAllowed = "move";
    event.dataTransfer.setData("text/plain", String(order.id));
    setDragged(order);
  };

  const handleDragEnd = () => {
    setDragged(null);
    setHoveredStatus(null);
  };

  const canDropOn = (status) =>
    dragged ? Boolean(allowedTransitions[dragged.status]?.includes(status) && canMoveTo(status)) : false;

  const dropProps = (status) => ({
    onDragOver: (event) => {
      if (!canDropOn(status)) return;
      event.preventDefault();
      event.dataTransfer.dropEffect = "move";
      if (hoveredStatus !== status) setHoveredStatus(status);
    },
    onDragLeave: (event) => {
      if (event.currentTarget.contains(event.relatedTarget)) return;
      setHoveredStatus((current) => (current === status ? null : current));
    },
    onDrop: (event) => {
      event.preventDefault();
      if (dragged && canDropOn(status)) onMove(dragged, status);
      handleDragEnd();
    },
  });

  const columnState = (status) => {
    const target = canDropOn(status);
    return {
      target,
      hovered: hoveredStatus === status,
      dimmed: Boolean(dragged) && !target && dragged.status !== status,
    };
  };

  const renderCard = (order, extraSx) => (
    <OrderCard
      key={order.id}
      order={order}
      draggable={canMove && (allowedTransitions[order.status]?.length ?? 0) > 0}
      dragging={dragged?.id === order.id}
      highlighted={highlighted.has(order.id)}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onClick={onOpen}
      sx={extraSx}
    />
  );

  return (
    <Box
      ref={fitRef}
      sx={{
        display: "flex",
        flexDirection: "column",
        gap: 1.5,
        height: { xs: "auto", lg: fitHeight ?? "auto" },
      }}
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))", lg: "repeat(5, minmax(0, 1fr))" },
          gap: 1.5,
          flex: 1,
          minHeight: 0,
        }}
      >
        {flowColumns.map((column) => {
          const color = statusColors[column.status];
          const { target, hovered, dimmed } = columnState(column.status);
          const hidden = column.totalCount - column.orders.length;

          return (
            <Paper
              key={column.status}
              variant="outlined"
              {...dropProps(column.status)}
              sx={{
                display: "flex",
                flexDirection: "column",
                minHeight: { xs: 220, lg: 0 },
                maxHeight: { xs: 520, lg: "none" },
                overflow: "hidden",
                opacity: dimmed ? 0.4 : 1,
                outline: target ? `2px solid ${color}` : "none",
                outlineOffset: -1,
                bgcolor: hovered ? `statusTint.${column.status}` : "background.paper",
                transition: "opacity 150ms, background-color 120ms",
              }}
            >
              <Stack
                direction="row"
                sx={{ alignItems: "center", gap: 1, bgcolor: color, color: "#FFFFFF", px: 1.5, py: 0.875, flexShrink: 0 }}
              >
                <Typography variant="body2" sx={{ fontWeight: 600, flexGrow: 1 }} noWrap>
                  {column.title}
                </Typography>
                <Box
                  sx={{
                    minWidth: 22,
                    px: 0.75,
                    borderRadius: 10,
                    bgcolor: "rgba(255,255,255,0.22)",
                    fontSize: "0.74rem",
                    fontWeight: 600,
                    textAlign: "center",
                    lineHeight: "20px",
                  }}
                >
                  {column.totalCount}
                </Box>
              </Stack>

              <Box
                sx={{
                  px: 1.5,
                  py: 0.625,
                  borderBottom: "1px solid",
                  borderColor: "divider",
                  bgcolor: `statusTint.${column.status}`,
                  fontFamily: monoFont,
                  fontSize: "0.72rem",
                  color: "text.secondary",
                  flexShrink: 0,
                }}
              >
                {formatMoney(column.totalValue)}
              </Box>

              {target && <DropHint color={color} active={hovered} label={`Pusti u „${column.title}"`} />}

              <Stack sx={{ flex: 1, minHeight: 0, overflowY: "auto", p: 1.25, gap: 1 }}>
                {column.orders.map((order) => renderCard(order))}

                {column.orders.length === 0 && !target && (
                  <Typography variant="body2" sx={{ color: "text.disabled", fontSize: "0.78rem", py: 3, textAlign: "center" }}>
                    Nema naloga
                  </Typography>
                )}

                {hidden > 0 && (
                  <Button size="small" onClick={() => onShowAll?.(column.status)} sx={{ flexShrink: 0, mt: 0.5 }}>
                    Jos {hidden.toLocaleString("sr-Latn-RS")} — prikazi u listi
                  </Button>
                )}
              </Stack>
            </Paper>
          );
        })}
      </Box>

      {cancelledColumn && (() => {
        const color = statusColors[CANCELLED];
        const { target, hovered, dimmed } = columnState(CANCELLED);
        const hidden = cancelledColumn.totalCount - cancelledColumn.orders.length;

        return (
          <Paper
            variant="outlined"
            {...dropProps(CANCELLED)}
            sx={{
              flexShrink: 0,
              overflow: "hidden",
              opacity: dimmed ? 0.4 : 1,
              outline: target ? `2px solid ${color}` : "none",
              outlineOffset: -1,
              bgcolor: hovered ? `statusTint.${CANCELLED}` : "background.paper",
              transition: "opacity 150ms, background-color 120ms",
            }}
          >
            <Stack
              direction="row"
              onClick={() => setCancelledOpen((current) => !current)}
              sx={{ alignItems: "center", gap: 1, px: 1.5, py: 1, cursor: "pointer" }}
            >
              <Box sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: color }} />
              <Typography variant="body2" sx={{ fontWeight: 600 }}>
                {cancelledColumn.title}
              </Typography>
              <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.78rem" }}>
                {cancelledColumn.totalCount}
              </Typography>
              <Box sx={{ flexGrow: 1 }} />
              {target ? (
                <Typography variant="body2" sx={{ color, fontSize: "0.78rem", fontWeight: 600 }}>
                  Pusti ovde da otkazes nalog
                </Typography>
              ) : (
                <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
                  {cancelledOpen ? "Sakrij" : "Prikazi"}
                </Typography>
              )}
              {cancelledOpen ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
            </Stack>

            <Collapse in={cancelledOpen}>
              <Stack direction="row" sx={{ gap: 1, overflowX: "auto", px: 1.5, pb: 1.5, alignItems: "stretch" }}>
                {cancelledColumn.orders.length === 0 ? (
                  <Typography variant="body2" sx={{ color: "text.disabled", fontSize: "0.78rem" }}>
                    Nema otkazanih naloga.
                  </Typography>
                ) : (
                  cancelledColumn.orders.map((order) => renderCard(order, { width: 260, flexShrink: 0 }))
                )}
                {hidden > 0 && (
                  <Button size="small" onClick={() => onShowAll?.(CANCELLED)} sx={{ flexShrink: 0, whiteSpace: "nowrap" }}>
                    Jos {hidden} u listi
                  </Button>
                )}
              </Stack>
            </Collapse>
          </Paper>
        );
      })()}
    </Box>
  );
}
