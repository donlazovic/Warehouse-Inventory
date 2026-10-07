import { Box, Paper, Stack, Typography } from "@mui/material";
import { useEffect } from "react";
import { ColorModeToggle } from "../../ColorMode";
import Logo from "../common/Logo";

export default function AuthLayout({ children, maxWidth = 400, documentTitle }) {
  useEffect(() => {
    document.title = documentTitle ? `${documentTitle} · Skladisnik` : "Skladisnik";
  }, [documentTitle]);

  return (
    <Box sx={{ minHeight: "100vh", display: "grid", gridTemplateColumns: { xs: "1fr", md: "1.1fr 1fr" } }}>
      <Box
        sx={{
          display: { xs: "none", md: "flex" },
          flexDirection: "column",
          justifyContent: "space-between",
          px: 8,
          py: 6,
          color: "sidebar.textStrong",
          bgcolor: "sidebar.bg",
          backgroundImage: (theme) => `
            linear-gradient(140deg, ${theme.palette.sidebar.heroFrom} 0%, ${theme.palette.sidebar.heroTo} 65%),
            repeating-linear-gradient(0deg, rgba(255,255,255,0.05) 0 1px, transparent 1px 88px),
            repeating-linear-gradient(90deg, rgba(255,255,255,0.05) 0 1px, transparent 1px 132px)
          `,
        }}
      >
        <Stack direction="row" spacing={1.5} sx={{
          alignItems: "center"
        }}>
          <Box sx={{ color: "sidebar.logo" }}>
            <Logo size={30} />
          </Box>
          <Typography sx={{ fontWeight: 600, fontSize: "1.15rem" }}>Skladisnik</Typography>
        </Stack>

        <Box sx={{ maxWidth: "46ch" }}>
          <Typography sx={{ fontSize: "2.4rem", fontWeight: 600, letterSpacing: "-0.02em", lineHeight: 1.15 }}>
            Cela roba na jednom mestu
          </Typography>
          <Typography sx={{ color: "sidebar.heroText", mt: 2.5, lineHeight: 1.75 }}>
            Evidencija artikala, stanje zaliha po skladistima i prodajnim objektima, i tok
            naloga od nabavke do isporuke.
          </Typography>
        </Box>

        <Typography sx={{ color: "sidebar.textMuted", fontSize: "0.78rem" }}>
          Interni sistem maloprodajnog lanca
        </Typography>
      </Box>

      <Box sx={{ display: "grid", placeItems: "center", p: 3, bgcolor: "background.default", position: "relative" }}>
        <ColorModeToggle sx={{ position: "absolute", top: 16, right: 16 }} />
        <Paper variant="outlined" sx={{ p: { xs: 3, sm: 4.5 }, width: "100%", maxWidth }}>
          <Stack
            direction="row"
            spacing={1.5}
            sx={{
              alignItems: "center",
              mb: 3,
              display: { md: "none" }
            }}>
            <Box sx={{ color: "primary.main" }}>
              <Logo size={26} />
            </Box>
            <Typography sx={{ fontWeight: 600, fontSize: "1.1rem" }}>Skladisnik</Typography>
          </Stack>
          {children}
        </Paper>
      </Box>
    </Box>
  );
}
