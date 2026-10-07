import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  Stack,
} from "@mui/material";
import DialogHeader from "./DialogHeader";

export default function FormDialog({
  open,
  title,
  submitLabel = "Sacuvaj",
  error,
  submitting = false,
  maxWidth = "sm",
  onSubmit,
  onClose,
  children,
}) {
  return (
    <Dialog open={open} onClose={submitting ? undefined : onClose} maxWidth={maxWidth} fullWidth>
      <form onSubmit={onSubmit}>
        <DialogHeader title={title} onClose={onClose} disabled={submitting} />
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            {children}
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} disabled={submitting}>
            Odustani
          </Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            {submitting ? "Cuvanje..." : submitLabel}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
