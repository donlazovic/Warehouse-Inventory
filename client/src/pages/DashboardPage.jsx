import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import PendingActionsOutlinedIcon from "@mui/icons-material/PendingActionsOutlined";
import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Box, Paper, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ordersApi, stockApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import PageHeader from "../components/common/PageHeader";
import { monoFont } from "../theme";
import { orderStatusLabels } from "../utils/format";

function StatCard({ icon: Icon, label, value, hint, tone = "primary.main", to }) {
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
        <Icon sx={{ fontSize: 18, color: tone }} />
        <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.8rem" }}>
          {label}
        </Typography>
      </Stack>
      <Typography sx={{ fontFamily: monoFont, fontSize: "1.65rem", fontWeight: 600, lineHeight: 1.1 }}>
        {value}
      </Typography>
      {hint && (
        <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.76rem", mt: 0.5 }}>
          {hint}
        </Typography>
      )}
    </Paper>
  );
}

export default function DashboardPage() {
  const { user } = useAuth();
  const [columns, setColumns] = useState([]);
  const [lowStock, setLowStock] = useState(0);
  const [stockLines, setStockLines] = useState(0);

  useEffect(() => {
    ordersApi.kanban({}).then(setColumns).catch(() => {});
    stockApi
      .list({ pageSize: 1, onlyBelowMinimum: true })
      .then((result) => setLowStock(result.totalCount))
      .catch(() => {});
    stockApi
      .list({ pageSize: 1 })
      .then((result) => setStockLines(result.totalCount))
      .catch(() => {});
  }, []);

  const countFor = (status) => columns.find((column) => column.status === status)?.totalCount ?? 0;
  const openOrders = countFor(1) + countFor(2) + countFor(3) + countFor(4);

  return (
    <Box>
      <PageHeader
        title={`Dobrodosli, ${user?.firstName ?? ""}`}
        description="Pregled kljucnih informacija o stanju u vasem lancu."
      />

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr", lg: "repeat(4, 1fr)" },
          gap: 2,
          mb: 3,
        }}
      >
        <StatCard
          icon={AssignmentOutlinedIcon}
          label="Nalozi u toku"
          value={openOrders}
          hint="Nisu jos realizovani ni stornirani"
          to="/nalozi"
        />
        <StatCard
          icon={PendingActionsOutlinedIcon}
          label="Cekaju odobrenje"
          value={countFor(2)}
          hint="Potreban pregled menadzera"
          tone="warning.main"
          to="/nalozi"
        />
        <StatCard
          icon={WarningAmberOutlinedIcon}
          label="Ispod minimuma"
          value={lowStock}
          hint="Artikala na lokacijama"
          tone="error.main"
          to="/zalihe"
        />
        <StatCard
          icon={Inventory2OutlinedIcon}
          label="Stavki na zalihama"
          value={stockLines}
          hint="Kombinacija artikla i lokacije"
          to="/zalihe"
        />
      </Box>

      <Paper variant="outlined" sx={{ p: 2.5 }}>
        <Typography variant="h3" sx={{ mb: 2 }}>
          Nalozi po statusima
        </Typography>

        <Stack spacing={1.25}>
          {columns.map((column) => (
            <Stack key={column.status} direction="row" alignItems="center" spacing={2}>
              <Typography variant="body2" sx={{ width: 150, flexShrink: 0 }}>
                {orderStatusLabels[column.status]}
              </Typography>
              <Box sx={{ flexGrow: 1, height: 6, bgcolor: "#EEF1F2", borderRadius: 3, overflow: "hidden" }}>
                <Box
                  sx={{
                    height: "100%",
                    width: `${Math.min(100, column.totalCount * 12)}%`,
                    bgcolor: "primary.main",
                  }}
                />
              </Box>
              <Typography sx={{ fontFamily: monoFont, fontSize: "0.85rem", width: 32, textAlign: "right" }}>
                {column.totalCount}
              </Typography>
            </Stack>
          ))}
        </Stack>

        <Typography variant="body2" color="text.secondary" sx={{ mt: 2.5 }}>
          Detaljni izvestaji sa grafikonima i izvozom bice dostupni na stranici Izvestaji.
        </Typography>
      </Paper>
    </Box>
  );
}
