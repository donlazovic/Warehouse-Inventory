import DarkModeOutlinedIcon from "@mui/icons-material/DarkModeOutlined";
import LightModeOutlinedIcon from "@mui/icons-material/LightModeOutlined";
import SettingsBrightnessOutlinedIcon from "@mui/icons-material/SettingsBrightnessOutlined";
import {
  Alert,
  Avatar,
  Box,
  Button,
  Chip,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { authApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";
import { useColorMode } from "../ColorMode";
import PasswordField from "../components/auth/PasswordField";
import PageHeader from "../components/common/PageHeader";
import { useToast } from "../components/common/Toast";
import { monoFont } from "../theme";

const MIN_PASSWORD = 8;

const moduleLabels = {
  products: "Proizvodi",
  categories: "Kategorije",
  stock: "Zalihe",
  orders: "Nalozi",
  suppliers: "Dobavljaci",
  stores: "Prodajni objekti",
  reports: "Izvestaji",
  users: "Korisnici",
};

const actionLabels = {
  view: "pregled",
  create: "kreiranje",
  update: "izmena",
  delete: "brisanje",
  approve: "odobravanje",
  execute: "realizacija",
  export: "izvoz",
  issue: "izlaz robe",
};

function Section({ title, description, children }) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, md: 3 } }}>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "260px 1fr" },
          gap: { xs: 2, md: 4 },
        }}
      >
        <Box>
          <Typography variant="h3">{title}</Typography>
          {description && (
            <Typography variant="body2" sx={{ color: "text.secondary", mt: 0.75 }}>
              {description}
            </Typography>
          )}
        </Box>
        <Box sx={{ minWidth: 0 }}>{children}</Box>
      </Box>
    </Paper>
  );
}

function ProfileSection() {
  const { user, updateUser } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({ firstName: "", lastName: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setForm({ firstName: user?.firstName ?? "", lastName: user?.lastName ?? "" });
  }, [user?.firstName, user?.lastName]);

  const changed = form.firstName.trim() !== user?.firstName || form.lastName.trim() !== user?.lastName;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError(null);

    try {
      updateUser(await authApi.updateProfile({ firstName: form.firstName.trim(), lastName: form.lastName.trim() }));
      toast.success("Profil je sacuvan.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const initials = `${form.firstName[0] ?? ""}${form.lastName[0] ?? ""}`.toUpperCase() || "?";

  return (
    <Section title="Profil" description="Ime i prezime se prikazuju u nalozima, istoriji statusa i kretanjima robe.">
      <Box component="form" onSubmit={handleSubmit}>
        <Stack direction="row" sx={{ alignItems: "center", gap: 2, mb: 3 }}>
          <Avatar sx={{ width: 56, height: 56, bgcolor: "avatar.strong", color: "#FFFFFF", fontWeight: 600 }}>
            {initials}
          </Avatar>
          <Box>
            <Typography sx={{ fontWeight: 600 }}>
              {user?.firstName} {user?.lastName}
            </Typography>
            <Stack direction="row" sx={{ alignItems: "center", gap: 1, mt: 0.5 }}>
              <Chip size="small" label={user?.role} color="primary" variant="outlined" />
              {user?.isOwner && <Chip size="small" label="Vlasnik sistema" variant="outlined" />}
            </Stack>
          </Box>
        </Stack>

        <Stack sx={{ gap: 2, maxWidth: 520 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <Stack direction={{ xs: "column", sm: "row" }} sx={{ gap: 2 }}>
            <TextField
              label="Ime"
              value={form.firstName}
              onChange={(event) => setForm((current) => ({ ...current, firstName: event.target.value }))}
              required
              fullWidth
            />
            <TextField
              label="Prezime"
              value={form.lastName}
              onChange={(event) => setForm((current) => ({ ...current, lastName: event.target.value }))}
              required
              fullWidth
            />
          </Stack>
          <TextField
            label="Email"
            value={user?.email ?? ""}
            disabled
            fullWidth
            helperText="Email je korisnicko ime za prijavu i menja ga administrator."
          />
          <Box>
            <Button type="submit" variant="contained" disabled={!changed || saving}>
              {saving ? "Cuvanje..." : "Sacuvaj profil"}
            </Button>
          </Box>
        </Stack>
      </Box>
    </Section>
  );
}

function PasswordSection() {
  const { applySession } = useAuth();
  const toast = useToast();
  const empty = { current: "", next: "", confirm: "" };
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const set = (field) => (event) => setForm((current) => ({ ...current, [field]: event.target.value }));
  const tooShort = form.next.length > 0 && form.next.length < MIN_PASSWORD;
  const mismatch = form.confirm.length > 0 && form.confirm !== form.next;

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    if (form.next.length < MIN_PASSWORD) return setError(`Nova lozinka mora imati najmanje ${MIN_PASSWORD} karaktera.`);
    if (form.next !== form.confirm) return setError("Nove lozinke se ne poklapaju.");

    setSaving(true);

    try {
      applySession(await authApi.changePassword(form.current, form.next));
      setForm(empty);
      toast.success("Lozinka je promenjena. Odjavljeni ste sa ostalih uredjaja.");
    } catch (err) {
      setError(err.response?.status === 429 ? "Previse pokusaja. Sacekajte minut." : err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Section
      title="Lozinka"
      description="Posle promene ostajete prijavljeni ovde, a sve ostale sesije se prekidaju. Na mejl stize potvrda."
    >
      <Stack component="form" onSubmit={handleSubmit} sx={{ gap: 2, maxWidth: 520 }}>
        {error && <Alert severity="error">{error}</Alert>}
        <PasswordField label="Trenutna lozinka" value={form.current} onChange={set("current")} required fullWidth />
        <PasswordField
          label="Nova lozinka"
          value={form.next}
          onChange={set("next")}
          required
          fullWidth
          error={tooShort}
          helperText={`Najmanje ${MIN_PASSWORD} karaktera`}
        />
        <PasswordField
          label="Potvrda nove lozinke"
          value={form.confirm}
          onChange={set("confirm")}
          required
          fullWidth
          error={mismatch}
          helperText={mismatch ? "Lozinke se ne poklapaju" : " "}
        />
        <Box>
          <Button type="submit" variant="contained" disabled={saving || !form.current || !form.next || !form.confirm}>
            {saving ? "Cuvanje..." : "Promeni lozinku"}
          </Button>
        </Box>
      </Stack>
    </Section>
  );
}

function AppearanceSection() {
  const { preference, setPreference } = useColorMode();

  return (
    <Section title="Izgled" description="„Sistem“ prati podesavanje operativnog sistema i menja se zajedno sa njim.">
      <ToggleButtonGroup
        exclusive
        value={preference}
        onChange={(_, next) => next && setPreference(next)}
        sx={{ flexWrap: "wrap" }}
      >
        <ToggleButton value="light" sx={{ gap: 1, px: 2.5 }}>
          <LightModeOutlinedIcon fontSize="small" /> Svetli
        </ToggleButton>
        <ToggleButton value="dark" sx={{ gap: 1, px: 2.5 }}>
          <DarkModeOutlinedIcon fontSize="small" /> Tamni
        </ToggleButton>
        <ToggleButton value="system" sx={{ gap: 1, px: 2.5 }}>
          <SettingsBrightnessOutlinedIcon fontSize="small" /> Sistem
        </ToggleButton>
      </ToggleButtonGroup>
    </Section>
  );
}

function PermissionsSection() {
  const { user } = useAuth();

  const grouped = useMemo(() => {
    const result = {};
    (user?.permissions ?? []).forEach((code) => {
      const [module, action] = code.split(".");
      (result[module] ??= []).push(action);
    });
    return Object.entries(result).sort(([a], [b]) => a.localeCompare(b));
  }, [user?.permissions]);

  return (
    <Section
      title="Moje dozvole"
      description={`Dozvole uloge ${user?.role ?? ""}. Menja ih administrator na stranici Korisnici i uloge.`}
    >
      {grouped.length === 0 ? (
        <Typography variant="body2" sx={{ color: "text.secondary" }}>
          Nemate dodeljenih dozvola.
        </Typography>
      ) : (
        <Stack sx={{ gap: 1.5 }}>
          {grouped.map(([module, actions]) => (
            <Stack
              key={module}
              direction={{ xs: "column", sm: "row" }}
              sx={{ gap: { xs: 0.75, sm: 2 }, alignItems: { sm: "center" } }}
            >
              <Typography variant="body2" sx={{ width: 150, flexShrink: 0, fontWeight: 500 }}>
                {moduleLabels[module] ?? module}
              </Typography>
              <Stack direction="row" sx={{ gap: 0.75, flexWrap: "wrap" }}>
                {actions.map((action) => (
                  <Chip
                    key={action}
                    size="small"
                    variant="outlined"
                    label={actionLabels[action] ?? action}
                    title={`${module}.${action}`}
                    sx={{ fontFamily: monoFont, fontSize: "0.72rem" }}
                  />
                ))}
              </Stack>
            </Stack>
          ))}
        </Stack>
      )}
    </Section>
  );
}

export default function SettingsPage() {
  return (
    <Box>
      <PageHeader title="Podesavanja" description="Vas nalog, lozinka i izgled aplikacije." />
      <Stack sx={{ gap: 2, maxWidth: 1100 }}>
        <ProfileSection />
        <PasswordSection />
        <AppearanceSection />
        <PermissionsSection />
      </Stack>
    </Box>
  );
}
