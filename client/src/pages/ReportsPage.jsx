import { Box, Tab, Tabs } from "@mui/material";
import { useEffect, useState } from "react";
import { categoriesApi, locationsApi } from "../api/endpoints";
import PageHeader from "../components/common/PageHeader";
import SnapshotReport from "../components/reports/SnapshotReport";
import SupplierActivityReport from "../components/reports/SupplierActivityReport";
import TurnoverReport from "../components/reports/TurnoverReport";

const tabs = [
  { value: "turnover", label: "Promet robe" },
  { value: "snapshot", label: "Stanje na dan" },
  { value: "supplier", label: "Aktivnost dobavljaca" },
];

export default function ReportsPage() {
  const [tab, setTab] = useState("turnover");
  const [locations, setLocations] = useState([]);
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    locationsApi.lookup().then(setLocations).catch(() => {});
    categoriesApi.list({ pageSize: 100 }).then((result) => setCategories(result.items)).catch(() => {});
  }, []);

  return (
    <Box>
      <PageHeader
        title="Izvestaji"
        description="Promet robe u periodu, stanje zaliha na izabrani dan i aktivnost dobavljaca."
      />

      <Tabs value={tab} onChange={(_, next) => setTab(next)} sx={{ mb: 2, borderBottom: 1, borderColor: "divider" }}>
        {tabs.map((item) => (
          <Tab key={item.value} value={item.value} label={item.label} />
        ))}
      </Tabs>

      {tab === "turnover" && <TurnoverReport locations={locations} categories={categories} />}
      {tab === "snapshot" && <SnapshotReport locations={locations} categories={categories} />}
      {tab === "supplier" && <SupplierActivityReport />}
    </Box>
  );
}
