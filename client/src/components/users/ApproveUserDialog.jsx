import { MenuItem, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { usersApi } from "../../api/endpoints";
import FormDialog from "../common/FormDialog";

export default function ApproveUserDialog({ user, roles, onClose, onSaved }) {
  const [roleId, setRoleId] = useState("");
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    setRoleId("");
    setError(null);
  }, [user]);

  if (!user) return null;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await usersApi.approve(user.id, Number(roleId));
      onSaved(`Nalog ${user.firstName} ${user.lastName} je odobren.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open
      title="Odobravanje naloga"
      submitLabel="Odobri nalog"
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Typography variant="body2">
        <strong>
          {user.firstName} {user.lastName}
        </strong>{" "}
        ({user.email}) trazi pristup sistemu. Izaberite ulogu koju ce imati nakon odobrenja.
      </Typography>

      <TextField
        select
        label="Uloga"
        value={roleId}
        onChange={(event) => setRoleId(event.target.value)}
        required
        fullWidth
        autoFocus
      >
        {roles.map((role) => (
          <MenuItem key={role.id} value={role.id}>
            {role.name}
            {role.description ? ` — ${role.description}` : ""}
          </MenuItem>
        ))}
      </TextField>
    </FormDialog>
  );
}
