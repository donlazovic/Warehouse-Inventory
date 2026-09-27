import { Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { usersApi } from "../../api/endpoints";
import PasswordField from "../auth/PasswordField";
import FormDialog from "../common/FormDialog";

export default function ResetPasswordDialog({ user, onClose, onSaved }) {
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    setPassword("");
    setError(null);
  }, [user]);

  if (!user) return null;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await usersApi.resetPassword(user.id, password);
      onSaved("Lozinka je promenjena. Korisnik je odjavljen sa svih uredjaja.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open
      title="Nova lozinka"
      submitLabel="Postavi lozinku"
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Typography variant="body2">
        Postavljate novu lozinku za{" "}
        <strong>
          {user.firstName} {user.lastName}
        </strong>
        . Sve njegove aktivne sesije bice prekinute.
      </Typography>

      <PasswordField
        label="Nova lozinka"
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        required
        fullWidth
        autoFocus
        helperText="Najmanje 8 karaktera"
      />
    </FormDialog>
  );
}
