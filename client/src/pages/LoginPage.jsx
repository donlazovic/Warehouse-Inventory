import { Alert, Box, Button, Paper, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function LoginPage() {
  const { user, signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
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
      navigate(location.state?.from?.pathname ?? "/", { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",
        gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
      }}
    >
      <Box
        sx={{
          bgcolor: "#101B17",
          color: "#FFFFFF",
          display: { xs: "none", md: "flex" },
          flexDirection: "column",
          justifyContent: "center",
          px: 8,
        }}
      >
        <Typography sx={{ fontSize: "2.5rem", fontWeight: 600, letterSpacing: "-0.02em" }}>
          Magacin
        </Typography>
        <Typography sx={{ color: "#8FA39A", mt: 2, maxWidth: "42ch", lineHeight: 1.7 }}>
          Evidencija robe, stanje zaliha po lokacijama i tok naloga izmedju centralnog
          magacina i prodajnih objekata.
        </Typography>
      </Box>

      <Box sx={{ display: "grid", placeItems: "center", p: 3 }}>
        <Paper
          variant="outlined"
          component="form"
          onSubmit={handleSubmit}
          sx={{ p: 4, width: "100%", maxWidth: 380 }}
        >
          <Typography variant="h2" gutterBottom>
            Prijava
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Unesite podatke svog naloga.
          </Typography>

          <Stack spacing={2}>
            {error && <Alert severity="error">{error}</Alert>}

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
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              fullWidth
            />

            <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
              {submitting ? "Prijavljivanje..." : "Prijavi se"}
            </Button>
          </Stack>
        </Paper>
      </Box>
    </Box>
  );
}
