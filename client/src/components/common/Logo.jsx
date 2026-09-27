import { Box } from "@mui/material";

export default function Logo({ size = 28, color = "currentColor" }) {
  return (
    <Box
      component="svg"
      viewBox="0 0 24 24"
      sx={{ width: size, height: size, display: "block", color, flexShrink: 0 }}
      aria-hidden="true"
    >
      <rect x="2" y="18" width="20" height="3" rx="1.2" fill="currentColor" />
      <rect x="3.5" y="10.5" width="7.5" height="6" rx="1.2" fill="currentColor" opacity="0.5" />
      <rect x="13" y="5" width="7.5" height="11.5" rx="1.2" fill="currentColor" />
    </Box>
  );
}
