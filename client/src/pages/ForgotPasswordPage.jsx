import MarkEmailReadOutlinedIcon from "@mui/icons-material/MarkEmailReadOutlined";
import { Alert, Box, Button, Link, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { Link as RouterLink, Navigate } from "react-router-dom";
import { authApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import AuthLayout from "../components/auth/AuthLayout";

export default function ForgotPasswordPage() {
  const { user } = useAuth();
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to="/" replace />;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      await authApi.forgotPassword(email.trim());
      setSent(true);
    } catch (err) {
      setError(err.response?.status === 429 ? "Previse pokusaja. Sacekajte minut pa probajte ponovo." : err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AuthLayout>
      {sent ? (
        <Stack spacing={2.5} sx={{
          alignItems: "flex-start"
        }}>
          <Box sx={{ color: "primary.main" }}>
            <MarkEmailReadOutlinedIcon sx={{ fontSize: 40 }} />
          </Box>
          <Typography variant="h2">Proverite inbox</Typography>
          <Typography variant="body2" sx={{
            color: "text.secondary"
          }}>
            Ako nalog sa adresom <strong>{email.trim()}</strong> postoji, poslali smo link za promenu
            lozinke. Link vazi 30 minuta i moze se iskoristiti samo jednom.
          </Typography>
          <Typography variant="body2" sx={{
            color: "text.secondary"
          }}>
            Ne vidite mejl? Proverite folder za nezeljenu postu, ili posaljite zahtev ponovo za minut.
          </Typography>
          <Stack direction="row" spacing={1}>
            <Button component={RouterLink} to="/prijava" variant="contained">
              Nazad na prijavu
            </Button>
            <Button onClick={() => setSent(false)}>Posalji ponovo</Button>
          </Stack>
        </Stack>
      ) : (
        <form onSubmit={handleSubmit}>
          <Typography variant="h2" gutterBottom>
            Zaboravljena lozinka
          </Typography>
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mb: 3
            }}>
            Unesite email svog naloga i poslacemo vam link za postavljanje nove lozinke.
          </Typography>

          <Stack spacing={2.5}>
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

            <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
              {submitting ? "Slanje..." : "Posalji link"}
            </Button>

            <Typography
              variant="body2"
              sx={{
                color: "text.secondary",
                textAlign: "center"
              }}>
              Setili ste se?{" "}
              <Link component={RouterLink} to="/prijava" underline="hover">
                Prijavite se
              </Link>
            </Typography>
          </Stack>
        </form>
      )}
    </AuthLayout>
  );
}
