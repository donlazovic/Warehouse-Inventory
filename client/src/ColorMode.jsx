import DarkModeOutlinedIcon from "@mui/icons-material/DarkModeOutlined";
import LightModeOutlinedIcon from "@mui/icons-material/LightModeOutlined";
import { CssBaseline, IconButton, ThemeProvider, Tooltip } from "@mui/material";
import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { createAppTheme } from "./theme";

const STORAGE_KEY = "skladisnik.colorMode";
const query = "(prefers-color-scheme: dark)";

const ColorModeContext = createContext({ mode: "light", toggle: () => {} });

const readStored = () => {
  try {
    const value = localStorage.getItem(STORAGE_KEY);
    return value === "light" || value === "dark" ? value : null;
  } catch {
    return null;
  }
};

export function ColorModeProvider({ children }) {
  const [stored, setStored] = useState(readStored);
  const [systemDark, setSystemDark] = useState(() => window.matchMedia?.(query).matches ?? false);

  useEffect(() => {
    const media = window.matchMedia?.(query);
    if (!media) return undefined;

    const handle = (event) => setSystemDark(event.matches);
    media.addEventListener("change", handle);
    return () => media.removeEventListener("change", handle);
  }, []);

  const mode = stored ?? (systemDark ? "dark" : "light");

  const value = useMemo(
    () => ({
      mode,
      preference: stored ?? "system",
      toggle: () => {
        const next = mode === "dark" ? "light" : "dark";
        localStorage.setItem(STORAGE_KEY, next);
        setStored(next);
      },
      setPreference: (preference) => {
        if (preference === "system") {
          localStorage.removeItem(STORAGE_KEY);
          setStored(null);
        } else {
          localStorage.setItem(STORAGE_KEY, preference);
          setStored(preference);
        }
      },
    }),
    [mode, stored]
  );

  const theme = useMemo(() => createAppTheme(mode), [mode]);

  return (
    <ColorModeContext.Provider value={value}>
      <ThemeProvider theme={theme}>
        <CssBaseline enableColorScheme />
        {children}
      </ThemeProvider>
    </ColorModeContext.Provider>
  );
}

export function useColorMode() {
  return useContext(ColorModeContext);
}

export function ColorModeToggle({ sx }) {
  const { mode, toggle } = useColorMode();
  const dark = mode === "dark";

  return (
    <Tooltip title={dark ? "Svetli rezim" : "Tamni rezim"}>
      <IconButton size="small" onClick={toggle} sx={sx} aria-label={dark ? "Ukljuci svetli rezim" : "Ukljuci tamni rezim"}>
        {dark ? <LightModeOutlinedIcon fontSize="small" /> : <DarkModeOutlinedIcon fontSize="small" />}
      </IconButton>
    </Tooltip>
  );
}
