import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import ArrowUpwardIcon from "@mui/icons-material/ArrowUpward";
import { Box, Paper, Stack, Typography } from "@mui/material";
import { Link } from "react-router-dom";
import { monoFont } from "../../theme";

export const percentChange = (current, previous) =>
  previous > 0 ? Math.round(((current - previous) / previous) * 100) : null;

export default function StatCard({ icon: Icon, label, value, hint, trend, tone = "primary.main", to }) {
  return (
    <Paper
      variant="outlined"
      component={to ? Link : "div"}
      to={to}
      sx={{
        p: 2.5,
        display: "block",
        textDecoration: "none",
        color: "inherit",
        "&:hover": to ? { borderColor: "text.secondary" } : undefined,
      }}
    >
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1.5 }}>
        {Icon && <Icon sx={{ fontSize: 18, color: tone }} />}
        <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.8rem" }}>
          {label}
        </Typography>
      </Stack>

      <Typography sx={{ fontFamily: monoFont, fontSize: "1.55rem", fontWeight: 600, lineHeight: 1.15 }}>
        {value}
      </Typography>

      <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 0.75, minHeight: 20 }}>
        {trend != null && (
          <Stack
            direction="row"
            alignItems="center"
            sx={{ color: trend >= 0 ? "success.main" : "error.main", fontSize: "0.76rem", fontWeight: 600 }}
          >
            {trend >= 0 ? <ArrowUpwardIcon sx={{ fontSize: 14 }} /> : <ArrowDownwardIcon sx={{ fontSize: 14 }} />}
            {Math.abs(trend)}%
          </Stack>
        )}
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
