import { createTheme } from "@mui/material/styles";

const ink = "#16191C";
const pine = "#1F5F4B";
const rust = "#B4541A";
const clay = "#9B2C2C";

export const sidebar = {
  bg: "#0E1A15",
  bgHover: "#17302A",
  bgActive: "#1B3A2F",
  text: "#B9C5BF",
  textMuted: "#6F8079",
  textStrong: "#FFFFFF",
  border: "#1C2F28",
};

export const statusColors = {
  1: "#6B7280",
  2: "#B4541A",
  3: "#1F5F4B",
  4: "#1D5A87",
  5: "#14532D",
  6: "#9B2C2C",
};

export const statusTints = {
  1: "#F1F2F4",
  2: "#FAF0E9",
  3: "#EDF4F1",
  4: "#EDF2F7",
  5: "#ECF2EE",
  6: "#F8EDED",
};

const theme = createTheme({
  palette: {
    mode: "light",
    primary: { main: pine, light: "#2F7A61", dark: "#154234" },
    secondary: { main: "#1D5A87" },
    warning: { main: rust },
    error: { main: clay },
    success: { main: "#14532D" },
    background: { default: "#F2F4F5", paper: "#FFFFFF" },
    text: { primary: ink, secondary: "#5A6169" },
    divider: "#DDE2E5",
  },
  typography: {
    fontFamily: '"IBM Plex Sans", system-ui, -apple-system, sans-serif',
    h1: { fontSize: "1.75rem", fontWeight: 600, letterSpacing: "-0.01em" },
    h2: { fontSize: "1.375rem", fontWeight: 600, letterSpacing: "-0.01em" },
    h3: { fontSize: "1.125rem", fontWeight: 600 },
    body2: { fontSize: "0.875rem" },
    button: { textTransform: "none", fontWeight: 500 },
  },
  shape: { borderRadius: 6 },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { paddingInline: 16 } },
    },
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: "none" },
        outlined: { borderColor: "#DDE2E5" },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: { borderColor: "#E7EBED", paddingBlock: 10 },
        head: { fontWeight: 600, backgroundColor: "#F7F9FA", whiteSpace: "nowrap" },
      },
    },
    MuiTextField: { defaultProps: { size: "small" } },
    MuiSelect: { defaultProps: { size: "small" } },
    MuiChip: { styleOverrides: { root: { fontWeight: 500 } } },
  },
});

export default theme;

export const monoFont = '"IBM Plex Mono", ui-monospace, monospace';
