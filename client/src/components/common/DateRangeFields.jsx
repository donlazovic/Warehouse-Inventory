import { endOfDayIso, startOfDayIso, toDateInput } from "../../utils/format";
import DatePickerField from "./DatePickerField";

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
      <DatePickerField
        label={fromLabel}
        value={fromValue}
        onChange={(value) => onChange({ from: startOfDayIso(value), to })}
        max={toValue || undefined}
        rangeEnd={toValue || undefined}
        width={172}
      />
      <DatePickerField
        label={toLabel}
        value={toValue}
        error={invalid}
        helperText={invalid ? "Pre pocetnog datuma" : undefined}
        onChange={(value) => onChange({ from, to: endOfDayIso(value) })}
        min={fromValue || undefined}
        rangeStart={fromValue || undefined}
        width={172}
      />
    </>
  );
}
