import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import ArrowUpwardIcon from "@mui/icons-material/ArrowUpward";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import { Box, Paper, Stack, Typography } from "@mui/material";
import { Link } from "react-router-dom";
import { monoFont } from "../../theme";

export const percentChange = (current, previous) =>
  previous > 0 ? Math.round(((current - previous) / previous) * 100) : null;

function TrendPill({ value }) {
  const up = value >= 0;

  return (
    <Stack
      direction="row"
      sx={{
        alignItems: "center",
        gap: 0.25,
        px: 0.75,
        py: 0.25,
        borderRadius: 10,
        bgcolor: up ? "tint.success" : "tint.danger",
        color: up ? "success.main" : "error.main",
        fontSize: "0.74rem",
        fontWeight: 600,
        lineHeight: 1.4,
        flexShrink: 0,
      }}
    >
      {up ? <ArrowUpwardIcon sx={{ fontSize: 13 }} /> : <ArrowDownwardIcon sx={{ fontSize: 13 }} />}
      {Math.abs(value)}%
    </Stack>
  );
}

export default function StatCard({ icon: Icon, label, value, hint, trend, tone = "primary.main", to }) {
  return (
    <Paper
      variant="outlined"
      component={to ? Link : "div"}
      to={to}
      sx={{
        p: 2.5,
        display: "flex",
        flexDirection: "column",
        gap: 1.25,
        textDecoration: "none",
        color: "inherit",
        transition: "border-color 120ms, background-color 120ms",
        "&:hover": to ? { borderColor: "primary.main", bgcolor: "surface.muted" } : undefined,
        "&:hover .stat-arrow": { opacity: 1, transform: "translateX(0)" },
      }}
    >
      <Stack direction="row" sx={{ alignItems: "center", gap: 1 }}>
        {Icon && <Icon sx={{ fontSize: 18, color: tone }} />}
        <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.8rem", flexGrow: 1 }}>
          {label}
        </Typography>
        {to && (
          <ChevronRightIcon
            className="stat-arrow"
            sx={{
              fontSize: 18,
              color: "text.secondary",
              opacity: 0,
              transform: "translateX(-4px)",
              transition: "opacity 120ms, transform 120ms",
            }}
          />
        )}
      </Stack>

      <Typography sx={{ fontFamily: monoFont, fontSize: "1.55rem", fontWeight: 600, lineHeight: 1.15 }}>
        {value}
      </Typography>

      <Stack direction="row" sx={{ alignItems: "center", gap: 1, minHeight: 22 }}>
        {trend != null && <TrendPill value={trend} />}
        {hint && (
          <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.76rem" }}>
            {hint}
          </Typography>
        )}
      </Stack>
    </Paper>
  );
}

export function StatGrid({ children, columns = 4 }) {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr", lg: `repeat(${columns}, 1fr)` },
        gap: 2,
        mb: 3,
      }}
    >
      {children}
    </Box>
  );
}
