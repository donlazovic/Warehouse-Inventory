import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import OutputIcon from "@mui/icons-material/Output";
import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Alert, Box, Link as MuiLink, Paper, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Area,
  AreaChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip as ChartTooltip,
  XAxis,
  YAxis,
} from "recharts";
import { reportsApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import PageHeader from "../components/common/PageHeader";
import StatCard, { percentChange, StatGrid } from "../components/common/StatCard";
import { monoFont, statusColors } from "../theme";
import {
  chartColors,
  formatCompact,
  formatMoney,
  formatQuantity,
  orderStatusLabels,
  unitLabels,
} from "../utils/format";

const flowSeries = [
  { key: "receivedValue", label: "Ulaz", color: chartColors.received },
  { key: "transferredValue", label: "Prenos", color: chartColors.transferred },
  { key: "issuedValue", label: "Izlaz", color: chartColors.issued },
];

function Panel({ title, action, children, sx }) {
  return (
    <Paper variant="outlined" sx={{ p: 2.5, display: "flex", flexDirection: "column", ...sx }}>
      <Stack direction="row" justifyContent="space-between" alignItems="baseline" sx={{ mb: 2 }}>
        <Typography variant="h3">{title}</Typography>
        {action}
      </Stack>
      {children}
    </Paper>
  );
}

function FlowTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null;

  return (
    <Paper variant="outlined" sx={{ p: 1.25 }}>
      <Typography variant="body2" sx={{ fontWeight: 600, mb: 0.5 }}>
        {new Date(label).toLocaleDateString("sr-RS", { day: "2-digit", month: "2-digit" })}
      </Typography>
      {payload.map((entry) => (
        <Stack key={entry.dataKey} direction="row" spacing={1} alignItems="center">
          <Box sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: entry.color }} />
          <Typography variant="body2" sx={{ fontSize: "0.78rem", flexGrow: 1 }}>
            {flowSeries.find((s) => s.key === entry.dataKey)?.label}
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{formatMoney(entry.value)}</Typography>
        </Stack>
      ))}
    </Paper>
  );
}

export default function DashboardPage() {
  const { user, can } = useAuth();
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!can("reports.view")) return;
    reportsApi.dashboard().then(setData).catch((err) => setError(err.message));
  }, [can]);

  const header = (
    <PageHeader
      title={`Dobrodosli, ${user?.firstName ?? ""}`}
      description={new Date().toLocaleDateString("sr-RS", {
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric",
      })}
    />
  );

  if (!can("reports.view")) {
    return (
      <Box>
        {header}
        <Typography color="text.secondary">Izaberite stavku iz menija da biste nastavili.</Typography>
      </Box>
    );
  }

  if (error) return <Box>{header}<Alert severity="error">{error}</Alert></Box>;
  if (!data) return <Box>{header}<Typography color="text.secondary">Ucitavanje...</Typography></Box>;

  const { kpi } = data;
  const categoryTotal = data.stockByCategory.reduce((sum, item) => sum + item.value, 0);
  const maxStatus = Math.max(1, ...data.ordersByStatus.map((item) => item.count));

  return (
    <Box>
      {header}

      <StatGrid>
        <StatCard
          icon={Inventory2OutlinedIcon}
          label="Vrednost zaliha"
          value={formatMoney(kpi.stockValue)}
          hint="Po trenutnim cenama"
          to="/zalihe"
        />
        <StatCard
          icon={AssignmentOutlinedIcon}
          label="Nalozi u toku"
          value={kpi.openOrders}
          hint={`${kpi.pendingApproval} ceka odobrenje`}
          to="/nalozi"
        />
        <StatCard
          icon={WarningAmberOutlinedIcon}
          label="Ispod minimuma"
          value={kpi.belowMinimum}
          hint="Artikala po lokacijama"
          tone="warning.main"
          to="/zalihe"
        />
        <StatCard
          icon={OutputIcon}
          label="Izlaz ovog meseca"
          value={formatMoney(kpi.issuedValueThisMonth)}
          trend={percentChange(kpi.issuedValueThisMonth, kpi.issuedValueLastMonth)}
          hint="u odnosu na prosli mesec"
          tone="warning.main"
        />
      </StatGrid>

      <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", lg: "2fr 1fr" }, gap: 2, mb: 2 }}>
        <Panel
          title="Tok robe — poslednjih 14 dana"
          action={
            <Stack direction="row" spacing={2}>
              {flowSeries.map((series) => (
                <Stack key={series.key} direction="row" spacing={0.75} alignItems="center">
                  <Box sx={{ width: 10, height: 3, bgcolor: series.color, borderRadius: 1 }} />
                  <Typography variant="body2" sx={{ fontSize: "0.78rem" }}>
                    {series.label}
                  </Typography>
                </Stack>
              ))}
            </Stack>
          }
        >
          <Box sx={{ height: 260 }}>
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={data.dailyFlow} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
                <CartesianGrid stroke="#EEF1F2" vertical={false} />
                <XAxis
                  dataKey="date"
                  tickFormatter={(value) =>
                    new Date(value).toLocaleDateString("sr-RS", { day: "2-digit", month: "2-digit" })
                  }
                  tick={{ fontSize: 11, fill: "#5A6169" }}
                  axisLine={false}
                  tickLine={false}
                />
                <YAxis
                  tickFormatter={formatCompact}
                  tick={{ fontSize: 11, fill: "#5A6169" }}
                  axisLine={false}
                  tickLine={false}
                  width={52}
                />
                <ChartTooltip content={<FlowTooltip />} />
                {flowSeries.map((series) => (
                  <Area
                    key={series.key}
                    type="monotone"
                    dataKey={series.key}
                    stroke={series.color}
                    fill={series.color}
                    fillOpacity={0.08}
                    strokeWidth={2}
                  />
                ))}
              </AreaChart>
            </ResponsiveContainer>
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.75rem", mt: 1 }}>
            Vrednost u dinarima po trenutnim cenama — kolicine razlicitih artikala se ne mogu sabirati.
          </Typography>
        </Panel>

        <Panel title="Zalihe po kategorijama">
          {data.stockByCategory.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              Nema robe na stanju.
            </Typography>
          ) : (
            <>
              <Box sx={{ height: 180, position: "relative" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={data.stockByCategory}
                      dataKey="value"
                      nameKey="category"
                      innerRadius={55}
                      outerRadius={80}
                      paddingAngle={2}
                      stroke="none"
                    >
                      {data.stockByCategory.map((item, index) => (
                        <Cell key={item.category} fill={chartColors.palette[index % chartColors.palette.length]} />
                      ))}
                    </Pie>
                  </PieChart>
                </ResponsiveContainer>
                <Box sx={{ position: "absolute", inset: 0, display: "grid", placeItems: "center", pointerEvents: "none" }}>
                  <Box sx={{ textAlign: "center" }}>
                    <Typography sx={{ fontFamily: monoFont, fontWeight: 600 }}>{formatCompact(categoryTotal)}</Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.7rem" }}>
                      RSD
                    </Typography>
                  </Box>
                </Box>
              </Box>

              <Stack spacing={0.75} sx={{ mt: 2 }}>
                {data.stockByCategory.map((item, index) => (
                  <Stack key={item.category} direction="row" spacing={1} alignItems="center">
                    <Box
                      sx={{
                        width: 8,
                        height: 8,
                        borderRadius: "50%",
                        bgcolor: chartColors.palette[index % chartColors.palette.length],
                        flexShrink: 0,
                      }}
                    />
                    <Typography variant="body2" sx={{ fontSize: "0.8rem", flexGrow: 1 }} noWrap>
                      {item.category}
                    </Typography>
                    <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "text.secondary" }}>
                      {categoryTotal > 0 ? Math.round((item.value / categoryTotal) * 100) : 0}%
                    </Typography>
                  </Stack>
                ))}
              </Stack>
            </>
          )}
        </Panel>
      </Box>

      <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", lg: "1fr 1fr 1fr" }, gap: 2 }}>
        <Panel title="Najvise izlaza — 30 dana">
          {data.topIssued.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              Nema evidentiranih izlaza.
            </Typography>
          ) : (
            <Stack spacing={1.5}>
              {data.topIssued.map((item, index) => (
                <Stack key={item.productId} direction="row" spacing={1.5} alignItems="center">
                  <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "text.disabled", width: 14 }}>
                    {index + 1}
                  </Typography>
                  <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                    <Typography variant="body2" sx={{ fontWeight: 500 }} noWrap>
                      {item.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.75rem" }}>
                      {formatQuantity(item.quantity)} {unitLabels[item.unitOfMeasure]}
                    </Typography>
                  </Box>
                  <Typography sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>{formatCompact(item.value)}</Typography>
                </Stack>
              ))}
            </Stack>
          )}
        </Panel>

        <Panel
          title="Niske zalihe"
          action={
            <MuiLink component={Link} to="/zalihe" variant="body2" underline="hover">
              Sve zalihe
            </MuiLink>
          }
        >
          {data.lowStock.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              Sve zalihe su iznad minimuma.
            </Typography>
          ) : (
            <Stack spacing={1.5}>
              {data.lowStock.map((item) => (
                <Box key={item.stockItemId}>
                  <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                    <Typography variant="body2" sx={{ fontWeight: 500 }} noWrap>
                      {item.productName}
                    </Typography>
                    <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "warning.main" }}>
                      {formatQuantity(item.quantity)} / {formatQuantity(item.minStock)} {unitLabels[item.unitOfMeasure]}
                    </Typography>
                  </Stack>
                  <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.75rem" }}>
                    {item.storeName ?? item.locationCode}
                  </Typography>
                  <Box sx={{ mt: 0.5, height: 4, bgcolor: "#F4E6DA", borderRadius: 2, overflow: "hidden" }}>
                    <Box
                      sx={{
                        height: "100%",
                        width: `${Math.min(100, (item.quantity / item.minStock) * 100)}%`,
                        bgcolor: "warning.main",
                      }}
                    />
                  </Box>
                </Box>
              ))}
            </Stack>
          )}
        </Panel>

        <Panel
          title="Nalozi po statusima"
          action={
            <MuiLink component={Link} to="/nalozi" variant="body2" underline="hover">
              Tabla
            </MuiLink>
          }
        >
          <Stack spacing={1.25}>
            {data.ordersByStatus.length === 0 && (
              <Typography variant="body2" color="text.secondary">
                Jos nema naloga.
              </Typography>
            )}
            {data.ordersByStatus.map((item) => (
              <Stack key={item.status} direction="row" alignItems="center" spacing={1.5}>
                <Typography variant="body2" sx={{ width: 120, flexShrink: 0, fontSize: "0.82rem" }}>
                  {orderStatusLabels[item.status]}
                </Typography>
                <Box sx={{ flexGrow: 1, height: 6, bgcolor: "#EEF1F2", borderRadius: 3, overflow: "hidden" }}>
                  <Box
                    sx={{
                      height: "100%",
                      width: `${(item.count / maxStatus) * 100}%`,
                      bgcolor: statusColors[item.status],
                    }}
                  />
                </Box>
                <Typography sx={{ fontFamily: monoFont, fontSize: "0.8rem", width: 28, textAlign: "right" }}>
                  {item.count}
                </Typography>
              </Stack>
            ))}
          </Stack>
        </Panel>
      </Box>
    </Box>
  );
}
