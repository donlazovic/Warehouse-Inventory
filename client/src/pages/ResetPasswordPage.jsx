import LinkOffOutlinedIcon from "@mui/icons-material/LinkOffOutlined";
import { Alert, Box, Button, CircularProgress, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { Link as RouterLink, Navigate, useNavigate, useSearchParams } from "react-router-dom";
import { authApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import AuthLayout from "../components/auth/AuthLayout";
import PasswordField from "../components/auth/PasswordField";

const MIN_PASSWORD = 8;

export default function ResetPasswordPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const [status, setStatus] = useState("checking");
  const [maskedEmail, setMaskedEmail] = useState(null);
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!token) {
      setStatus("invalid");
      return;
    }

    authApi
      .checkResetToken(token)
      .then((result) => {
        setStatus(result.isValid ? "valid" : "invalid");
        setMaskedEmail(result.email);
      })
      .catch(() => setStatus("invalid"));
  }, [token]);

  if (user) return <Navigate to="/" replace />;

  const tooShort = password.length > 0 && password.length < MIN_PASSWORD;
  const mismatch = confirm.length > 0 && confirm !== password;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    if (password.length < MIN_PASSWORD) {
      setError(`Lozinka mora imati najmanje ${MIN_PASSWORD} karaktera.`);
      return;
    }

    if (password !== confirm) {
      setError("Lozinke se ne poklapaju.");
      return;
    }

    setSubmitting(true);

    try {
      await authApi.resetPassword(token, password);
      navigate("/prijava", { replace: true, state: { passwordReset: true } });
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AuthLayout>
      {status === "checking" && (
        <Box sx={{ display: "grid", placeItems: "center", py: 6 }}>
          <CircularProgress size={28} />
        </Box>
      )}

      {status === "invalid" && (
        <Stack spacing={2.5} sx={{
          alignItems: "flex-start"
        }}>
          <Box sx={{ color: "warning.main" }}>
            <LinkOffOutlinedIcon sx={{ fontSize: 40 }} />
          </Box>
          <Typography variant="h2">Link vise ne vazi</Typography>
          <Typography variant="body2" sx={{
            color: "text.secondary"
          }}>
            Link za promenu lozinke je istekao, vec je iskoriscen, ili je zamenjen novijim zahtevom.
            Zatrazite novi — stize za nekoliko sekundi.
          </Typography>
          <Stack direction="row" spacing={1}>
            <Button component={RouterLink} to="/zaboravljena-lozinka" variant="contained">
              Zatrazi novi link
            </Button>
            <Button component={RouterLink} to="/prijava">
              Prijava
            </Button>
          </Stack>
        </Stack>
      )}

      {status === "valid" && (
        <form onSubmit={handleSubmit}>
          <Typography variant="h2" gutterBottom>
            Nova lozinka
          </Typography>
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mb: 3
            }}>
            {maskedEmail ? `Postavljate novu lozinku za nalog ${maskedEmail}.` : "Postavite novu lozinku."} Posle
            promene bicete odjavljeni sa svih uredjaja.
          </Typography>

          <Stack spacing={2.5}>
            {error && <Alert severity="error">{error}</Alert>}

            <PasswordField
              label="Nova lozinka"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              autoFocus
              fullWidth
              error={tooShort}
              helperText={`Najmanje ${MIN_PASSWORD} karaktera`}
            />

            <PasswordField
              label="Potvrda nove lozinke"
              value={confirm}
              onChange={(event) => setConfirm(event.target.value)}
              required
              fullWidth
              error={mismatch}
              helperText={mismatch ? "Lozinke se ne poklapaju" : " "}
            />

            <Button type="submit" variant="contained" size="large" disabled={submitting} fullWidth>
              {submitting ? "Cuvanje..." : "Postavi lozinku"}
            </Button>
          </Stack>
        </form>
      )}
    </AuthLayout>
  );
}
