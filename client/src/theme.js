import { alpha, createTheme } from "@mui/material/styles";

export const monoFont = '"IBM Plex Mono", ui-monospace, monospace';

export const statusColors = {
  1: "#6B7280",
  2: "#B4541A",
  3: "#1F5F4B",
  4: "#1D5A87",
  5: "#14532D",
  6: "#9B2C2C",
};

const tokens = {
  light: {
    background: { default: "#F2F4F5", paper: "#FFFFFF" },
    text: { primary: "#16191C", secondary: "#5A6169", disabled: "#A3AAB0" },
    divider: "#DDE2E5",
    primary: { main: "#1F5F4B", light: "#2F7A61", dark: "#154234", contrastText: "#FFFFFF" },
    secondary: { main: "#1D5A87" },
    warning: { main: "#B4541A" },
    error: { main: "#9B2C2C" },
    success: { main: "#14532D" },
    surface: {
      muted: "#F7F9FA",
      subtle: "#F2F4F5",
      track: "#EEF1F2",
      overlay: "rgba(255,255,255,0.7)",
      highlight: "#F3F8F6",
      preview: "#E9ECEE",
      tableLine: "#E7EBED",
    },
    tint: {
      success: "#EDF4F1",
      warning: "#FAF0E9",
      warningHover: "#F5E4D7",
      warningTrack: "#F4E6DA",
      danger: "#F8EDED",
      unread: "#FAF6F1",
      unreadHover: "#F5EDE3",
      readHover: "#F7F9FA",
    },
    statusTint: {
      1: "#F1F2F4",
      2: "#FAF0E9",
      3: "#EDF4F1",
      4: "#EDF2F7",
      5: "#ECF2EE",
      6: "#F8EDED",
    },
    sidebar: {
      bg: "#0E1A15",
      bgHover: "#17302A",
      bgActive: "#1B3A2F",
      text: "#B9C5BF",
      textMuted: "#6F8079",
      textStrong: "#FFFFFF",
      border: "#1C2F28",
      logo: "#4EA88A",
      heroFrom: "rgba(31,95,75,0.55)",
      heroTo: "rgba(14,26,21,0.92)",
      heroText: "#9DB3AA",
    },
    avatar: { bg: "#DDE7E3", fg: "#1F5F4B", strong: "#2F7A61" },
    connection: { online: "#2F9E6E", pending: "#C9A227", offline: "#9B2C2C" },
    chart: {
      received: "#1F5F4B",
      transferred: "#1D5A87",
      issued: "#B4541A",
      grid: "#EEF1F2",
      axis: "#5A6169",
      cursor: "#F2F4F5",
      palette: ["#1F5F4B", "#1D5A87", "#B4541A", "#7A8B3A", "#9B2C2C", "#2F7A61", "#B58A3C", "#6B7280"],
    },
  },
  dark: {
    background: { default: "#0C1311", paper: "#141E1A" },
    text: { primary: "#E3EAE6", secondary: "#8FA39A", disabled: "#5A6B64" },
    divider: "#26352F",
    primary: { main: "#4EA88A", light: "#6FC4A3", dark: "#2F7A61", contrastText: "#0C1311" },
    secondary: { main: "#5B9BD5" },
    warning: { main: "#E08A4F" },
    error: { main: "#E07068" },
    success: { main: "#4EA88A" },
    surface: {
      muted: "#1B2823",
      subtle: "#111A17",
      track: "#1F2C27",
      overlay: "rgba(12,19,17,0.72)",
      highlight: "#1B3A2F",
      preview: "#0A100E",
      tableLine: "#22302A",
    },
    tint: {
      success: "rgba(78,168,138,0.14)",
      warning: "rgba(224,138,79,0.14)",
      warningHover: "rgba(224,138,79,0.22)",
      warningTrack: "rgba(224,138,79,0.18)",
      danger: "rgba(224,112,104,0.14)",
      unread: "rgba(224,138,79,0.10)",
      unreadHover: "rgba(224,138,79,0.16)",
      readHover: "#1B2823",
    },
    statusTint: {
      1: "rgba(143,163,154,0.12)",
      2: "rgba(224,138,79,0.14)",
      3: "rgba(78,168,138,0.14)",
      4: "rgba(91,155,213,0.14)",
      5: "rgba(78,168,138,0.10)",
      6: "rgba(224,112,104,0.14)",
    },
    sidebar: {
      bg: "#080F0C",
      bgHover: "#12211B",
      bgActive: "#1B3A2F",
      text: "#B9C5BF",
      textMuted: "#6F8079",
      textStrong: "#FFFFFF",
      border: "#16241E",
      logo: "#4EA88A",
      heroFrom: "rgba(31,95,75,0.6)",
      heroTo: "rgba(8,15,12,0.96)",
      heroText: "#9DB3AA",
    },
    avatar: { bg: "#1B3A2F", fg: "#8FD1B5", strong: "#2F7A61" },
    connection: { online: "#4EC28F", pending: "#D9B545", offline: "#E07068" },
    chart: {
      received: "#4EA88A",
      transferred: "#5B9BD5",
      issued: "#E08A4F",
      grid: "#1F2C27",
      axis: "#8FA39A",
      cursor: "#1B2823",
      palette: ["#4EA88A", "#5B9BD5", "#E08A4F", "#A3B86C", "#E07068", "#6FC4A3", "#D6B06A", "#8C96A0"],
    },
  },
};

export function createAppTheme(mode = "light") {
  const t = tokens[mode];

  return createTheme({
    palette: {
      mode,
      ...t,
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
          outlined: { borderColor: t.divider },
        },
      },
      MuiTableCell: {
        styleOverrides: {
          root: { borderColor: t.surface.tableLine, paddingBlock: 10 },
          head: { fontWeight: 600, backgroundColor: t.surface.muted, whiteSpace: "nowrap" },
        },
      },
      MuiTableRow: {
        styleOverrides: {
          root: {
            "&.MuiTableRow-hover:hover": { backgroundColor: alpha(t.primary.main, mode === "dark" ? 0.06 : 0.03) },
          },
        },
      },
      MuiTextField: { defaultProps: { size: "small" } },
      MuiSelect: { defaultProps: { size: "small" } },
      MuiChip: { styleOverrides: { root: { fontWeight: 500 } } },
      MuiTooltip: {
        styleOverrides: {
          tooltip: mode === "dark" ? { backgroundColor: "#E3EAE6", color: "#0C1311" } : {},
        },
      },
    },
  });
}

export default createAppTheme("light");
