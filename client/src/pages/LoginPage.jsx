import VisibilityOffOutlinedIcon from "@mui/icons-material/VisibilityOffOutlined";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Link,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import Logo from "../components/common/Logo";
import { sidebar } from "../theme";

const REMEMBERED_EMAIL = "skladisnik.rememberedEmail";

export default function LoginPage() {
  const { user, signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const remembered = localStorage.getItem(REMEMBERED_EMAIL) ?? "";

  const [email, setEmail] = useState(remembered);
  const [password, setPassword] = useState("");
  const [remember, setRemember] = useState(Boolean(remembered));
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState(null);
  const [hint, setHint] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  if (user) {
    return <Navigate to={location.state?.from?.pathname ?? "/"} replace />;
  }

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      await signIn(email.trim(), password);

      if (remember) localStorage.setItem(REMEMBERED_EMAIL, email.trim());
      else localStorage.removeItem(REMEMBERED_EMAIL);

      navigate(location.state?.from?.pathname ?? "/", { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

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
          position: "relative",
          overflow: "hidden",
          bgcolor: sidebar.bg,
          backgroundImage: `
            url("/login-background.jpg")
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
        <Paper
          variant="outlined"
          component="form"
          onSubmit={handleSubmit}
          sx={{ p: { xs: 3, sm: 4.5 }, width: "100%", maxWidth: 400 }}
        >
          <Stack direction="row" alignItems="center" spacing={1.5} sx={{ mb: 3, display: { md: "none" } }}>
            <Box sx={{ color: "primary.main" }}>
              <Logo size={26} />
            </Box>
            <Typography sx={{ fontWeight: 600, fontSize: "1.1rem" }}>Skladisnik</Typography>
          </Stack>

          <Typography variant="h2" gutterBottom>
            Prijava
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Unesite podatke svog naloga da biste nastavili.
          </Typography>

          <Stack spacing={2.5}>
            {error && <Alert severity="error">{error}</Alert>}
            {hint && (
              <Alert severity="info" onClose={() => setHint(false)}>
                Lozinku resetuje administrator sistema. Obratite mu se sa svojom email adresom.
              </Alert>
            )}

            <TextField
              label="Email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              autoFocus
              fullWidth
            />

            <TextField
              label="Lozinka"
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              fullWidth
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      size="small"
                      edge="end"
                      onClick={() => setShowPassword((current) => !current)}
                      aria-label={showPassword ? "Sakrij lozinku" : "Prikazi lozinku"}
                    >
                      {showPassword ? (
                        <VisibilityOffOutlinedIcon fontSize="small" />
                      ) : (
                        <VisibilityOutlinedIcon fontSize="small" />
                      )}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <FormControlLabel
                control={
                  <Checkbox
                    size="small"
                    checked={remember}
                    onChange={(event) => setRemember(event.target.checked)}
                  />
                }
                label={<Typography variant="body2">Zapamti me</Typography>}
              />
              <Link
                component="button"
                type="button"
                variant="body2"
                underline="hover"
                onClick={() => setHint(true)}
              >
                Zaboravljena lozinka?
              </Link>
            </Stack>

            <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
              {submitting ? "Prijavljivanje..." : "Prijavi se"}
            </Button>
          </Stack>
        </Paper>
      </Box>
    </Box>
  );
}
