import { TextField } from "@mui/material";
import { endOfDayIso, startOfDayIso, toDateInput } from "../../utils/format";

/**
 * Dva polja za datum koja citaju i pisu ISO vrednosti iz filtera.
 * Prikaz se izvodi iz filtera, pa "Ponisti filtere" automatski prazni i ova polja.
 * from se salje kao pocetak dana, to kao kraj dana, oba u lokalnom vremenu.
 */
export default function DateRangeFields({ from, to, onChange, fromLabel = "Od", toLabel = "Do" }) {
  const fromValue = from ? toDateInput(from) : "";
  const toValue = to ? toDateInput(to) : "";
  const invalid = Boolean(fromValue && toValue && fromValue > toValue);

  return (
    <>
      <TextField
        label={fromLabel}
        type="date"
        value={fromValue}
        onChange={(event) => onChange({ from: startOfDayIso(event.target.value), to })}
        slotProps={{ inputLabel: { shrink: true }, htmlInput: { max: toValue || undefined } }}
        sx={{ width: 160 }}
      />
      <TextField
        label={toLabel}
        type="date"
        value={toValue}
        error={invalid}
        helperText={invalid ? "Pre pocetnog datuma" : undefined}
        onChange={(event) => onChange({ from, to: endOfDayIso(event.target.value) })}
        slotProps={{ inputLabel: { shrink: true }, htmlInput: { min: fromValue || undefined } }}
        sx={{ width: 160 }}
      />
    </>
  );
}
