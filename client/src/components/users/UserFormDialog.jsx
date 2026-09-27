import { MenuItem, Stack, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { usersApi } from "../../api/endpoints";
import PasswordField from "../auth/PasswordField";
import FormDialog from "../common/FormDialog";

const emptyForm = { firstName: "", lastName: "", email: "", password: "", roleId: "", isActive: true };

export default function UserFormDialog({ open, user, roles, isSelf = false, onClose, onSaved }) {
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setForm(
      user
        ? {
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            password: "",
            roleId: user.roleId,
            isActive: user.isActive,
          }
        : emptyForm
    );
  }, [open, user]);

  const setField = (field) => (event) =>
    setForm((current) => ({ ...current, [field]: event.target.value }));

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      if (user) {
        await usersApi.update(user.id, {
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          roleId: Number(form.roleId),
          isActive: form.isActive,
        });
      } else {
        await usersApi.create({
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          email: form.email.trim(),
          password: form.password,
          roleId: Number(form.roleId),
        });
      }
      onSaved(user ? "Korisnik je izmenjen." : "Korisnik je kreiran.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <FormDialog
      open={open}
      title={user ? "Izmena korisnika" : "Novi korisnik"}
      error={error}
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
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
        disabled={Boolean(user)}
        helperText={user ? "Email se ne menja jer je korisnicko ime" : " "}
      />

      {!user && (
        <PasswordField
          label="Pocetna lozinka"
          value={form.password}
          onChange={setField("password")}
          required
          fullWidth
          helperText="Najmanje 8 karaktera. Saopstite je korisniku licno."
        />
      )}

      <Stack direction="row" spacing={2}>
        <TextField
          select
          label="Uloga"
          value={form.roleId}
          onChange={setField("roleId")}
          required
          fullWidth
          disabled={isSelf}
          helperText={isSelf ? "Sopstvenu ulogu ne mozete menjati" : " "}
        >
          {roles.map((role) => (
            <MenuItem key={role.id} value={role.id}>
              {role.name}
            </MenuItem>
          ))}
        </TextField>

        {user && (
          <TextField
            select
            label="Status"
            value={form.isActive}
            onChange={(event) =>
              setForm((current) => ({ ...current, isActive: event.target.value === "true" }))
            }
            sx={{ width: 200 }}
            disabled={isSelf}
            helperText=" "
          >
            <MenuItem value="true">Aktivan</MenuItem>
            <MenuItem value="false">Neaktivan</MenuItem>
          </TextField>
        )}
      </Stack>
    </FormDialog>
  );
}
