import { Alert, Button, Dialog, DialogActions, DialogContent, Stack } from "@mui/material";
import { useEffect, useState } from "react";
import ConfirmDialog from "./ConfirmDialog";
import DialogHeader from "./DialogHeader";

export default function FormDialog({
  open,
  title,
  submitLabel = "Sacuvaj",
  error,
  submitting = false,
  maxWidth = "sm",
  guardChanges = true,
  onSubmit,
  onClose,
  children,
}) {
  const [dirty, setDirty] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);

  useEffect(() => {
    if (open) {
      setDirty(false);
      setConfirmOpen(false);
    }
  }, [open]);

  const markDirty = () => {
    if (!dirty) setDirty(true);
  };

  const requestClose = () => {
    if (submitting) return;
    if (guardChanges && dirty) setConfirmOpen(true);
    else onClose();
  };

  return (
    <Dialog open={open} onClose={requestClose} maxWidth={maxWidth} fullWidth>
      <form
        onSubmit={onSubmit}
        onChange={markDirty}
        onClickCapture={(event) => {
          if (event.target.closest?.('[role="option"]')) markDirty();
        }}
      >
        <DialogHeader title={title} onClose={requestClose} disabled={submitting} />
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            {children}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={requestClose} disabled={submitting}>
            Odustani
          </Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            {submitting ? "Cuvanje..." : submitLabel}
          </Button>
        </DialogActions>
      </form>

      <ConfirmDialog
        open={confirmOpen}
        title="Odbaciti izmene?"
        message="Uneli ste podatke koji nisu sacuvani. Ako zatvorite formu, bice izgubljeni."
        confirmLabel="Odbaci izmene"
        cancelLabel="Nastavi uredjivanje"
        destructive
        onConfirm={() => {
          setConfirmOpen(false);
          onClose();
        }}
        onClose={() => setConfirmOpen(false)}
      />
    </Dialog>
  );
}
