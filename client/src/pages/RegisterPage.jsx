import { Alert, Button, Link, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { Link as RouterLink, Navigate, useNavigate } from "react-router-dom";
import { usersApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import AuthLayout from "../components/auth/AuthLayout";
import PasswordField from "../components/auth/PasswordField";

const MIN_PASSWORD = 8;

export default function RegisterPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirm: "",
  });
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to="/" replace />;

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const passwordTooShort = form.password.length > 0 && form.password.length < MIN_PASSWORD;
  const mismatch = form.confirm.length > 0 && form.confirm !== form.password;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    if (form.password.length < MIN_PASSWORD) {
      setError(`Lozinka mora imati najmanje ${MIN_PASSWORD} karaktera.`);
      return;
    }

    if (form.password !== form.confirm) {
      setError("Lozinke se ne poklapaju.");
      return;
    }

    setSubmitting(true);

    try {
      await usersApi.register({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
      });
      navigate("/prijava", { replace: true, state: { registered: true } });
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AuthLayout maxWidth={440}>
      <form onSubmit={handleSubmit}>
        <Typography variant="h2" gutterBottom>
          Zahtev za pristup
        </Typography>
        <Typography
          variant="body2"
          sx={{
            color: "text.secondary",
            mb: 3
          }}>
          Nalog postaje aktivan kada ga administrator odobri i dodeli vam ulogu.
        </Typography>

        <Stack spacing={2.5}>
          {error && <Alert severity="error">{error}</Alert>}

          <Stack direction="row" spacing={2}>
            <TextField label="Ime" value={form.firstName} onChange={setField("firstName")} required fullWidth autoFocus />
            <TextField label="Prezime" value={form.lastName} onChange={setField("lastName")} required fullWidth />
          </Stack>

          <TextField
            label="Email"
            type="email"
            value={form.email}
            onChange={setField("email")}
            required
            fullWidth
          />

          <PasswordField
            label="Lozinka"
            value={form.password}
            onChange={setField("password")}
            required
            fullWidth
            error={passwordTooShort}
            helperText={`Najmanje ${MIN_PASSWORD} karaktera`}
          />

          <PasswordField
            label="Potvrda lozinke"
            value={form.confirm}
            onChange={setField("confirm")}
            required
            fullWidth
            error={mismatch}
            helperText={mismatch ? "Lozinke se ne poklapaju" : " "}
          />

          <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
            {submitting ? "Slanje..." : "Posalji zahtev"}
          </Button>

          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              textAlign: "center"
            }}>
            Vec imate nalog?{" "}
            <Link component={RouterLink} to="/prijava" underline="hover">
              Prijavite se
            </Link>
          </Typography>
        </Stack>
      </form>
    </AuthLayout>
  );
}
