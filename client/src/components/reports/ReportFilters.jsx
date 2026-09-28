import { MenuItem, Paper, Stack, TextField } from "@mui/material";

export function DateField({ label, value, onChange }) {
  return (
    <TextField
      label={label}
      type="date"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      slotProps={{ inputLabel: { shrink: true } }}
      sx={{ width: 170 }}
    />
  );
}

export function LocationSelect({ locations, value, onChange, emptyLabel = "Sve lokacije" }) {
  return (
    <TextField
      select
      label="Lokacija"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      sx={{ minWidth: 220 }}
    >
      <MenuItem value="">{emptyLabel}</MenuItem>
      {locations.map((location) => (
        <MenuItem key={location.id} value={location.id}>
          {location.code} — {location.name}
        </MenuItem>
      ))}
    </TextField>
  );
}

export function CategorySelect({ categories, value, onChange }) {
  return (
    <TextField
      select
      label="Kategorija"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      sx={{ minWidth: 180 }}
    >
      <MenuItem value="">Sve kategorije</MenuItem>
      {categories.map((category) => (
        <MenuItem key={category.id} value={category.id}>
          {category.name}
        </MenuItem>
      ))}
    </TextField>
  );
}

export function FilterBar({ children }) {
  return (
    <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
      <Stack direction={{ xs: "column", md: "row" }} spacing={2} alignItems={{ md: "center" }}>
        {children}
      </Stack>
    </Paper>
  );
}

export const cleanParams = (params) =>
  Object.fromEntries(Object.entries(params).filter(([, value]) => value !== "" && value != null));
