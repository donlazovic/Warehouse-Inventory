import { Alert, Box, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { exportsApi, reportsApi } from "../../api/endpoints";
import { useAuth } from "../../auth/AuthContext";
import ExportMenu from "../common/ExportMenu";
import { monoFont } from "../../theme";
import { daysAgoInput, endOfDayIso, formatMoney, startOfDayIso, toDateInput, unitLabels } from "../../utils/format";
import ReportTable from "../common/ReportTable";
import StatCard, { StatGrid } from "../common/StatCard";
import Quantity from "./Quantity";
import { CategorySelect, cleanParams, DateField, FilterBar, LocationSelect } from "./ReportFilters";

export default function TurnoverReport({ locations, categories }) {
  const { can } = useAuth();
  const [filter, setFilter] = useState({
    from: daysAgoInput(30),
    to: toDateInput(new Date()),
    locationId: "",
    categoryId: "",
  });
  const [report, setReport] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!filter.from || !filter.to) return;
    setLoading(true);
    setError(null);

    reportsApi
      .turnover(
        cleanParams({
          from: startOfDayIso(filter.from),
          to: endOfDayIso(filter.to),
          locationId: filter.locationId,
          categoryId: filter.categoryId,
        })
      )
      .then(setReport)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [filter]);

  const set = (field) => (value) => setFilter((current) => ({ ...current, [field]: value }));

  const params = () =>
    cleanParams({
      from: startOfDayIso(filter.from),
      to: endOfDayIso(filter.to),
      locationId: filter.locationId,
      categoryId: filter.categoryId,
    });

  const byLocation = filter.locationId !== "";

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
            {row.sku} · {row.category}
          </Typography>
        </Box>
      ),
    },
    { field: "unit", headerName: "JM", render: (row) => unitLabels[row.unitOfMeasure] },
    { field: "opening", headerName: "Pocetno", align: "right", render: (row) => <Quantity value={row.opening} muted /> },
    { field: "received", headerName: "Ulaz", align: "right", render: (row) => <Quantity value={row.received} tone="success.main" /> },
    ...(byLocation
      ? [
          { field: "in", headerName: "Prenos +", align: "right", render: (row) => <Quantity value={row.transferredIn} /> },
          { field: "out", headerName: "Prenos −", align: "right", render: (row) => <Quantity value={row.transferredOut} /> },
        ]
      : []),
    { field: "issued", headerName: "Izlaz", align: "right", render: (row) => <Quantity value={row.issued} tone="warning.main" /> },
    { field: "adjust", headerName: "Korekcija", align: "right", render: (row) => <Quantity value={row.adjustmentNet} signed /> },
    {
      field: "closing",
      headerName: "Zavrsno",
      align: "right",
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.82rem", fontWeight: 600 }}>
          {row.closing.toLocaleString("sr-Latn-RS", { maximumFractionDigits: 3 })}
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <FilterBar>
        <DateField label="Od" value={filter.from} onChange={set("from")} />
        <DateField label="Do" value={filter.to} onChange={set("to")} />
        <LocationSelect locations={locations} value={filter.locationId} onChange={set("locationId")} emptyLabel="Ceo lanac" />
        <CategorySelect categories={categories} value={filter.categoryId} onChange={set("categoryId")} />
        <Box sx={{ flexGrow: 1 }} />
        {can("reports.export") && (
          <ExportMenu
            fileName={`promet-${filter.from}-${filter.to}`}
            previewTitle="Promet robe"
            load={(format) => exportsApi.turnover(format, params())}
          />
        )}
      </FilterBar>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {report && (
        <>
          <StatGrid columns={3}>
            <StatCard label="Vrednost ulaza" value={formatMoney(report.totalReceivedValue)} hint="Prijem i pocetna stanja" />
            <StatCard label="Vrednost izlaza" value={formatMoney(report.totalIssuedValue)} hint="Prodaja, otpis, lom" tone="warning.main" />
            <StatCard label="Artikala sa prometom" value={report.rows.length} hint={loading ? "Osvezavanje..." : "U izabranom periodu"} />
          </StatGrid>

          <ReportTable columns={columns} rows={report.rows} rowKey={(row) => row.productId} />

          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mt: 1.5,
              maxWidth: "80ch"
            }}>
            Zavrsno = pocetno + ulaz {byLocation ? "+ prenos u lokaciju − prenos iz lokacije " : ""}− izlaz ± korekcija.
            {!byLocation &&
              " Za ceo lanac prenosi se ne prikazuju jer roba samo menja mesto unutar lanca — ukupna kolicina ostaje ista."}
          </Typography>
        </>
      )}
    </Box>
  );
}
