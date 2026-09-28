import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { Box, Collapse, IconButton, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import { monoFont, statusColors, statusTints } from "../../theme";
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

export default function KanbanBoard({ columns, canMove, canMoveTo = () => true, onMove, onOpen }) {
  const [dragged, setDragged] = useState(null);
  const [hoveredStatus, setHoveredStatus] = useState(null);
  const [cancelledOpen, setCancelledOpen] = useState(false);

  const flowColumns = columns.filter((column) => FLOW_STATUSES.includes(column.status));
  const cancelledColumn = columns.find((column) => column.status === CANCELLED);

  const handleDragStart = (event, order) => {
    setDragged(order);
    event.dataTransfer.effectAllowed = "move";
  };

  const handleDragEnd = () => {
    setDragged(null);
    setHoveredStatus(null);
  };

  const canDropOn = (status) =>
    dragged
      ? Boolean(allowedTransitions[dragged.status]?.includes(status) && canMoveTo(status))
      : false;

  const dropProps = (status) => ({
    onDragOver: (event) => {
      if (!canDropOn(status)) return;
      event.preventDefault();
      setHoveredStatus(status);
    },
    onDragLeave: () => setHoveredStatus((current) => (current === status ? null : current)),
    onDrop: (event) => {
      event.preventDefault();
      if (dragged && canDropOn(status)) onMove(dragged, status);
      handleDragEnd();
    },
  });

  return (
    <Box>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", lg: "repeat(5, minmax(230px, 1fr))" },
          gap: 1.5,
          alignItems: "start",
        }}
      >
        {flowColumns.map((column) => {
          const isTarget = dragged && canDropOn(column.status);
          const isHovered = hoveredStatus === column.status;

          return (
            <Paper
              key={column.status}
              variant="outlined"
              sx={{
                overflow: "hidden",
                borderColor: isTarget ? statusColors[column.status] : "divider",
                borderWidth: isTarget ? 2 : 1,
                transition: "border-color 120ms",
              }}
              {...dropProps(column.status)}
            >
              <Stack
                direction="row"
                alignItems="center"
                spacing={1}
                sx={{ bgcolor: statusColors[column.status], color: "#FFFFFF", px: 1.5, py: 1 }}
              >
                <Typography variant="body2" sx={{ fontWeight: 600, flexGrow: 1 }}>
                  {column.title}
                </Typography>
                <Box
                  sx={{
                    minWidth: 22,
                    px: 0.75,
                    py: 0.125,
                    borderRadius: 10,
                    bgcolor: "rgba(255,255,255,0.22)",
                    fontSize: "0.74rem",
                    fontWeight: 600,
                    textAlign: "center",
                  }}
                >
                  {column.totalCount}
                </Box>
              </Stack>

              <Box
                sx={{
                  px: 1.5,
                  py: 0.75,
                  bgcolor: statusTints[column.status],
                  borderBottom: "1px solid",
                  borderColor: "divider",
                  fontFamily: monoFont,
                  fontSize: "0.72rem",
                  color: "text.secondary",
                }}
              >
                {formatMoney(column.totalValue)}
              </Box>

              <Box
                sx={{
                  p: 1.25,
                  minHeight: 180,
                  bgcolor: isHovered ? statusTints[column.status] : "background.paper",
                  transition: "background-color 120ms",
                }}
              >
                {column.orders.map((order) => (
                  <OrderCard
                    key={order.id}
                    order={order}
                    draggable={canMove}
                    onDragStart={handleDragStart}
                    onDragEnd={handleDragEnd}
                    onClick={onOpen}
                  />
                ))}

                {column.orders.length === 0 && (
                  <Typography
                    variant="body2"
                    sx={{ color: "text.disabled", fontSize: "0.78rem", py: 3, textAlign: "center" }}
                  >
                    {isTarget ? "Pustite ovde" : "Prazno"}
                  </Typography>
                )}
              </Box>
            </Paper>
          );
        })}
      </Box>

      {cancelledColumn && (
        <Paper
          variant="outlined"
          sx={{
            mt: 1.5,
            overflow: "hidden",
            borderColor: dragged && canDropOn(CANCELLED) ? statusColors[CANCELLED] : "divider",
            borderWidth: dragged && canDropOn(CANCELLED) ? 2 : 1,
            bgcolor: hoveredStatus === CANCELLED ? statusTints[CANCELLED] : "background.paper",
            transition: "background-color 120ms, border-color 120ms",
          }}
          {...dropProps(CANCELLED)}
        >
          <Stack
            direction="row"
            alignItems="center"
            spacing={1}
            sx={{ p: 1.5, cursor: "pointer" }}
            onClick={() => setCancelledOpen((current) => !current)}
          >
            <Box sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: statusColors[CANCELLED] }} />
            <Typography variant="body2" sx={{ fontWeight: 600 }}>
              {cancelledColumn.title}
            </Typography>
            <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.78rem", flexGrow: 1 }}>
              {cancelledColumn.totalCount}
            </Typography>
            {dragged && canDropOn(CANCELLED) && (
              <Typography variant="body2" sx={{ color: "error.main", fontSize: "0.75rem" }}>
                Pustite ovde da biste otkazali
              </Typography>
            )}
            <IconButton size="small">
              {cancelledOpen ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
            </IconButton>
          </Stack>

          <Collapse in={cancelledOpen}>
            <Box
              sx={{
                px: 1.5,
                pb: 1.5,
                display: "grid",
                gridTemplateColumns: { xs: "1fr", md: "repeat(auto-fill, minmax(240px, 1fr))" },
                gap: 1,
              }}
            >
              {cancelledColumn.orders.length === 0 ? (
                <Typography variant="body2" sx={{ color: "text.disabled", fontSize: "0.78rem" }}>
                  Nema otkazanih naloga.
                </Typography>
              ) : (
                cancelledColumn.orders.map((order) => (
                  <OrderCard key={order.id} order={order} onClick={onOpen} />
                ))
              )}
            </Box>
          </Collapse>
        </Paper>
      )}
    </Box>
  );
}
