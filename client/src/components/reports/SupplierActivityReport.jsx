import { Alert, Box, MenuItem, Paper, TextField, Typography } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { useEffect, useState } from "react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip as ChartTooltip, XAxis, YAxis } from "recharts";
import { exportsApi, reportsApi, suppliersApi } from "../../api/endpoints";
import { useAuth } from "../../auth/AuthContext";
import ExportMenu from "../common/ExportMenu";
import ClearFiltersButton from "../common/ClearFiltersButton";
import { monoFont } from "../../theme";
import {
  endOfDayIso,
  formatCompact,
  formatMoney,
  monthLabels,
  monthsAgoInput,
  startOfDayIso,
  toDateInput,
  unitLabels,
} from "../../utils/format";
import ReportTable from "../common/ReportTable";
import StatCard, { StatGrid } from "../common/StatCard";
import Quantity from "./Quantity";
import { DateField, FilterBar } from "./ReportFilters";

export default function SupplierActivityReport() {
  const { can } = useAuth();
  const theme = useTheme();
  const chart = theme.palette.chart;
  const [suppliers, setSuppliers] = useState([]);
  const defaults = () => ({ supplierId: "", from: monthsAgoInput(6), to: toDateInput(new Date()) });
  const [filter, setFilter] = useState(defaults);
  const changed = JSON.stringify(filter) !== JSON.stringify(defaults());
  const [report, setReport] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    suppliersApi.list({ pageSize: 100 }).then((result) => setSuppliers(result.items)).catch(() => {});
  }, []);

  useEffect(() => {
    if (filter.supplierId === "") {
      setReport(null);
      return;
    }
    setError(null);

    reportsApi
      .supplierActivity(filter.supplierId, { from: startOfDayIso(filter.from), to: endOfDayIso(filter.to) })
      .then(setReport)
      .catch((err) => setError(err.message));
  }, [filter]);

  const set = (field) => (value) => setFilter((current) => ({ ...current, [field]: value }));

  const monthly = (report?.monthly ?? []).map((item) => ({
    ...item,
    label: `${monthLabels[item.month - 1]} ${String(item.year).slice(2)}`,
  }));

  const columns = [
    {
      field: "name",
      headerName: "Proizvod",
      render: (row) => (
        <Box>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>
            {row.name}
          </Typography>
          <Typography sx={{ fontFamily: monoFont, fontSize: "0.72rem", color: "text.secondary" }}>
            {row.sku}
          </Typography>
        </Box>
      ),
    },
    {
      field: "quantity",
      headerName: "Nabavljeno",
      align: "right",
      render: (row) => (
        <>
          <Quantity value={row.quantity} />
          <Box component="span" sx={{ ml: 0.5, fontSize: "0.78rem", color: "text.secondary" }}>
            {unitLabels[row.unitOfMeasure]}
          </Box>
        </>
      ),
    },
    {
      field: "value",
      headerName: "Vrednost",
      align: "right",
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem" }}>
          {formatMoney(row.value)}
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <FilterBar>
        <TextField
          select
          label="Dobavljac"
          value={filter.supplierId}
          onChange={(event) => set("supplierId")(event.target.value)}
          sx={{ minWidth: 260 }}
        >
          {suppliers.map((supplier) => (
            <MenuItem key={supplier.id} value={supplier.id}>
              {supplier.name}
            </MenuItem>
          ))}
        </TextField>
        <DateField label="Od" value={filter.from} onChange={set("from")} />
        <DateField label="Do" value={filter.to} onChange={set("to")} />
        <ClearFiltersButton active={changed} onClick={() => setFilter(defaults())} />
        {can("reports.export") && (
          <ExportMenu
            disabled={filter.supplierId === ""}
            fileName={`dobavljac-${filter.supplierId}-${filter.from}-${filter.to}`}
            previewTitle="Aktivnost dobavljaca"
            load={(format) =>
              exportsApi.supplierActivity(filter.supplierId, format, {
                from: startOfDayIso(filter.from),
                to: endOfDayIso(filter.to),
              })
            }
          />
        )}
      </FilterBar>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {!report && !error && (
        <Paper variant="outlined" sx={{ p: 5, textAlign: "center" }}>
          <Typography sx={{
            color: "text.secondary"
          }}>Izaberite dobavljaca da biste videli njegovu aktivnost.</Typography>
        </Paper>
      )}

      {report && (
        <>
          <StatGrid columns={4}>
            <StatCard label="Nalozi u periodu" value={report.totalOrders} hint={`${report.openOrders} jos u toku`} />
            <StatCard
              label="Realizovano"
              value={report.completedOrders}
              hint={report.cancelledOrders ? `${report.cancelledOrders} otkazano` : "Nijedan otkazan"}
            />
            <StatCard label="Vrednost nabavki" value={formatMoney(report.completedValue)} hint="Realizovani nalozi" />
            <StatCard
              label="Prosecno vreme realizacije"
              value={report.averageLeadDays != null ? `${report.averageLeadDays} d` : "—"}
              hint="Od kreiranja do prijema"
            />
          </StatGrid>

          <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", lg: "3fr 2fr" }, gap: 2 }}>
            <Paper variant="outlined" sx={{ p: 2.5 }}>
              <Typography variant="h3" sx={{ mb: 2 }}>
                Vrednost nabavki po mesecima
              </Typography>
              {monthly.length === 0 ? (
                <Typography variant="body2" sx={{
                  color: "text.secondary"
                }}>
                  Nema realizovanih naloga u periodu.
                </Typography>
              ) : (
                <Box sx={{ height: 260 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={monthly} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
                      <CartesianGrid stroke={chart.grid} vertical={false} />
                      <XAxis dataKey="label" tick={{ fontSize: 11, fill: chart.axis }} axisLine={false} tickLine={false} />
                      <YAxis tickFormatter={formatCompact} tick={{ fontSize: 11, fill: chart.axis }} axisLine={false} tickLine={false} width={52} />
                      <ChartTooltip
                        formatter={(value) => [formatMoney(value), "Vrednost"]}
                        labelStyle={{ fontWeight: 600, color: theme.palette.text.primary }}
                        itemStyle={{ color: theme.palette.text.primary }}
                        contentStyle={{
                          backgroundColor: theme.palette.background.paper,
                          borderColor: theme.palette.divider,
                          borderRadius: 6,
                        }}
                        cursor={{ fill: chart.cursor }}
                      />
                      <Bar dataKey="value" fill={chart.received} radius={[4, 4, 0, 0]} maxBarSize={48} />
                    </BarChart>
                  </ResponsiveContainer>
                </Box>
              )}
            </Paper>

            <Box>
              <Typography variant="h3" sx={{ mb: 1.5 }}>
                Najvise nabavljano
              </Typography>
              <ReportTable
                columns={columns}
                rows={report.topProducts}
                rowKey={(row) => row.productId}
                emptyText="Nema realizovanih nabavki."
                maxHeight={300}
              />
            </Box>
          </Box>
        </>
      )}
    </Box>
  );
}
