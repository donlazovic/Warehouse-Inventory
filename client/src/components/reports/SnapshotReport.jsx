import { Alert, Box, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { reportsApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { endOfDayIso, formatMoney, toDateInput, unitLabels } from "../../utils/format";
import ReportTable from "../common/ReportTable";
import StatCard, { StatGrid } from "../common/StatCard";
import Quantity from "./Quantity";
import { CategorySelect, cleanParams, DateField, FilterBar, LocationSelect } from "./ReportFilters";

export default function SnapshotReport({ locations, categories }) {
  const [filter, setFilter] = useState({ at: toDateInput(new Date()), locationId: "", categoryId: "" });
  const [report, setReport] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!filter.at) return;
    setError(null);

    reportsApi
      .snapshot(cleanParams({ at: endOfDayIso(filter.at), locationId: filter.locationId, categoryId: filter.categoryId }))
      .then(setReport)
      .catch((err) => setError(err.message));
  }, [filter]);

  const set = (field) => (value) => setFilter((current) => ({ ...current, [field]: value }));

  const columns = [
    {
      field: "location",
      headerName: "Lokacija",
      render: (row) => (
        <Box>
          <Typography sx={{ fontFamily: monoFont, fontSize: "0.78rem" }}>{row.locationCode}</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.75rem" }}>
            {row.locationName}
          </Typography>
        </Box>
      ),
    },
    {
      field: "product",
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
    { field: "category", headerName: "Kategorija" },
    {
      field: "quantity",
      headerName: "Kolicina",
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
      field: "price",
      headerName: "Cena",
      align: "right",
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.78rem", color: "text.secondary" }}>
          {formatMoney(row.price)}
        </Box>
      ),
    },
    {
      field: "value",
      headerName: "Vrednost",
      align: "right",
      render: (row) => (
        <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem", fontWeight: 500 }}>
          {formatMoney(row.value)}
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <FilterBar>
        <DateField label="Stanje na dan" value={filter.at} onChange={set("at")} />
        <LocationSelect locations={locations} value={filter.locationId} onChange={set("locationId")} />
        <CategorySelect categories={categories} value={filter.categoryId} onChange={set("categoryId")} />
      </FilterBar>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {report && (
        <>
          <StatGrid columns={2}>
            <StatCard label="Ukupna vrednost" value={formatMoney(report.totalValue)} hint="Po trenutnim cenama" />
            <StatCard label="Stavki na stanju" value={report.rows.length} hint="Artikal po lokaciji" />
          </StatGrid>

          <ReportTable
            columns={columns}
            rows={report.rows}
            rowKey={(row) => `${row.productId}-${row.locationId}`}
            emptyText="Na izabrani dan nije bilo robe na stanju."
          />

          <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5, maxWidth: "80ch" }}>
            Stanje nije preuzeto iz trenutnih zaliha, nego je izracunato ponovnim sabiranjem svih
            kretanja robe do kraja izabranog dana. Zato prikazuje stvarno stanje tog trenutka, cak i
            za datume u proslosti.
          </Typography>
        </>
      )}
    </Box>
  );
}
