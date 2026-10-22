import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import CloseIcon from "@mui/icons-material/Close";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import {
  Box,
  Button,
  ButtonBase,
  IconButton,
  InputAdornment,
  Popover,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { alpha } from "@mui/material/styles";
import { useState } from "react";

const MONTHS = [
  "januar", "februar", "mart", "april", "maj", "jun",
  "jul", "avgust", "septembar", "oktobar", "novembar", "decembar",
];
const MONTHS_SHORT = ["jan", "feb", "mar", "apr", "maj", "jun", "jul", "avg", "sep", "okt", "nov", "dec"];
const WEEKDAYS = ["Po", "Ut", "Sr", "Ce", "Pe", "Su", "Ne"];

const pad = (n) => String(n).padStart(2, "0");
const keyOf = (y, m, d) => `${y}-${pad(m + 1)}-${pad(d)}`;
const todayKey = () => {
  const now = new Date();
  return keyOf(now.getFullYear(), now.getMonth(), now.getDate());
};
const parseKey = (key) => {
  const [y, m, d] = key.split("-").map(Number);
  return { y, m: m - 1, d };
};
const display = (key) => {
  if (!key) return "";
  const { y, m, d } = parseKey(key);
  return `${pad(d)}.${pad(m + 1)}.${y}.`;
};

/**
 * Polje za izbor datuma u stilu aplikacije.
 * Vrednost je string "YYYY-MM-DD" ili "", isto kao kod nativnog <input type="date">,
 * pa se moze koristiti umesto njega bez izmena okolnog koda.
 * rangeStart / rangeEnd (opciono) iscrtavaju izabrani period kada polje radi u paru.
 */
export default function DatePickerField({
  label,
  value,
  onChange,
  min,
  max,
  rangeStart,
  rangeEnd,
  error,
  helperText,
  width = 170,
  sx,
}) {
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);
  const [view, setView] = useState("days");
  const [cursor, setCursor] = useState(() => parseKey(value || todayKey()));

  const today = todayKey();
  const isDisabled = (key) => (min && key < min) || (max && key > max);

  const openPicker = (event) => {
    setCursor(parseKey(value || (max && today > max ? max : today)));
    setView("days");
    setAnchorEl(event.currentTarget);
  };
  const close = () => setAnchorEl(null);

  const pick = (key) => {
    if (isDisabled(key)) return;
    onChange(key);
    close();
  };

  const shiftMonth = (delta) =>
    setCursor(({ y, m }) => {
      const date = new Date(y, m + delta, 1);
      return { y: date.getFullYear(), m: date.getMonth(), d: 1 };
    });

  const shiftYear = (delta) => setCursor(({ y, m }) => ({ y: y + delta, m, d: 1 }));

  // 6 nedelja, pocinje od ponedeljka
  const first = new Date(cursor.y, cursor.m, 1);
  const offset = (first.getDay() + 6) % 7;
  const cells = Array.from({ length: 42 }, (_, i) => {
    const date = new Date(cursor.y, cursor.m, 1 - offset + i);
    return {
      key: keyOf(date.getFullYear(), date.getMonth(), date.getDate()),
      day: date.getDate(),
      inMonth: date.getMonth() === cursor.m,
    };
  });

  const lo = rangeStart || (rangeEnd ? value : null);
  const hi = rangeEnd || (rangeStart ? value : null);
  const inRange = (key) => Boolean(lo && hi && key > lo && key < hi);
  const isEndpoint = (key) => key === rangeStart || key === rangeEnd;

  return (
    <>
      <TextField
        label={label}
        value={display(value)}
        placeholder="dd.mm.gggg."
        error={error}
        helperText={helperText}
        onClick={openPicker}
        onKeyDown={(event) => {
          if (["Enter", " ", "ArrowDown"].includes(event.key)) {
            event.preventDefault();
            openPicker(event);
          }
        }}
        slotProps={{
          inputLabel: { shrink: true },
          htmlInput: { readOnly: true, style: { cursor: "pointer" } },
          input: {
            sx: { cursor: "pointer" },
            endAdornment: (
              <InputAdornment position="end" sx={{ gap: 0.25 }}>
                {value && (
                  <IconButton
                    size="small"
                    aria-label="Obrisi datum"
                    onClick={(event) => {
                      event.stopPropagation();
                      onChange("");
                    }}
                    sx={{ p: 0.25 }}
                  >
                    <CloseIcon sx={{ fontSize: 16 }} />
                  </IconButton>
                )}
                <CalendarMonthOutlinedIcon fontSize="small" sx={{ color: open ? "primary.main" : "text.secondary" }} />
              </InputAdornment>
            ),
          },
        }}
        sx={{ width, ...sx }}
      />

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={close}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
        transformOrigin={{ vertical: "top", horizontal: "left" }}
        slotProps={{ paper: { sx: { mt: 0.75, p: 1.5, width: 292, borderRadius: 2 } } }}
      >
        <Stack direction="row" sx={{ alignItems: "center", mb: 1 }}>
          <IconButton size="small" onClick={() => (view === "days" ? shiftMonth(-1) : shiftYear(-1))} aria-label="Prethodni">
            <ChevronLeftIcon fontSize="small" />
          </IconButton>
          <ButtonBase
            onClick={() => setView(view === "days" ? "months" : "days")}
            sx={{
              flexGrow: 1,
              py: 0.5,
              borderRadius: 1,
              gap: 0.25,
              fontWeight: 600,
              fontSize: "0.95rem",
              "&:hover": { bgcolor: "action.hover" },
            }}
          >
            {view === "days" ? `${MONTHS[cursor.m]} ${cursor.y}` : cursor.y}
            <KeyboardArrowDownIcon
              sx={{ fontSize: 18, transition: "transform 150ms", transform: view === "months" ? "rotate(180deg)" : "none" }}
            />
          </ButtonBase>
          <IconButton size="small" onClick={() => (view === "days" ? shiftMonth(1) : shiftYear(1))} aria-label="Sledeci">
            <ChevronRightIcon fontSize="small" />
          </IconButton>
        </Stack>

        {view === "months" ? (
          <Box sx={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 0.75, py: 0.5 }}>
            {MONTHS_SHORT.map((name, m) => {
              const selected = value && parseKey(value).y === cursor.y && parseKey(value).m === m;
              return (
                <ButtonBase
                  key={name}
                  onClick={() => {
                    setCursor({ y: cursor.y, m, d: 1 });
                    setView("days");
                  }}
                  sx={{
                    height: 40,
                    borderRadius: 1.5,
                    fontSize: "0.85rem",
                    fontWeight: selected ? 600 : 400,
                    bgcolor: selected ? "primary.main" : "transparent",
                    color: selected ? "primary.contrastText" : "text.primary",
                    "&:hover": { bgcolor: selected ? "primary.dark" : "action.hover" },
                  }}
                >
                  {name}
                </ButtonBase>
              );
            })}
          </Box>
        ) : (
          <>
            <Box sx={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)", mb: 0.5 }}>
              {WEEKDAYS.map((day, i) => (
                <Typography
                  key={day}
                  sx={{
                    textAlign: "center",
                    fontSize: "0.72rem",
                    fontWeight: 600,
                    color: i >= 5 ? "text.disabled" : "text.secondary",
                    py: 0.5,
                  }}
                >
                  {day}
                </Typography>
              ))}
            </Box>
            <Box sx={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)", rowGap: 0.25 }}>
              {cells.map((cell) => {
                const selected = cell.key === value;
                const disabled = isDisabled(cell.key);
                const between = inRange(cell.key);
                const endpoint = !selected && isEndpoint(cell.key);
                return (
                  <Box
                    key={cell.key}
                    sx={(theme) => ({
                      display: "flex",
                      justifyContent: "center",
                      bgcolor: between ? alpha(theme.palette.primary.main, 0.1) : "transparent",
                    })}
                  >
                    <ButtonBase
                      disabled={disabled}
                      onClick={() => pick(cell.key)}
                      sx={(theme) => ({
                        width: 36,
                        height: 36,
                        borderRadius: "50%",
                        fontSize: "0.85rem",
                        fontWeight: selected || cell.key === today ? 600 : 400,
                        color: selected
                          ? theme.palette.primary.contrastText
                          : !cell.inMonth
                            ? theme.palette.text.disabled
                            : theme.palette.text.primary,
                        bgcolor: selected
                          ? theme.palette.primary.main
                          : endpoint
                            ? alpha(theme.palette.primary.main, 0.22)
                            : "transparent",
                        border: cell.key === today && !selected ? `1px solid ${theme.palette.primary.main}` : "1px solid transparent",
                        opacity: disabled ? 0.35 : 1,
                        transition: "background-color 120ms",
                        "&:hover": {
                          bgcolor: selected ? theme.palette.primary.dark : alpha(theme.palette.primary.main, 0.12),
                        },
                      })}
                    >
                      {cell.day}
                    </ButtonBase>
                  </Box>
                );
              })}
            </Box>
          </>
        )}

        <Stack direction="row" sx={{ justifyContent: "space-between", mt: 1.25, pt: 1, borderTop: 1, borderColor: "divider" }}>
          <Button size="small" color="inherit" disabled={!value} onClick={() => { onChange(""); close(); }}>
            Obrisi
          </Button>
          <Button size="small" disabled={isDisabled(today)} onClick={() => pick(today)}>
            Danas
          </Button>
        </Stack>
      </Popover>
    </>
  );
}
