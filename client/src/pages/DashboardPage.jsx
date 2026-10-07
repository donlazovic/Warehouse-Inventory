import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import OutputIcon from "@mui/icons-material/Output";
import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Alert, Box, Button, Paper, Stack, Typography } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
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
import useRealtimeEvent from "../hooks/useRealtimeEvent";
import { monoFont, statusColors } from "../theme";
import { formatCompact, formatMoney, formatQuantity, orderStatusLabels, unitLabels } from "../utils/format";

const flowKeys = [
  { key: "receivedValue", label: "Ulaz", tone: "received" },
  { key: "transferredValue", label: "Prenos", tone: "transferred" },
  { key: "issuedValue", label: "Izlaz", tone: "issued" },
];

function Panel({ title, subtitle, action, children }) {
  return (
    <Paper variant="outlined" sx={{ p: 2.5, display: "flex", flexDirection: "column", minWidth: 0 }}>
      <Stack direction="row" sx={{ alignItems: "flex-start", justifyContent: "space-between", gap: 2, mb: 2 }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="h3">{title}</Typography>
          {subtitle && (
            <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.78rem", mt: 0.25 }}>
              {subtitle}
            </Typography>
          )}
        </Box>
        {action}
      </Stack>
      {children}
    </Paper>
  );
}

function PanelLink({ to, children }) {
  return (
    <Button
      component={Link}
      to={to}
      size="small"
      variant="outlined"
      endIcon={<ChevronRightIcon />}
      sx={{ flexShrink: 0, whiteSpace: "nowrap", py: 0.25, pl: 1.25, pr: 0.75 }}
    >
      {children}
    </Button>
  );
}

function ClickableRow({ onClick, children, title }) {
  return (
    <Stack
      direction="row"
      role="button"
      tabIndex={0}
      title={title}
      onClick={onClick}
      onKeyDown={(event) => (event.key === "Enter" || event.key === " ") && onClick()}
      sx={{
        alignItems: "center",
        gap: 1.5,
        mx: -1.25,
        px: 1.25,
        py: 1,
        borderRadius: 1,
        cursor: "pointer",
        transition: "background-color 120ms",
        "&:hover, &:focus-visible": { bgcolor: "surface.muted", outline: "none" },
        "&:hover .row-arrow, &:focus-visible .row-arrow": { opacity: 1 },
      }}
    >
      {children}
      <ChevronRightIcon className="row-arrow" sx={{ fontSize: 18, color: "text.secondary", opacity: 0, transition: "opacity 120ms" }} />
    </Stack>
  );
}

function FlowTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null;

  return (
    <Paper variant="outlined" sx={{ p: 1.25, minWidth: 180 }}>
      <Typography variant="body2" sx={{ fontWeight: 600, mb: 0.5 }}>
        {new Date(label).toLocaleDateString("sr-Latn-RS", { weekday: "short", day: "2-digit", month: "2-digit" })}
      </Typography>
      {payload.map((entry) => (
        <Stack key={entry.dataKey} direction="row" sx={{ alignItems: "center", gap: 1 }}>
          <Box sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: entry.color }} />
          <Typography variant="body2" sx={{ fontSize: "0.78rem", flexGrow: 1 }}>
            {flowKeys.find((s) => s.key === entry.dataKey)?.label}
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{formatMoney(entry.value)}</Typography>
        </Stack>
      ))}
    </Paper>
  );
}

function CategoryDonut({ items, palette, onSelect }) {
  const [active, setActive] = useState(null);
  const total = items.reduce((sum, item) => sum + item.value, 0);
  const current = active == null ? null : items[active];
  const share = (value) => (total > 0 ? Math.round((value / total) * 100) : 0);
  const color = (index) => palette[index % palette.length];

  return (
    <>
      <Box sx={{ height: 200, position: "relative" }} onMouseLeave={() => setActive(null)}>
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={items}
              dataKey="value"
              nameKey="category"
              innerRadius={62}
              outerRadius={90}
              paddingAngle={2}
              stroke="none"
              isAnimationActive={false}
              onMouseEnter={(_, index) => setActive(index)}
              onClick={(_, index) => onSelect(items[index])}
            >
              {items.map((item, index) => (
                <Cell
                  key={item.categoryId}
                  fill={color(index)}
                  fillOpacity={active == null || active === index ? 1 : 0.35}
                  style={{ cursor: "pointer", transition: "fill-opacity 120ms" }}
                />
              ))}
            </Pie>
          </PieChart>
        </ResponsiveContainer>

        <Box sx={{ position: "absolute", inset: 0, display: "grid", placeItems: "center", pointerEvents: "none" }}>
          <Box sx={{ textAlign: "center", maxWidth: 108 }}>
            <Typography
              noWrap
              sx={{ fontSize: "0.72rem", color: current ? color(active) : "text.secondary", fontWeight: 600 }}
            >
              {current ? current.category : "Ukupno"}
            </Typography>
            <Typography sx={{ fontFamily: monoFont, fontWeight: 600, fontSize: "1.05rem", lineHeight: 1.3 }}>
              {formatCompact(current ? current.value : total)}
            </Typography>
            <Typography sx={{ fontSize: "0.7rem", color: "text.secondary" }}>
              {current ? `${share(current.value)}% · RSD` : "RSD"}
            </Typography>
          </Box>
        </Box>
      </Box>

      <Stack sx={{ mt: 1.5 }}>
        {items.map((item, index) => (
          <Stack
            key={item.categoryId}
            direction="row"
            onMouseEnter={() => setActive(index)}
            onMouseLeave={() => setActive(null)}
            onClick={() => onSelect(item)}
            sx={{
              alignItems: "center",
              gap: 1,
              mx: -1,
              px: 1,
              py: 0.5,
              borderRadius: 1,
              cursor: "pointer",
              bgcolor: active === index ? "surface.muted" : "transparent",
            }}
          >
            <Box sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: color(index), flexShrink: 0 }} />
            <Typography variant="body2" noWrap sx={{ fontSize: "0.8rem", flexGrow: 1 }}>
              {item.category}
            </Typography>
            <Typography sx={{ fontFamily: monoFont, fontSize: "0.76rem", color: "text.secondary" }}>
              {share(item.value)}%
            </Typography>
          </Stack>
        ))}
      </Stack>
    </>
  );
}

export default function DashboardPage() {
  const { user, can } = useAuth();
  const navigate = useNavigate();
  const chart = useTheme().palette.chart;
  const flowSeries = flowKeys.map((item) => ({ ...item, color: chart[item.tone] }));

  const [data, setData] = useState(null);
  const [error, setError] = useState(null);

  const canSeeReports = can("reports.view");

  const load = useCallback(() => {
    if (!canSeeReports) return;
    reportsApi.dashboard().then(setData).catch((err) => setError(err.message));
  }, [canSeeReports]);

  useEffect(load, [load]);
  useRealtimeEvent("ordersChanged", load);
  useRealtimeEvent("stockChanged", load);

  const header = (
    <PageHeader
      documentTitle="Pocetna"
      title={`Dobrodosli, ${user?.firstName ?? ""}`}
      description={new Date().toLocaleDateString("sr-Latn-RS", {
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric",
      })}
    />
  );

  if (!canSeeReports) {
    return (
      <Box>
        {header}
        <Typography sx={{ color: "text.secondary" }}>Izaberite stavku iz menija da biste nastavili.</Typography>
      </Box>
    );
  }

  if (error) return <Box>{header}<Alert severity="error">{error}</Alert></Box>;
  if (!data) return <Box>{header}<Typography sx={{ color: "text.secondary" }}>Ucitavanje...</Typography></Box>;

  const { kpi } = data;
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
          hint={kpi.pendingApproval === 1 ? "1 ceka odobrenje" : `${kpi.pendingApproval} cekaju odobrenje`}
          to="/nalozi"
        />
        <StatCard
          icon={WarningAmberOutlinedIcon}
          label="Ispod minimuma"
          value={kpi.belowMinimum}
          hint="Artikala po lokacijama"
          tone="warning.main"
          to="/zalihe?onlyBelowMinimum=true"
        />
        <StatCard
          icon={OutputIcon}
          label="Izlaz ovog meseca"
          value={formatMoney(kpi.issuedValueThisMonth)}
          trend={percentChange(kpi.issuedValueThisMonth, kpi.issuedValueLastMonth)}
          hint="u odnosu na isti period proslog meseca"
          tone="warning.main"
          to="/kretanja?movementType=3"
        />
      </StatGrid>

      <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", lg: "2fr 1fr" }, gap: 2, mb: 2 }}>
        <Panel
          title="Tok robe"
          subtitle="Poslednjih 14 dana, vrednost u dinarima"
          action={
            <Stack direction="row" sx={{ gap: 2, flexWrap: "wrap", pt: 0.5 }}>
              {flowSeries.map((series) => (
                <Stack key={series.key} direction="row" sx={{ alignItems: "center", gap: 0.75 }}>
                  <Box sx={{ width: 12, height: 3, bgcolor: series.color, borderRadius: 1 }} />
                  <Typography variant="body2" sx={{ fontSize: "0.78rem" }}>
                    {series.label}
                  </Typography>
                </Stack>
              ))}
            </Stack>
          }
        >
          <Box sx={{ height: 300 }}>
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={data.dailyFlow} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
                <CartesianGrid stroke={chart.grid} vertical={false} />
                <XAxis
                  dataKey="date"
                  tickFormatter={(value) =>
                    new Date(value).toLocaleDateString("sr-Latn-RS", { day: "2-digit", month: "2-digit" })
                  }
                  tick={{ fontSize: 11, fill: chart.axis }}
                  axisLine={false}
                  tickLine={false}
                />
                <YAxis
                  tickFormatter={formatCompact}
                  tick={{ fontSize: 11, fill: chart.axis }}
                  axisLine={false}
                  tickLine={false}
                  width={60}
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
        </Panel>

        <Panel title="Zalihe po kategorijama" subtitle="Klik vodi na zalihe te kategorije">
          {data.stockByCategory.length === 0 ? (
            <Typography variant="body2" sx={{ color: "text.secondary" }}>
              Nema robe na stanju.
            </Typography>
          ) : (
            <CategoryDonut
              items={data.stockByCategory}
              palette={chart.palette}
              onSelect={(item) => navigate(`/zalihe?categoryId=${item.categoryId}`)}
            />
          )}
        </Panel>
      </Box>

      <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", lg: "1fr 1fr 1fr" }, gap: 2 }}>
        <Panel title="Najvise izlaza" subtitle="Poslednjih 30 dana" action={<PanelLink to="/kretanja?movementType=3">Kretanja</PanelLink>}>
          {data.topIssued.length === 0 ? (
            <Typography variant="body2" sx={{ color: "text.secondary" }}>
              Nema evidentiranih izlaza.
            </Typography>
          ) : (
            <Stack>
              {data.topIssued.map((item, index) => (
                <ClickableRow
                  key={item.productId}
                  title="Prikazi izlaze ovog artikla"
                  onClick={() => navigate(`/kretanja?productId=${item.productId}&movementType=3`)}
                >
                  <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "text.disabled", width: 16 }}>
                    {index + 1}
                  </Typography>
                  <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                    <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
                      {item.name}
                    </Typography>
                    <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
                      {formatQuantity(item.quantity)} {unitLabels[item.unitOfMeasure]}
                    </Typography>
                  </Box>
                  <Typography sx={{ fontFamily: monoFont, fontSize: "0.8rem", whiteSpace: "nowrap" }}>
                    {formatCompact(item.value)}
                  </Typography>
                </ClickableRow>
              ))}
            </Stack>
          )}
        </Panel>

        <Panel title="Niske zalihe" subtitle="Najkriticnije stavke" action={<PanelLink to="/zalihe?onlyBelowMinimum=true">Sve zalihe</PanelLink>}>
          {data.lowStock.length === 0 ? (
            <Typography variant="body2" sx={{ color: "text.secondary" }}>
              Sve zalihe su iznad minimuma.
            </Typography>
          ) : (
            <Stack>
              {data.lowStock.map((item) => (
                <ClickableRow
                  key={item.stockItemId}
                  title="Otvori ovu stavku u zalihama"
                  onClick={() =>
                    navigate(`/zalihe?storageLocationId=${item.storageLocationId}&search=${encodeURIComponent(item.productSku)}`)
                  }
                >
                  <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                    <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "baseline", gap: 1.5 }}>
                      <Typography variant="body2" noWrap sx={{ fontWeight: 500, minWidth: 0 }}>
                        {item.productName}
                      </Typography>
                      <Typography
                        sx={{ fontFamily: monoFont, fontSize: "0.76rem", color: "warning.main", whiteSpace: "nowrap", flexShrink: 0 }}
                      >
                        {formatQuantity(item.quantity)} / {formatQuantity(item.minStock)} {unitLabels[item.unitOfMeasure]}
                      </Typography>
                    </Stack>
                    <Typography variant="body2" noWrap sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
                      {item.storeName ?? item.locationCode}
                    </Typography>
                    <Box sx={{ mt: 0.75, height: 4, bgcolor: "tint.warningTrack", borderRadius: 2, overflow: "hidden" }}>
                      <Box
                        sx={{
                          height: "100%",
                          width: `${Math.min(100, (item.quantity / item.minStock) * 100)}%`,
                          bgcolor: "warning.main",
                        }}
                      />
                    </Box>
                  </Box>
                </ClickableRow>
              ))}
            </Stack>
          )}
        </Panel>

        <Panel title="Nalozi po statusima" subtitle="Klik otvara listu u tom statusu" action={<PanelLink to="/nalozi">Tabla</PanelLink>}>
          {data.ordersByStatus.length === 0 ? (
            <Typography variant="body2" sx={{ color: "text.secondary" }}>
              Jos nema naloga.
            </Typography>
          ) : (
            <Stack>
              {data.ordersByStatus.map((item) => (
                <ClickableRow
                  key={item.status}
                  title={`Prikazi naloge: ${orderStatusLabels[item.status]}`}
                  onClick={() => navigate(`/nalozi?status=${item.status}`)}
                >
                  <Typography variant="body2" sx={{ width: 120, flexShrink: 0, fontSize: "0.82rem" }}>
                    {orderStatusLabels[item.status]}
                  </Typography>
                  <Box sx={{ flexGrow: 1, height: 6, bgcolor: "surface.track", borderRadius: 3, overflow: "hidden" }}>
                    <Box
                      sx={{
                        height: "100%",
                        width: `${Math.max(2, (item.count / maxStatus) * 100)}%`,
                        bgcolor: statusColors[item.status],
                      }}
                    />
                  </Box>
                  <Typography sx={{ fontFamily: monoFont, fontSize: "0.8rem", minWidth: 40, textAlign: "right" }}>
                    {formatQuantity(item.count)}
                  </Typography>
                </ClickableRow>
              ))}
            </Stack>
          )}
        </Panel>
      </Box>
    </Box>
  );
}
