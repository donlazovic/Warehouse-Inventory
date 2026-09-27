import { Box, Paper, Stack, Typography } from "@mui/material";
import Logo from "../common/Logo";
import { sidebar } from "../../theme";

export default function AuthLayout({ children, maxWidth = 400 }) {
  return (
    <Box sx={{ minHeight: "100vh", display: "grid", gridTemplateColumns: { xs: "1fr", md: "1.1fr 1fr" } }}>
      <Box
        sx={{
          display: { xs: "none", md: "flex" },
          flexDirection: "column",
          justifyContent: "space-between",
          px: 8,
          py: 6,
          color: "#FFFFFF",
          bgcolor: sidebar.bg,
          backgroundImage: `
            linear-gradient(140deg, rgba(31,95,75,0.55) 0%, rgba(14,26,21,0.92) 65%),
            repeating-linear-gradient(0deg, rgba(255,255,255,0.05) 0 1px, transparent 1px 88px),
            repeating-linear-gradient(90deg, rgba(255,255,255,0.05) 0 1px, transparent 1px 132px)
          `,
        }}
      >
        <Stack direction="row" alignItems="center" spacing={1.5}>
          <Box sx={{ color: "#4EA88A" }}>
            <Logo size={30} />
          </Box>
          <Typography sx={{ fontWeight: 600, fontSize: "1.15rem" }}>Skladisnik</Typography>
        </Stack>

        <Box sx={{ maxWidth: "46ch" }}>
          <Typography sx={{ fontSize: "2.4rem", fontWeight: 600, letterSpacing: "-0.02em", lineHeight: 1.15 }}>
            Cela roba na jednom mestu
          </Typography>
          <Typography sx={{ color: "#9DB3AA", mt: 2.5, lineHeight: 1.75 }}>
            Evidencija artikala, stanje zaliha po skladistima i prodajnim objektima, i tok
            naloga od nabavke do isporuke.
          </Typography>
        </Box>

        <Typography sx={{ color: "#6F8079", fontSize: "0.78rem" }}>
          Interni sistem maloprodajnog lanca
        </Typography>
      </Box>

      <Box sx={{ display: "grid", placeItems: "center", p: 3, bgcolor: "background.default" }}>
        <Paper variant="outlined" sx={{ p: { xs: 3, sm: 4.5 }, width: "100%", maxWidth }}>
          <Stack direction="row" alignItems="center" spacing={1.5} sx={{ mb: 3, display: { md: "none" } }}>
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
