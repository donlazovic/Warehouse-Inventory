import {
  Alert,
  Button,
  Checkbox,
  FormControlLabel,
  Link,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { Link as RouterLink, Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import AuthLayout from "../components/auth/AuthLayout";
import PasswordField from "../components/auth/PasswordField";

const REMEMBERED_EMAIL = "skladisnik.rememberedEmail";

export default function LoginPage() {
  const { user, signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const remembered = localStorage.getItem(REMEMBERED_EMAIL) ?? "";

  const [email, setEmail] = useState(remembered);
  const [password, setPassword] = useState("");
  const [remember, setRemember] = useState(Boolean(remembered));
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
    <AuthLayout documentTitle="Prijava">
      <form onSubmit={handleSubmit}>
        <Typography variant="h2" gutterBottom>
          Prijava
        </Typography>
        <Typography
          variant="body2"
          sx={{
            color: "text.secondary",
            mb: 3
          }}>
          Unesite podatke svog naloga da biste nastavili.
        </Typography>

        <Stack spacing={2.5}>
          {location.state?.passwordReset && !error && (
            <Alert severity="success">Lozinka je promenjena. Prijavite se novom lozinkom.</Alert>
          )}
          {location.state?.registered && !error && (
            <Alert severity="success">
              Zahtev za nalog je poslat. Moci cete da se prijavite kada ga administrator odobri.
            </Alert>
          )}
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

          <PasswordField
            label="Lozinka"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            fullWidth
          />

          <Stack
            direction="row"
            sx={{
              justifyContent: "space-between",
              alignItems: "center"
            }}>
            <FormControlLabel
              control={
                <Checkbox size="small" checked={remember} onChange={(event) => setRemember(event.target.checked)} />
              }
              label={<Typography variant="body2">Zapamti me</Typography>}
            />
            <Link component={RouterLink} to="/zaboravljena-lozinka" variant="body2" underline="hover">
              Zaboravljena lozinka?
            </Link>
          </Stack>

          <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
            {submitting ? "Prijavljivanje..." : "Prijavi se"}
          </Button>

          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              textAlign: "center"
            }}>
            Nemate nalog?{" "}
            <Link component={RouterLink} to="/registracija" underline="hover">
              Zatrazite pristup
            </Link>
          </Typography>
        </Stack>
      </form>
    </AuthLayout>
  );
}
