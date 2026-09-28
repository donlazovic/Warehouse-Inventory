import { Box } from "@mui/material";
import { monoFont } from "../../theme";
import { formatQuantity } from "../../utils/format";

export default function Quantity({ value, signed = false, muted = false, tone }) {
  if (!value) {
    return (
      <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.8rem", color: "text.disabled" }}>
        —
      </Box>
    );
  }

  return (
    <Box
      component="span"
      sx={{
        fontFamily: monoFont,
        fontSize: "0.8rem",
        color: tone ?? (muted ? "text.secondary" : "inherit"),
        whiteSpace: "nowrap",
      }}
    >
      {signed && value > 0 ? "+" : ""}
      {formatQuantity(value)}
    </Box>
  );
}
